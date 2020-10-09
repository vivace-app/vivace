using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PlayScreenProcessManager : MonoBehaviour
{
    public GameObject[] Note;
    public Text ComboText, ScoreText, JudgeText, AddText;
    public Tweener JTextFade, JTextReduce, ATextFade, ATextReduce; //消失/縮小(判定表示)，消失/縮小(加算表示)アニメーションにTweenerを付与
    private PlayScreenProcessManager _gameManager;

    private AudioSource _audioSource;
    private AudioClip _music;
    private string Perfect16 = "#FF7DF2", Great16 = "#FF9C7D", Good16 = "#34E045", Miss16 = "#8D8D8D", Score16 = "#6C95FF"; //ResultScreenの色を拝借
    private Color Perfect_c, Great_c, Good_c, Miss_c, Score_c;
    private int _notesTotal = 0;
    private int _notesCount = 0;
    private float _startTime = 0;
    private float _stoptime = 0; //ポーズ時間を計測
    private float[] _timing;
    private int[] _lineNum;
    private static AudioSource[] _SoundEffects; //効果音用変数
    public static bool _isPlaying = true; //プレイ中がポーズ中かを判定
    private static bool playedFlag = false; //楽曲の再生が一度でも開始されたかどうかを判定
    public static bool _autoPlay = false; //自動プレイ用
    private int _combo = 0; //コンボ数
    private int _perfects = 0, _greats = 0, _goods = 0, _misss = 0;
    public static int res_perfects = 0, res_greats = 0, res_goods = 0, res_misss = 0; //リザルト画面用
    private double _score = 0; //得点
    public static double res_score = 0; //リザルト画面用
    private double _basescore = 0; //基礎点:ノーツ1つあたりのスコア
    public static float _notesSpeedIndex = 5.0f; //ノーツ落下速度の設定用(1.0f~10.0fまで動作確認)
    private int JTextUsed = 0; //JudgeTextのtextを変更した回数
    private float delay_time = 0; //遅延開始時間の計算用

    // ------------------------------------------------------------------------------------

    static readonly string registScoreApiUri = EnvDataStore.registScoreApiUri;
    static readonly string[] musicTitles = MusicTitleDataStore.musicTitles;
    private string csvFilePass = "CSV/" + musicTitles[SwipeMenu.selectedNumTmp] + "_" + SelectScreenProcessManager.selectedLevel;

    // ------------------------------------------------------------------------------------


    [Serializable]
    public class RegistScoreResponse
    {
        public bool success;
        public string msg;
    }

    async void Start()
    {
        _timing = new float[1024];
        _lineNum = new int[1024];
        ComboText.text = _combo.ToString("D");
        ScoreText.text = ((int)Math.Round(_score, 0, MidpointRounding.AwayFromZero)).ToString("D7");
        JudgeText.text = "";
        AddText.text = "";
        ColorUtility.TryParseHtmlString(Perfect16, out Perfect_c);
        ColorUtility.TryParseHtmlString(Great16, out Great_c);
        ColorUtility.TryParseHtmlString(Good16, out Good_c);
        ColorUtility.TryParseHtmlString(Miss16, out Miss_c);
        ColorUtility.TryParseHtmlString(Score16, out Score_c);
        AdjustJudgeRange(); //ノーツ落下速度に合わせて判定オブジェクトの高さを変化
        _music = Resources.Load<AudioClip>("music/BurningHeart");
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.clip = _music;
        _SoundEffects = GameObject.Find("SoundEffect").GetComponents<AudioSource>();
        _gameManager = GameObject.Find("ProcessManager").GetComponent<PlayScreenProcessManager>();
        delay_time = 12800 / _notesSpeedIndex; //遅延開始時間の計算
        await Task.Delay(1000); //処理落ちによるトラブル防止
        LoadCSV();
        _startTime = Time.time;
        await Task.Delay((int)delay_time); //開始タイミングの調整
        _audioSource.Play();
        await Task.Delay(10); //playedFlagの変数代入をずらして誤作動を防止
        playedFlag = true; //楽曲が1回以上再生されたことを確認
    }

    void Update()
    {
        if (_isPlaying) CheckNextNotes();
        if (playedFlag && !_audioSource.isPlaying)
        {
            playedFlag = false; //何度も処理が呼び出されないようにする
            ChangeScene();
        }
    }

    void LoadCSV()
    {
        TextAsset csv = Resources.Load(csvFilePass) as TextAsset;
        StringReader reader = new StringReader(csv.text);
        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();
            string[] values = line.Split(',');
            _timing[_notesTotal] = float.Parse(values[0]);
            _lineNum[_notesTotal++] = int.Parse(values[1]);
        }

        if (_notesTotal >= 30)
        { //コンボ数が30以上のとき
            _basescore = 1000000 / ((double)_notesTotal - 15); //基礎点は1000000点を最大コンボ数-15で割った値
        }
        else
        { //コンボ数が30未満のとき
            _basescore = 1000000 / (double)_notesTotal; // 基礎点は1000000点を最大コンボ数で割った値
        }
    }

    public async void AddScore(int num)
    { //加点のための関数,引数magniは判定ごとのスコア倍率
        double magni = 0, scoreTemp = 0, JTextTemp = ++JTextUsed;
        Vector3 vl = new Vector3(1.5f, 1.5f, 1.5f);
        Vector3 vo = Vector3.one;
        if (JTextFade != null)
        {
            JTextFade.Kill(); //もし現在進行形で 消失 アニメーション中なら中断
        }
        if (JTextReduce != null)
        {
            JTextReduce.Kill(); //もし現在進行形で 縮小 アニメーション中なら中断
        }
        if (ATextFade != null)
        {
            ATextFade.Kill(); //もし現在進行形で 消失 アニメーション中なら中断
        }
        if (ATextReduce != null)
        {
            ATextReduce.Kill(); //もし現在進行形で 縮小 アニメーション中なら中断
        }
        switch (num)
        { //0：Perfect，1：Great，2：Good，3：Miss
            case 0:
                magni = 1;
                _combo++;
                _perfects++;
                JudgeText.transform.localScale = vl; //テキストサイズを全方向1.5倍化
                JTextReduce = JudgeText.transform.DOScale(vo, 0.2f); //元の大きさまでの縮小アニメーション
                JudgeText.color = Perfect_c;
                JudgeText.text = "Perfect!";
                break;
            case 1:
                magni = 0.75;
                _combo++;
                _greats++;
                JudgeText.transform.localScale = vl; //テキストサイズを全方向1.5倍化
                JTextReduce = JudgeText.transform.DOScale(vo, 0.2f); //元の大きさまでの縮小アニメーション
                JudgeText.color = Great_c;
                JudgeText.text = "Great!";
                break;
            case 2:
                magni = 0.25;
                _combo = 0;
                _goods++;
                JudgeText.transform.localScale = vo; //テキストサイズを標準化
                JudgeText.color = Good_c;
                JudgeText.text = "Good!";
                break;
            case 3:
                magni = 0;
                _combo = 0;
                _misss++;
                JudgeText.transform.localScale = vo; //テキストサイズを標準化
                JudgeText.color = Miss_c;
                JudgeText.text = "Miss!";
                break;
        }
        ComboText.text = _combo.ToString("D");
        if (_notesTotal >= 30)
        { //コンボ数が30以上のときにはスコアは以下の通り傾斜加算
            if (_combo <= 10) //コンボ数が10以下のとき
                scoreTemp = _basescore * 0.25 * magni; //スコアに基礎点*倍率の25％を加算
            else if (_combo <= 20) //コンボ数が20以下のとき
                scoreTemp = _basescore * 0.5 * magni; //スコアに基礎点*倍率の50％を加算
            else if (_combo <= 30) //コンボ数が30以下のとき
                scoreTemp = _basescore * 0.75 * magni; //スコアに基礎点*倍率の75％を加算
            else //コンボ数が31以上のとき
                scoreTemp = _basescore * magni; //スコアに基礎点*倍率を加算
        }
        else
        { //コンボ数が30未満のときは単に基礎点*倍率を加算
            scoreTemp = _basescore * magni;
        }
        AddText.color = Score_c;
        AddText.text = "+" + ((int)Math.Round(scoreTemp, 0, MidpointRounding.AwayFromZero)).ToString("D"); //四捨五入して型変換を行い加算スコアを表示
        AddText.transform.localScale = vl; //テキストサイズを全方向1.5倍化
        ATextReduce = AddText.transform.DOScale(vo, 0.2f); //元の大きさまでの縮小アニメーション

        for (int i = 0; i < 15; i++) //15分割したものを33ミリ秒ごとに15回加算()
        {
            _score += scoreTemp / 15;
            ScoreText.text = ((int)Math.Round(_score, 0, MidpointRounding.AwayFromZero)).ToString("D7"); //四捨五入して型変換を行い表示を更新
            await Task.Delay(33);
        }
        await Task.Delay(250);
        if (JTextUsed == JTextTemp) //もし次のAddScoreが読み込まれていなければ
        {
            JTextFade = DOTween.ToAlpha(() => JudgeText.color, cchanger => JudgeText.color = cchanger, 0.0f, 0.2f); //文字の消失アニメーション
            ATextFade = DOTween.ToAlpha(() => AddText.color, cchanger => AddText.color = cchanger, 0.0f, 0.2f); //文字の消失アニメーション
        }
    }

    void CheckNextNotes()
    {
        while (_timing[_notesCount] < (Time.time - _startTime) && _timing[_notesCount] != 0) SpawnNotes(_lineNum[_notesCount++]);
    }

    void SpawnNotes(int num)
    {
        Instantiate(Note[num], new Vector3(-0.676f + (0.338f * num), 8.4f, 4.5f), Quaternion.Euler(-30f, 0, 0));
    }

    public void SoundEffect(int num)
    {
        _SoundEffects[num].PlayOneShot(_SoundEffects[num].clip);
    }

    public async void Pause()
    {
        if (_isPlaying)
        {
            _isPlaying = false; //ポーズ中であることを変数に代入
            _stoptime = Time.time; //ポーズした時刻を代入
            _audioSource.pitch = 0.0f; //楽曲再生速度を0にする(Pauseを使うと楽曲終了判定が誤作動するため)
        }
        else if (!_isPlaying)
        {
            for (int i = 3; i > 0; i--) //再開のカウントダウン
            {
                await Task.Delay(1000);
                SoundEffect(2);
            }
            await Task.Delay(1000);
            _isPlaying = true;
            _startTime = _startTime + (Time.time - _stoptime); //開始時間をポーズ時間分加算
            await Task.Delay(10); //ノーツと楽曲がずれるのを補正
            _audioSource.pitch = 1.0f; //楽曲再生速度を1にする(等速再生)
        }
    }

    public void PerfectTimingFunc(int num)
    {
        SoundEffect(0);
        _gameManager.AddScore(0);
    }

    public void GreatTimingFunc(int num)
    {
        SoundEffect(1);
        _gameManager.AddScore(1);
    }

    public void GoodTimingFunc(int num)
    {
        SoundEffect(2);
        _gameManager.AddScore(2);
    }

    public void MissTimingFunc(int num)
    {
        _gameManager.AddScore(3);
    }

    public void AdjustJudgeRange()
    {
        Transform PRange = GameObject.Find("PerfectJudgeLine").GetComponent<Transform>();
        PRange.transform.localScale = new Vector3(1.8f, 0.1f, _notesSpeedIndex * 0.08f); //Perfect判定オブジェクトの高さを変更
        Transform GrRange = GameObject.Find("GreatJudgeLine").GetComponent<Transform>();
        GrRange.transform.localScale = new Vector3(1.8f, 0.1f, _notesSpeedIndex * 0.14f); //Great判定オブジェクトの高さを変更
        Transform GoRange = GameObject.Find("GoodJudgeLine").GetComponent<Transform>();
        GoRange.transform.localScale = new Vector3(1.8f, 0.1f, _notesSpeedIndex * 0.2f); //Good判定オブジェクトの高さを変更
    }

    public async void ChangeScene()
    {
        res_score = _score;
        res_perfects = _perfects;
        res_greats = _greats;
        res_goods = _goods;
        res_misss = _misss;
        StartCoroutine(RegistScoreNetworkProcess(musicTitles[SwipeMenu.selectedNumTmp], SelectScreenProcessManager.selectedLevel, (int)res_score));
        await Task.Delay(1000);
        SceneManager.LoadScene("ResultScene");
    }

    IEnumerator RegistScoreNetworkProcess(string selectedMusic, string selectedLevel, int score)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", PlayerPrefs.GetString("jwt"));
        form.AddField("music", selectedMusic);
        form.AddField("level", selectedLevel);
        form.AddField("score", score);
        UnityWebRequest www = UnityWebRequest.Post(registScoreApiUri, form);
        yield return www.SendWebRequest();
        if (www.isNetworkError)
        {
            Debug.LogError("ネットワークに接続できません．(" + www.error + ")");
        }
        else
        {
            ResponseCheck(www.downloadHandler.text);
        }
    }

    private void ResponseCheck(string data)
    {
        RegistScoreResponse jsnData = JsonUtility.FromJson<RegistScoreResponse>(data);

        if (jsnData.success)
            Debug.Log("スコアを登録しました");
        else
            Debug.Log("スコアの登録に失敗しました");
    }
}