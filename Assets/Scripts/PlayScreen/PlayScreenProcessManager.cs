using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Project.Scripts;
using Project.Scripts.SelectScreen;
using Project.Scripts.Tools.Firestore.Model;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Project.Scripts.Tools.AssetBundle;

public class PlayScreenProcessManager : MonoBehaviour
{
    // --- Attach from Unity --------------------------------------------------------------
    [FormerlySerializedAs("Note")] public GameObject[] notes; // 落下する5レーン分のノーツオブジェクト.
    [FormerlySerializedAs("PausePanel")] public GameObject pausePanel;
    [FormerlySerializedAs("Background")] public RectTransform background;
    [FormerlySerializedAs("ComboText")] public Text comboText;
    [FormerlySerializedAs("ScoreText")] public Text scoreText;
    [FormerlySerializedAs("JudgeText")] public Text judgeText;

    [FormerlySerializedAs("AddText")] public Text addText;
    // ------------------------------------------------------------------------------------

    // --- External variables -------------------------------------------------------------
    public const bool isAutoPlay = false; // 参照: NotesScript.
    public static bool isPlaying; // 参照: NotesScript.
    public static int Score, Perfect, Great, Good, Miss; // 参照: ResultScreenProcessManager.
    public float notesSpeedIndex = 5.0f; // 3.0f ~ 8.0f の値. 参照: NotesScript.

    public static Vector3 FallPerFrame; // 参照: NotesScript.
    // ------------------------------------------------------------------------------------

    // --- Environment variables ----------------------------------------------------------
    private const string RegisterScoreApiUri = EnvDataStore.ApiUri + "/auth/registScore";
    // ------------------------------------------------------------------------------------

    // Asset Bundle
    private AssetBundle[] _assetBundles;

    // Audio
    private AudioSource _playAudioSource;
    private AudioClip _playMusic;
    private CriAtomSource[] _judgeAudioSources;

    // Color
    private Color _perfectC, _greatC, _goodC, _missC, _scoreC;

    // CSV
    private readonly float[,] _noteTouchTime = new float[5, 4096];
    private int[] _totalNotes = {0, 0, 0, 0, 0};
    private int[] _currentNote = {0, 0, 0, 0, 0};

    private bool _isEndOfPlay, _isPaused, _isLowGraphicsMode, _enableNotesTouchSound;
    private float _score, _baseScore, _logSqSum;
    private float[] _logSq; // 区分求積法での各分点（0～最大49）におけるlogの値
    private float _startTime, _stopTime; // _stopTimeは一時停止の時刻を記録
    private int _combo, _perfect, _great, _good, _miss;
    private int _startTimingIndex; // 判定調整 (正: 遅くタップする)
    private int _jTextCounter; // JudgeText の変更回数

    // Magic Number
    private const int SepPoint = 50;

    // Music Data
    private Music[] _musics;

    // Tweener Animation
    private Tweener _jTextFade, _jTextReduce, _aTextFade, _aTextReduce;
    // ====================================================================================

    [Serializable]
    public class RegistScoreResponse
    {
        public bool success;
    }

    // ====================================================================================

    private async void Start()
    {
        _assetBundles = AssetBundleHandler.GetAssetBundles();
        _musics = AssetBundleHandler.GetMusics();

        // 初期化関連
        BackgroundCover();
        TextInitialization();
        ColorInitialization();
        AudioInitialization();
        AdjustJudgeRange();

        _startTimingIndex = (PlayerPrefs.GetInt("TimingAdjustment", 9) - 9) * 10; // 判定調整
        notesSpeedIndex = 5.0f + (PlayerPrefs.GetInt("NotesFallSpeed", 5) - 5) * 0.5f;
        FallPerFrame = (Vector3.down + Vector3.back * (float) Math.Sqrt(3)) * 0.6f * notesSpeedIndex; // ノーツ落下速度

        LoadCsv();
        BaseScoreCalculation();

        isPlaying = true;

        _startTime = Time.time;
        await Task.Delay((int) ((8100 + 10 * _startTimingIndex) / notesSpeedIndex));
        _playAudioSource.Play();


        _isEndOfPlay = true;
        _isLowGraphicsMode = PlayerPrefs.GetInt("lowGraphicsMode", 1) == 1;
        _enableNotesTouchSound = PlayerPrefs.GetInt("NotesTouchSound", 1) == 1;
    }

