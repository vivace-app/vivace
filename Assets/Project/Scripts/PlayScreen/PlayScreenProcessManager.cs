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
    // --- Attach from Unity --------------------------------------------------------------
    public GameObject[] Note; // Attach 5 notes objects.
    public RectTransform Background; // Background image for responsive.
    public Text ComboText, ScoreText, JudgeText, AddText;
<<<<<<< HEAD
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
    private double _basescore = 0, _logSqSum = 0; //基礎点:ノーツ1つあたりのスコア，スコア傾斜のlog部分の和
    private static int _sepPoint = 50; //分割点:50コンボ以降は加点数の増加なし
    private double[] _logSq = new double[_sepPoint];
    public static float _notesSpeedIndex = 5.0f; //ノーツ落下速度の設定用(1.0f~10.0fまで動作確認)
    public static int _starttimingIndex = 0; //スタートのタイミングを調整(10ms毎，正にするほど遅くなる)
    private int JTextUsed = 0; //JudgeTextのtextを変更した回数
    private float delay_time = 0; //遅延開始時間の計算用
=======
    public Tweener JTextFade, JTextReduce, ATextFade, ATextReduce; // For Tweener animation.
    // ------------------------------------------------------------------------------------
>>>>>>> [clean] deltaTime問題修正&ノーツがタッチできない問題修正&アタッチループ問題修正

    // --- External variables -------------------------------------------------------------
    public static bool _autoPlay = false;
    public static bool _isPlaying = true;
    public static float _notesSpeedIndex = 5.0f; // 1.0f ~ 10.0f
    public static int _score = 0, _perfect = 0, _great = 0, _good = 0, _miss = 0;
    public static int _startTimingIndex = 0; // Every 10ms / "+" -> slow / "-" ->fast
    public static Vector3 _deltaPosition; // Vector falling every frame.
    // ------------------------------------------------------------------------------------

    // --- Environment variables ----------------------------------------------------------
    static readonly string registScoreApiUri = EnvDataStore.registScoreApiUri;
    static readonly string[] musicTitles = MusicTitleDataStore.musicTitles;
    // ------------------------------------------------------------------------------------

    // Audio
    private AudioSource playAudioSource;
    private AudioClip playMusic;
    private AudioSource[] judgeAudioSource;

    // Color
    private Color Perfect_c, Great_c, Good_c, Miss_c, Score_c;

    // CSV data
    private int[] lineNum = new int[1024];
    private float[] timing = new float[1024];

    private bool alreadyPlayedFlag = false;
    private double score = 0, baseScore = 0;
    private float startTime = 0, stopTime = 0;
    private int combo = 0, perfect = 0, great = 0, good = 0, miss = 0, notesTotal = 0, notesCount = 0;
    private int JTextUsed = 0; // Number of times JudgeText has been changed.

    // ====================================================================================

    [Serializable]
    public class RegistScoreResponse
    {
        public bool success;
        public string msg;
    }

    // ====================================================================================

    async void Start()
    {
<<<<<<< HEAD
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
        _music = Resources.Load<AudioClip>("music/" + musicTitles[SwipeMenu.selectedNumTmp]);
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.clip = _music;
        _SoundEffects = GameObject.Find("SoundEffect").GetComponents<AudioSource>();
        _gameManager = GameObject.Find("ProcessManager").GetComponent<PlayScreenProcessManager>();
        _notesSpeedIndex = SelectScreenProcessManager._notesSpeedIndex; //SelectScreenから引っ張ってくる
        _starttimingIndex = SelectScreenProcessManager._starttimingIndex; //SelectScreenから引っ張ってくる
        delay_time = (12800 + 10 * _starttimingIndex) / _notesSpeedIndex; //遅延開始時間の計算
        await Task.Delay(1000); //処理落ちによるトラブル防止
=======
        ScreenResponsive();
        TextInitialization();
        ColorInitialization();
        AudioInitialization();
        AdjustJudgeRange();
        _deltaPosition = (Vector3.down + Vector3.back * (float)Math.Sqrt(3)) * 0.6f * _notesSpeedIndex;
        _notesSpeedIndex = SelectScreenProcessManager._notesSpeedIndex;
        _startTimingIndex = SelectScreenProcessManager._startTimingIndex;
        await Task.Delay(1000);
>>>>>>> [clean] deltaTime問題修正&ノーツがタッチできない問題修正&アタッチループ問題修正
        LoadCSV();
        BaseScoreDecision();
        startTime = Time.time;
        await Task.Delay((int)((12800 + 10 * _startTimingIndex) / _notesSpeedIndex));
        playAudioSource.Play();
        await Task.Delay(10);
        alreadyPlayedFlag = true;
    }

    void Update()
    {
        if (_isPlaying) CheckNextNotes();
        if (alreadyPlayedFlag && !playAudioSource.isPlaying)
        {
            alreadyPlayedFlag = false;
            ChangeScene();
        }
    }

