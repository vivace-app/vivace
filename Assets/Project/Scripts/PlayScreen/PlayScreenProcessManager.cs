using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PlayScreenProcessManager : MonoBehaviour
{
    private AudioSource _audioSource;
    public GameObject[] Note;
    public Text ComboText, ScoreText, JudgeText;
    private string Perfect16 = "#FF7DF2", Great16 = "#FF9C7D", Good16 = "#34E045", Miss16 = "#8D8D8D"; //ResultScreenの色を拝借
    private Color Perfect_c, Great_c, Good_c, Miss_c;
    private int _notesTotal = 0;
    private int _notesCount = 0;
    private float _startTime = 0;
    private float _stoptime = 0; //ポーズ時間を計測
    public float[] _timing;
    public int[] _lineNum;
    private static AudioSource[] _SoundEffects; //効果音用変数
    public static bool _isPlaying = true; //プレイ中がポーズ中かを判定
    private static bool playedFlag = false; //楽曲の再生が一度でも開始されたかどうかを判定
    public static bool _autoPlay = false; //自動プレイ用
    public int _combo = 0; //コンボ数
    public int _perfects = 0, _greats = 0, _goods = 0, _misss = 0;
    public static int r_perfects = 0, r_greats = 0, r_goods = 0, r_misss = 0; //リザルト画面用
    public double _score = 0; //得点
    public static double r_score = 0; //リザルト画面用
    public double _basescore = 0; //基礎点:ノーツ1つあたりのスコア
    public static float _notesSpeedIndex = 5.0f; //ノーツ落下速度の設定用(1.0f~10.0fまで動作確認)
    private Tweener tweener;

    // -- Temporary Variable. -------------------------------------------------------------
    private string csvFilePass = "CSV/BurningHeart";
    // ------------------------------------------------------------------------------------

    async void Start()
    {
        float delay_time = 0; //遅延開始時間の計算用

        _timing = new float[1024];
        _lineNum = new int[1024];
        ComboText.text = _combo.ToString("D");
        ScoreText.text = ((int)Math.Round(_score, 0, MidpointRounding.AwayFromZero)).ToString("D7");
        ColorUtility.TryParseHtmlString(Perfect16, out Perfect_c);
        ColorUtility.TryParseHtmlString(Great16, out Great_c);
        ColorUtility.TryParseHtmlString(Good16, out Good_c);
        ColorUtility.TryParseHtmlString(Miss16, out Miss_c);
        AdjustJudgeRange(); //ノーツ落下速度に合わせて判定オブジェクトの高さを変化
        delay_time = 12800 / _notesSpeedIndex; //遅延開始時間の計算
        _audioSource = GameObject.Find("Music").GetComponent<AudioSource>();
        _SoundEffects = GameObject.Find("SoundEffect").GetComponents<AudioSource>();
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
        if (_isPlaying == true) CheckNextNotes();
        if (playedFlag == true && _audioSource.isPlaying == false)
        {
            playedFlag = false; //何度も処理が呼び出されないようにする
            ChangeScene(); //楽曲が一度でも再生され，最後まで再生が終了したとき
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

    public async void AddScore(int swi)
    { //加点のための関数,引数magniは判定ごとのスコア倍率
        double magni = 0, ScoreTemp = 0;
        Vector3 vl = new Vector3(1.5f, 1.5f, 1.5f);
        Vector3 vo = Vector3.one;

        switch (swi)
        {
            case 0: //Perfect
                magni = 1; //加算倍率は1
                _combo++; //コンボ数を1加算
                _perfects++; //累計Perfect数を1加算
                JudgeText.transform.localScale = vl; //テキストを全方向1.5倍化
                JudgeText.transform.DOScale(vo, 0.2f); //元の大きさまで縮小
                JudgeText.color = Perfect_c;
                JudgeText.text = "Perfect!";
                break;
            case 1: //Great
                magni = 0.75; //加算倍率は0.75
                _combo++; //コンボ数を1加算
                _greats++; //累計Great数を1加算
                JudgeText.transform.localScale = vl; //テキストを全方向1.5倍化
                JudgeText.transform.DOScale(vo, 0.2f); //元の大きさまで縮小
                JudgeText.color = Great_c;
                JudgeText.text = "Great!";
                break;
            case 2: //Good
                magni = 0.25; //加算倍率は0.25
                _combo = 0; //コンボ数を初期化
                _goods++; //累計Good数を1加算
                JudgeText.color = Good_c;
                JudgeText.text = "Good!";
                break;
            case 3: //Miss
                magni = 0; //加算倍率は0
                _combo = 0; //コンボ数を初期化
                _misss++; //累計Miss数を1加算
                JudgeText.color = Miss_c;
                JudgeText.text = "Miss!";
                break;
        }
        ComboText.text = _combo.ToString("D"); //表示に反映
        if (_notesTotal >= 30)
        { //コンボ数が30以上のときにはスコアは以下の通り傾斜加算
            if (_combo <= 10) //コンボ数が10以下のとき
                ScoreTemp = _basescore * 0.25 * magni; //スコアに基礎点*倍率の25％を加算
            else if (_combo <= 20) //コンボ数が20以下のとき
                ScoreTemp = _basescore * 0.5 * magni; //スコアに基礎点*倍率の50％を加算
            else if (_combo <= 30) //コンボ数が30以下のとき
                ScoreTemp = _basescore * 0.75 * magni; //スコアに基礎点*倍率の75％を加算
            else //コンボ数が31以上のとき
                ScoreTemp = _basescore * magni; //スコアに基礎点*倍率を加算
        }
        else
        { //コンボ数が30未満のときは単に基礎点*倍率を加算
            ScoreTemp = _basescore * magni;
        }
        for (int i = 0; i < 15; i++) //15分割したものを33ミリ秒ごとに15回加算()
        {
            _score += ScoreTemp / 15;
            ScoreText.text = ((int)Math.Round(_score, 0, MidpointRounding.AwayFromZero)).ToString("D7"); //四捨五入して型変換を行い表示を更新
            await Task.Delay(33);
        }
        await Task.Delay(1000);
    }

    void CheckNextNotes()
    {
        while (_timing[_notesCount] < (Time.time - _startTime) && _timing[_notesCount] != 0) SpawnNotes(_lineNum[_notesCount++]);
    }

    void SpawnNotes(int num)
    {
        //Debug.Log(num);
        Instantiate(Note[num], new Vector3(-0.676f + (0.338f * num), 8.4f, 4.5f), Quaternion.Euler(-30f, 0, 0));
    }

    public void SoundEffect(int num)
    {
        _SoundEffects[num].PlayOneShot(_SoundEffects[num].clip);
        //Debug.Log ("SoundEffect Played.");
    }

    public async void Pause()
    {
        if (_isPlaying == true)
        {
            //Debug.Log("止まるドン！");
            _isPlaying = false; //ポーズ中であることを変数に代入
            _stoptime = Time.time; //ポーズした時刻を代入
            _audioSource.pitch = 0.0f; //楽曲再生速度を0にする(Pauseを使うと楽曲終了判定が誤作動するため)
        }
        else if (_isPlaying == false)
        {
            //Debug.Log("さぁ，再開するドン！");
            for (int i = 3; i > 0; i--) //再開のカウントダウン
            {
                await Task.Delay(1000);
                //Debug.Log(i);
                SoundEffect(2);
            }
            await Task.Delay(1000);
            _isPlaying = true; //プレイ中であることを変数に代入
            _startTime = _startTime + (Time.time - _stoptime); //開始時間をポーズ時間分加算
            await Task.Delay(10); //ノーツと楽曲がずれるのを補正
            _audioSource.pitch = 1.0f; //楽曲再生速度を1にする(等速再生)
        }
    }

    public void PerfectTimingFunc(int num)
    {
        PlayScreenProcessManager a = GameObject.Find("ProcessManager").GetComponent<PlayScreenProcessManager>();
        // Debug.Log ("Line:" + num + " Perfect!"); //ログ出力
        // Debug.Log (GetMusicTime ()); //ログ出力
        //EffectManager.Instance.PlayEffect(num); //num番目のエフェクトを表示
        a.tweener.Kill();
        SoundEffect(0); //Perfectサウンド（引数0）を再生
        a.AddScore(0); //スコア加算
        a.tweener = DOTween.ToAlpha(() => a.JudgeText.color, cchanger => a.JudgeText.color = cchanger, 0.0f, 0.2f); //文字が段々消えるやつ

        //Debug.Log("PerfectTimingFunc"); //ログ出力
    }

    public void GreatTimingFunc(int num)
    {
        PlayScreenProcessManager a = GameObject.Find("ProcessManager").GetComponent<PlayScreenProcessManager>();
        a.tweener.Kill();
        SoundEffect(1); //Greatサウンド再生
                        //EffectManager.Instance.PlayEffect(num); //num番目のエフェクトを表示
        a.AddScore(1); //スコア加算(倍率はGreatなので0.75)
        a.tweener = DOTween.ToAlpha(() => a.JudgeText.color, cchanger => a.JudgeText.color = cchanger, 0.0f, 0.2f); //文字が段々消えるやつ
                       //Debug.Log("GreatTimingFunc"); //ログ出力
    }

    public void GoodTimingFunc(int num)
    {
        PlayScreenProcessManager a = GameObject.Find("ProcessManager").GetComponent<PlayScreenProcessManager>();
        a.tweener.Kill();
        SoundEffect(2); //Goodサウンド再生
                        //EffectManager.Instance.PlayEffect(num); //num番目のエフェクトを表示
        a.AddScore(2); //スコア加算(倍率はGoodなので0.25)
        a.tweener = DOTween.ToAlpha(() => a.JudgeText.color, cchanger => a.JudgeText.color = cchanger, 0.0f, 0.2f); //文字が段々消えるやつ
                       //Debug.Log("GoodTimingFunc"); //ログ出力
    }

    public static void MissTimingFunc(int num)
    {
        PlayScreenProcessManager a = GameObject.Find("ProcessManager").GetComponent<PlayScreenProcessManager>();
        //EffectManager.Instance.PlayEffect(num); //num番目のエフェクトを表示
        a.tweener.Kill();
        a.AddScore(3); //スコア加算(スコアはあげないよ！ｗ)
        a.tweener = DOTween.ToAlpha(() => a.JudgeText.color, cchanger => a.JudgeText.color = cchanger, 0.0f, 0.2f); //文字が段々消えるやつ
                       //Debug.Log("MissTimingFunc"); //ログ出力
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
        r_score = _score;
        r_perfects = _perfects;
        r_greats = _greats;
        r_goods = _goods;
        r_misss = _misss;
        Debug.Log("3秒後にスコア画面に移行します．");
        await Task.Delay(3000);
        SceneManager.LoadScene("ResultScene");
    }
}