    private void Update()
    {
        if (isPlaying) CheckNextNotes();
        if (!_isEndOfPlay || _playAudioSource.isPlaying) return;
        _isEndOfPlay = false;
        ResultSceneTransition();
    }

    /// <summary>
    /// 背景画像を画面いっぱいに広げます．
    /// </summary>
    private void BackgroundCover()
    {
        var scale = 1f;
        if (Screen.width < 1920)
            scale = 1.5f;
        if (Screen.width < Screen.height)
            scale = (float) (Screen.height * 16) / (Screen.width * 9);
        background.sizeDelta = new Vector2(Screen.width * scale, Screen.height * scale);
    }

    /// <summary>
    /// スコアやコンボの表示を初期化します．
    /// </summary>
    private void TextInitialization()
    {
        addText.text = "";
        judgeText.text = "";
        comboText.text = _combo.ToString("D");
        scoreText.text = ((int) Math.Round(_score, 0, MidpointRounding.AwayFromZero)).ToString("D7");
    }

    /// <summary>
    /// Perfect や Great などの色を設定します．
    /// </summary>
    private void ColorInitialization()
    {
        ColorUtility.TryParseHtmlString("#FF7DF2", out _perfectC);
        ColorUtility.TryParseHtmlString("#FF9C7D", out _greatC);
        ColorUtility.TryParseHtmlString("#34E045", out _goodC);
        ColorUtility.TryParseHtmlString("#8D8D8D", out _missC);
        ColorUtility.TryParseHtmlString("#6C95FF", out _scoreC);
    }

    /// <summary>
    /// タッチ音や楽曲を設定します．
    /// </summary>
    private void AudioInitialization()
    {
        _judgeAudioSources = GameObject.Find("SoundEffect").GetComponents<CriAtomSource>();
        _playAudioSource = gameObject.AddComponent<AudioSource>();
        var musicName = _musics[SwipeMenu.selectedNumTmp].Name;
        _playMusic = _assetBundles[SwipeMenu.selectedNumTmp].LoadAsset<AudioClip>(musicName);
        _playAudioSource.clip = _playMusic;
    }

    /// <summary>
    /// それぞれの判定幅を調整します．
    /// </summary>
    private void AdjustJudgeRange()
    {
        var perfectRange = GameObject.Find("PerfectJudgeLine").GetComponent<Transform>();
        perfectRange.transform.localScale = new Vector3(1.8f, 0.1f, notesSpeedIndex * 1.2f * 0.12f);
        var greatRange = GameObject.Find("GreatJudgeLine").GetComponent<Transform>();
        greatRange.transform.localScale = new Vector3(1.8f, 0.1f, notesSpeedIndex * 1.2f * 0.18f);
        var goodRange = GameObject.Find("GoodJudgeLine").GetComponent<Transform>();
        goodRange.transform.localScale = new Vector3(1.8f, 0.1f, notesSpeedIndex * 1.2f * 0.2f);
    }

    /// <summary>
    /// CSVの譜面データを配列に格納します．
    /// </summary>
    private void LoadCsv()
    {
        var musicName = _musics[SwipeMenu.selectedNumTmp].Name;
        _playMusic = _assetBundles[SwipeMenu.selectedNumTmp].LoadAsset<AudioClip>(musicName);

        if (!(_assetBundles[SwipeMenu.selectedNumTmp]
                .LoadAsset<TextAsset>(musicName + "_" + SelectScreenProcessManager.selectedLevel) is { } csv)) return;
        var reader = new StringReader(csv.text);
        while (reader.Peek() > -1)
        {
            var line = reader.ReadLine();
            if (line == null) continue;
            var values = line.Split(',');
            _noteTouchTime[int.Parse(values[1]), _totalNotes[int.Parse(values[1])]++] = float.Parse(values[0]);
        }
    }