<<<<<<< HEAD
    void LoadCSV()
=======
    private void ScreenResponsive()
    {
        float scale = 1f;
        if (Screen.width < Screen.height)
            scale = (Screen.height * 16) / (Screen.width * 9);
        Background.sizeDelta = new Vector2(Screen.width * scale, Screen.height * scale);
    }

    private void TextInitialization()
    {
        AddText.text = "";
        JudgeText.text = "";
        ComboText.text = combo.ToString("D");
        ScoreText.text = ((int)Math.Round(score, 0, MidpointRounding.AwayFromZero)).ToString("D7");
    }

    private void ColorInitialization()
    {
        ColorUtility.TryParseHtmlString("#FF7DF2", out Perfect_c);
        ColorUtility.TryParseHtmlString("#FF9C7D", out Great_c);
        ColorUtility.TryParseHtmlString("#34E045", out Good_c);
        ColorUtility.TryParseHtmlString("#8D8D8D", out Miss_c);
        ColorUtility.TryParseHtmlString("#6C95FF", out Score_c);
    }

    private void AudioInitialization()
    {
        judgeAudioSource = GameObject.Find("SoundEffect").GetComponents<AudioSource>();
        playMusic = Resources.Load<AudioClip>("music/" + musicTitles[SwipeMenu.selectedNumTmp]);
        playAudioSource = gameObject.AddComponent<AudioSource>();
        playAudioSource.clip = playMusic;
    }

    private void AdjustJudgeRange()
>>>>>>> [clean] deltaTime問題修正&ノーツがタッチできない問題修正&アタッチループ問題修正
    {
        Transform PerfectRange = GameObject.Find("PerfectJudgeLine").GetComponent<Transform>();
        PerfectRange.transform.localScale = new Vector3(1.8f, 0.1f, _notesSpeedIndex * 0.12f);
        Transform GreatRange = GameObject.Find("GreatJudgeLine").GetComponent<Transform>();
        GreatRange.transform.localScale = new Vector3(1.8f, 0.1f, _notesSpeedIndex * 0.18f);
        Transform GoodRange = GameObject.Find("GoodJudgeLine").GetComponent<Transform>();
        GoodRange.transform.localScale = new Vector3(1.8f, 0.1f, _notesSpeedIndex * 0.2f);
    }

    private void LoadCSV()
    {
        TextAsset csv = Resources.Load("CSV/" + musicTitles[SwipeMenu.selectedNumTmp] + "_" + SelectScreenProcessManager.selectedLevel) as TextAsset;
        StringReader reader = new StringReader(csv.text);
        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();
            string[] values = line.Split(',');
            timing[notesTotal] = float.Parse(values[0]);
            lineNum[notesTotal++] = int.Parse(values[1]);
        }
    }

<<<<<<< HEAD
        if (_notesTotal >= _sepPoint)
        { //コンボ数が_sepPoint以上のとき
            for (int i = 0; i < _sepPoint; i++)
            {
                _logSq[i] = Math.Log10(1 + (9 * ((double)i + 1) / (double)_sepPoint));
                _logSqSum += _logSq[i];
            }
            _basescore = 1000000 / (_logSqSum + (double)_notesTotal - (double)_sepPoint); //基礎点は1000000点をlog部分の和+最大コンボ数-_sepPointで割った値
        }
        else
        { //コンボ数が_sepPoint未満のとき
            for (int i = 0; i < _notesTotal; i++)
            {
                _logSq[i] = Math.Log10(1 + (9 * ((double)i + 1) / (double)_notesTotal));
                _logSqSum += _logSq[i];
            }
            _basescore = 1000000 / _logSqSum; // 基礎点は1000000点をlog部分の和で割った値
=======
    private void BaseScoreDecision()
    {
        if (notesTotal >= 30)
            baseScore = 1000000 / ((double)notesTotal - 15);
        else
            baseScore = 1000000 / (double)notesTotal;
    }

    void CheckNextNotes()
    {
        while (timing[notesCount] < (Time.time - startTime) && timing[notesCount] != 0) SpawnNotes(lineNum[notesCount++]);
    }

    void SpawnNotes(int num)
    {
        Instantiate(Note[num], new Vector3(-0.676f + (0.338f * num), 8.4f, 4.5f), Quaternion.Euler(-30f, 0, 0));
    }

    public void SoundEffect(int num)
    {
        judgeAudioSource[num].PlayOneShot(judgeAudioSource[num].clip);
    }

    public async void Pause()
    {
        if (_isPlaying)
        {
            _isPlaying = false; //ポーズ中であることを変数に代入
            stopTime = Time.time; //ポーズした時刻を代入
            playAudioSource.pitch = 0.0f; //楽曲再生速度を0にする(Pauseを使うと楽曲終了判定が誤作動するため)
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
            startTime = startTime + (Time.time - stopTime); //開始時間をポーズ時間分加算
            await Task.Delay(10); //ノーツと楽曲がずれるのを補正
            playAudioSource.pitch = 1.0f; //楽曲再生速度を1にする(等速再生)
>>>>>>> [clean] deltaTime問題修正&ノーツがタッチできない問題修正&アタッチループ問題修正
        }
    }

    public void PerfectTimingFunc(int num)
    {
        SoundEffect(0);
        AddScore(0);
    }

    public void GreatTimingFunc(int num)
    {
        SoundEffect(1);
        AddScore(1);
    }

    public void GoodTimingFunc(int num)
    {
        SoundEffect(2);
        AddScore(2);
    }

    public void MissTimingFunc(int num)
    {
        AddScore(3);
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
                combo++;
                perfect++;
                JudgeText.transform.localScale = vl; //テキストサイズを全方向1.5倍化
                JTextReduce = JudgeText.transform.DOScale(vo, 0.2f); //元の大きさまでの縮小アニメーション
                JudgeText.color = Perfect_c;
                JudgeText.text = "Perfect!";
                break;
            case 1:
                magni = 0.75;
                combo++;
                great++;
                JudgeText.transform.localScale = vl; //テキストサイズを全方向1.5倍化
                JTextReduce = JudgeText.transform.DOScale(vo, 0.2f); //元の大きさまでの縮小アニメーション
                JudgeText.color = Great_c;
                JudgeText.text = "Great!";
                break;
            case 2:
                magni = 0.25;
                combo = 0;
                good++;
                JudgeText.transform.localScale = vo; //テキストサイズを標準化
                JudgeText.color = Good_c;
                JudgeText.text = "Good!";
                break;
            case 3:
                magni = 0;
                combo = 0;
                miss++;
                JudgeText.transform.localScale = vo; //テキストサイズを標準化
                JudgeText.color = Miss_c;
                JudgeText.text = "Miss!";
                break;
        }
<<<<<<< HEAD
        ComboText.text = _combo.ToString("D");
        if (_combo > 0)
        {
            if (_combo <= _sepPoint) //コンボ数が_sepPoint以下のとき
                scoreTemp = _basescore * _logSq[_combo - 1] * magni; //スコアに基礎点*log傾斜*倍率加算
            else //コンボ数が_sepPoint超過のとき
                scoreTemp = _basescore * magni; //スコアに基礎点*倍率を加算
        }
        else
        {
            if (_combo <= _sepPoint) //コンボ数が_sepPoint以下のとき
                scoreTemp = _basescore * _logSq[0] * magni; //スコアに基礎点*log傾斜(1コンボ時を利用)*倍率加算
            else //コンボ数が_sepPoint超過のとき
                scoreTemp = _basescore * magni; //スコアに基礎点*倍率を加算
=======
        ComboText.text = combo.ToString("D");
        if (notesTotal >= 30)
        { //コンボ数が30以上のときにはスコアは以下の通り傾斜加算
            if (combo <= 10) //コンボ数が10以下のとき
                scoreTemp = baseScore * 0.25 * magni; //スコアに基礎点*倍率の25％を加算
            else if (combo <= 20) //コンボ数が20以下のとき
                scoreTemp = baseScore * 0.5 * magni; //スコアに基礎点*倍率の50％を加算
            else if (combo <= 30) //コンボ数が30以下のとき
                scoreTemp = baseScore * 0.75 * magni; //スコアに基礎点*倍率の75％を加算
            else //コンボ数が31以上のとき
                scoreTemp = baseScore * magni; //スコアに基礎点*倍率を加算
        }
        else
        { //コンボ数が30未満のときは単に基礎点*倍率を加算
            scoreTemp = baseScore * magni;
>>>>>>> [clean] deltaTime問題修正&ノーツがタッチできない問題修正&アタッチループ問題修正
        }


        AddText.color = Score_c;
        AddText.text = "+" + ((int)Math.Round(scoreTemp, 0, MidpointRounding.AwayFromZero)).ToString("D"); //四捨五入して型変換を行い加算スコアを表示
        AddText.transform.localScale = vl; //テキストサイズを全方向1.5倍化
        ATextReduce = AddText.transform.DOScale(vo, 0.2f); //元の大きさまでの縮小アニメーション

        for (int i = 0; i < 15; i++) //15分割したものを33ミリ秒ごとに15回加算()
        {
            score += scoreTemp / 15;
            ScoreText.text = ((int)Math.Round(score, 0, MidpointRounding.AwayFromZero)).ToString("D7"); //四捨五入して型変換を行い表示を更新
            await Task.Delay(33);
        }
        await Task.Delay(250);
        if (JTextUsed == JTextTemp) //もし次のAddScoreが読み込まれていなければ
        {
            JTextFade = DOTween.ToAlpha(() => JudgeText.color, cchanger => JudgeText.color = cchanger, 0.0f, 0.2f); //文字の消失アニメーション
            ATextFade = DOTween.ToAlpha(() => AddText.color, cchanger => AddText.color = cchanger, 0.0f, 0.2f); //文字の消失アニメーション
        }
    }

    public async void ChangeScene()
    {
        _score = (int)Math.Round(score, 0, MidpointRounding.AwayFromZero);
        _perfect = perfect;
        _great = great;
        _good = good;
        _miss = miss;
        StartCoroutine(RegistScoreNetworkProcess(musicTitles[SwipeMenu.selectedNumTmp], SelectScreenProcessManager.selectedLevel, _score));
        await Task.Delay(1000);
        SceneManager.LoadScene("ResultScene");
    }

    IEnumerator RegistScoreNetworkProcess(string selectedMusic, string selectedLevel, int score)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", PlayerPrefs.GetString("jwt"));
        form.AddField("music", selectedMusic);
        form.AddField("level", selectedLevel);
        form.AddField("score", score); //TODO: _score(int) の方が良？
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