    /// <summary>
    /// 区分求積法によるスコア計算の値を用意しておきます．
    /// </summary>
    private void BaseScoreCalculation()
    {
        _logSq = new float[SepPoint];

        var denominator = _totalNotes.Sum() >= SepPoint ? SepPoint : _totalNotes.Sum();

        for (var i = 0; i < denominator; i++)
        {
            _logSq[i] = (float) Math.Log10(1 + (9 * ((float) i + 1) / denominator));
            _logSqSum += _logSq[i];
        }

        _baseScore = 1000000 / (_logSqSum + _totalNotes.Sum() - denominator);
    }

    /// <summary>
    /// 次のノーツがあるか確認し，ある場合は生成します．
    /// </summary>
    private void CheckNextNotes() // != 0だと反転孤独線の譜面が正常に読み込めません． >=0はどうですか？
    {
        for (var i = 0; i < 5; i++)
            while (_noteTouchTime[i, _currentNote[i]] < (Time.time - _startTime) &&
                   (Time.time - _startTime < 1 || _noteTouchTime[i, _currentNote[i]] != 0))
            {
                SpawnNotes(i);
                _currentNote[i]++;
            }
    }

    /// <summary>
    /// ノーツを生成します．
    /// </summary>
    private void SpawnNotes(int lineNum)
    {
        Instantiate(notes[lineNum], new Vector3(-0.73f + 0.365f * lineNum, 5.4f, -0.57f),
            Quaternion.Euler(-30f, 0, 0));
    }

    /// <summary>
    /// プレイを一時停止させます．
    /// </summary>
    public async void Pause()
    {
        switch (isPlaying)
        {
            case true when !_isPaused:
                isPlaying = false;
                _stopTime = Time.time;
                _playAudioSource.pitch = 0.0f;
                _isPaused = true;
                pausePanel.SetActive(true);
                break;
            case false when _isPaused:
            {
                pausePanel.SetActive(false);
                _isPaused = false;
                for (var i = 3; i > 0; i--)
                {
                    await Task.Delay(1000);
                    SoundEffect(2);
                }

                await Task.Delay(1000);
                isPlaying = true;
                _startTime += Time.time - _stopTime;
                await Task.Delay(10);
                _playAudioSource.pitch = 1.0f;
                break;
            }
        }
    }

    /// <summary>
    /// 曲選択画面に戻ります．
    /// </summary>
    public void SelectSceneTransition() => SceneManager.LoadScene("SelectScene");

    /// <summary>
    /// Perfect 時の動作です．
    /// </summary>
    public void PerfectTimingFunc()
    {
        if (_enableNotesTouchSound)
            SoundEffect(0);
        AddScore(0);
    }

    /// <summary>
    /// Great 時の動作です．
    /// </summary>
    public void GreatTimingFunc()
    {
        if (_enableNotesTouchSound)
            SoundEffect(1);
        AddScore(1);
    }

    /// <summary>
    /// Good 時の動作です．
    /// </summary>
    public void GoodTimingFunc()
    {
        if (_enableNotesTouchSound)
            SoundEffect(2);
        AddScore(2);
    }

    /// <summary>
    /// Miss 時の動作です．
    /// </summary>
    public void MissTimingFunc() => AddScore(3);

    /// <summary>
    /// タッチサウンドを再生します．
    /// </summary>
    public void SoundEffect(int num) =>
        //0：Perfect，1：Great，2：Good，3：Miss
        _judgeAudioSources[num].Play();

    /// <summary>
    /// スコアを加算します．
    /// </summary>
    public async void AddScore(int num) // 0：Perfect，1：Great，2：Good，3：Miss
    {
        float magni = 0, scoreTemp = 0, jTextTemp = ++_jTextCounter;
        var vl = new Vector3(1.5f, 1.5f, 1.5f);
        var vo = Vector3.one;

        // アニメーションの初期化
        _jTextFade?.Kill();
        _jTextReduce?.Kill();
        _aTextFade?.Kill();
        _aTextReduce?.Kill();

        switch (num)
        {
            case 0:
                magni = 1;
                _combo++;
                _perfect++;
                judgeText.transform.localScale = vl;
                _jTextReduce = judgeText.transform.DOScale(vo, 0.2f);
                judgeText.color = _perfectC;
                judgeText.text = "Perfect!";
                break;
            case 1:
                magni = 0.75f;
                _combo++;
                _great++;
                judgeText.transform.localScale = vl;
                _jTextReduce = judgeText.transform.DOScale(vo, 0.2f);
                judgeText.color = _greatC;
                judgeText.text = "Great!";
                break;
            case 2:
                magni = 0.25f;
                _combo = 0;
                _good++;
                judgeText.transform.localScale = vo;
                judgeText.color = _goodC;
                judgeText.text = "Good!";
                break;
            case 3:
                magni = 0;
                _combo = 0;
                _miss++;
                judgeText.transform.localScale = vo;
                judgeText.color = _missC;
                judgeText.text = "Miss!";
                break;
        }

        // コンボ数を更新
        comboText.text = _combo.ToString("D");

        // 区分求積法による加算スコアの計算
        scoreTemp = (_combo <= SepPoint) switch
        {
            true when _combo > 0 => _baseScore * _logSq[_combo - 1] * magni,
            true => _baseScore * _logSq[0] * magni,
            _ => _baseScore * magni
        };

        if (_perfect == _totalNotes.Sum()) scoreTemp = 1000000 - _score; // floatへの変更でAP時に100万点にならないので強制代入

        // 加算スコア表示の文字色を変更
        addText.color = _scoreC;
        // 加算スコア表示の値を更新
        addText.text = "+" + ((int) Math.Round(scoreTemp, 0, MidpointRounding.AwayFromZero)).ToString("D");
        // アニメーション
        addText.transform.localScale = vl;
        _aTextReduce = addText.transform.DOScale(vo, 0.2f);

        // 軽量化設定によってはスコアのパラパラ表示を省略
        if (!_isLowGraphicsMode)
        {
            for (var i = 0; i < 15; i++)
            {
                _score += scoreTemp / 15;
                scoreText.text = ((int) Math.Round(_score, 0, MidpointRounding.AwayFromZero)).ToString("D7");
                await Task.Delay(33);
            }
        }
        else
        {
            _score += scoreTemp;
            scoreText.text = ((int) Math.Round(_score, 0, MidpointRounding.AwayFromZero)).ToString("D7");
        }

        //  ↓なんで!=じゃないんだっけ
        if (_jTextCounter == jTextTemp) return;

        // 250ms経過しても次のAddScoreが発生していなければフェードアウト処理
        await Task.Delay(250);
        if (_jTextCounter != jTextTemp) return;
        _jTextFade = DOTween.ToAlpha(() => judgeText.color, cchanger => judgeText.color = cchanger, 0.0f, 0.2f);
        _aTextFade = DOTween.ToAlpha(() => addText.color, cchanger => addText.color = cchanger, 0.0f, 0.2f);
    }

    /// <summary>
    /// スコア結果を表示します．
    /// </summary>
    public async void ResultSceneTransition()
    {
        Score = (int) Math.Round(_score, 0, MidpointRounding.AwayFromZero);
        Perfect = _perfect;
        Great = _great;
        Good = _good;
        Miss = _miss;
        StartCoroutine(RegistScoreNetworkProcess(_musics[SwipeMenu.selectedNumTmp].Name,
            SelectScreenProcessManager.selectedLevel));
        await Task.Delay(1000);
        SceneManager.LoadScene("ResultScene");
    }

    /// <summary>
    /// データベースにユーザとスコアを送信します．
    /// </summary>
    IEnumerator RegistScoreNetworkProcess(string selectedMusic, string selectedLevel)
    {
        var form = new WWWForm();
        form.AddField("token", PlayerPrefs.GetString("jwt"));
        form.AddField("music", selectedMusic);
        form.AddField("level", selectedLevel);
        form.AddField("score", Score);
        var www = UnityWebRequest.Post(RegisterScoreApiUri, form);
        yield return www.SendWebRequest();
        switch (www.result)
        {
            case UnityWebRequest.Result.Success:
                ResponseCheck(www.downloadHandler.text);
                break;
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.ProtocolError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError("ネットワークに接続できません．(" + www.error + ")");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// APIサーバからのレスポンスを確認します．
    /// </summary>
    private static void ResponseCheck(string data)
    {
        var jsnData = JsonUtility.FromJson<RegistScoreResponse>(data);

        Debug.Log(jsnData.success ? "スコアを登録しました" : "スコアの登録に失敗しました");
    }
}