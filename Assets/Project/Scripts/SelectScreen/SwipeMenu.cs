using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Serialization;

public class SwipeMenu : MonoBehaviour
{
    public GameObject scrollbar;
    float scroll_pos = 0;
    float distance;

    float[] pos;

    // private AudioSource[] _AudioSource; //プレビュー楽曲情報格納
    ////////private AudioSource[] _fullAudioSource; //フル楽曲情報格納
    private static float[] Musictime; //楽曲の再生時間を格納
    public Text DisplayedMusicTime; //画面に表示される楽曲の再生時間
    [FormerlySerializedAs("Background")] public RectTransform background;
    public RectTransform ScrollView;
    public HorizontalLayoutGroup ScrollViewPadding;
    public Text yourHighScoreText;
    public Text onlineHighScoreText;
    public GameObject cardTemplate;
    private ToggleGroup[] _toggleGroup;
    public static int selectedNumTmp;
    private string selectedLevelTmp;

    private AudioClip[] previewAudioClip;
    private AudioSource[] previewAudioSource;

    private List<DownloadProcessManager.MusicList> _MusicData;
    private AssetBundle[] _assetBundle;

    // ------------------------------------------------------------------------------------

    static readonly string getMyScoreApiUri = EnvDataStore.getMyScoreApiUri;

    static readonly string getOnlineScoreApiUri = EnvDataStore.getOnlineScoreApiUri;
    // static readonly string[] musicTitles = MusicTitleDataStore.musicTitles;

    // ------------------------------------------------------------------------------------

    [Serializable]
    public class MyScoreResponse
    {
        public bool success;
        public List<ScoreList> data;
    }

    [Serializable]
    public class ScoreList
    {
        public string name;
        public int score;
    }

    private void Start()
    {
        _assetBundle = DownloadProcessManager.AssetBundle;
        _MusicData = DownloadProcessManager.MusicData;
        pos = new float[_MusicData.Count];
        distance = 1f / (pos.Length - 1f);
        for (var i = 0; i < pos.Length; i++) pos[i] = distance * i;
        BackgroundCover();
        CardCloner();
        GetScoresController(0);
        SetCardInformation();
        SetPreviewMusic();
    }

    public void Update()
    {
        if (Input.GetMouseButton(0)) // クリック中は，scroll_posに横スクロールのx座標を格納し続ける．
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
        else // クリックを離したときに，scroll_posの値を参考に，最も近いカードを中央に持ってくる．
            foreach (var t in pos)
                if (scroll_pos < t + distance / 2 && scroll_pos > t - distance / 2)
                    scrollbar.GetComponent<Scrollbar>().value =
                        Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, t, 0.1f);

        for (var i = 0; i < pos.Length; i++)
        {
            if (!(scroll_pos < pos[i] + distance / 2) || !(scroll_pos > pos[i] - distance / 2)) continue;

            // カードを拡大する
            transform.GetChild(i).localScale =
                Vector2.Lerp(transform.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);
            transform.GetChild(i).Find("NoteLogo").localScale =
                Vector2.Lerp(transform.GetChild(i).Find("NoteLogo").localScale, new Vector2(1f, 1f), 0.1f);
            transform.GetChild(i).Find("NoteLogo").gameObject.transform.position = Vector2.Lerp(
                transform.GetChild(i).Find("NoteLogo").transform.position,
                new Vector2(transform.GetChild(i).Find("NoteLogo").transform.position.x, 35), 0.08f);
            transform.GetChild(i).Find("Title").localScale =
                Vector2.Lerp(transform.GetChild(i).Find("Title").localScale, new Vector2(1f, 1f), 0.08f);
            transform.GetChild(i).Find("Title").gameObject.transform.position = Vector2.Lerp(
                transform.GetChild(i).Find("Title").transform.position,
                new Vector2(transform.GetChild(i).Find("Title").transform.position.x, 35), 0.08f);
            transform.GetChild(i).Find("Artist").localScale =
                Vector2.Lerp(transform.GetChild(i).Find("Artist").localScale, new Vector2(1f, 1f), 0.08f);
            transform.GetChild(i).Find("Artist").gameObject.transform.position = Vector2.Lerp(
                transform.GetChild(i).Find("Artist").transform.position,
                new Vector2(transform.GetChild(i).Find("Artist").transform.position.x, 30), 0.08f);
            transform.GetChild(i).Find("Artwork").localScale =
                Vector2.Lerp(transform.GetChild(i).Find("Artwork").localScale, new Vector2(1f, 1f), 0.08f);
            transform.GetChild(i).Find("Artwork").gameObject.transform.position = Vector2.Lerp(
                transform.GetChild(i).Find("Artwork").transform.position,
                new Vector2(transform.GetChild(i).Find("Artwork").transform.position.x, 10), 0.1f);
            transform.GetChild(i).Find("Easy").gameObject.SetActive(true);
            transform.GetChild(i).Find("Basic").gameObject.SetActive(true);
            transform.GetChild(i).Find("Hard").gameObject.SetActive(true);
            transform.GetChild(i).Find("Demon").gameObject.SetActive(true);
            transform.GetChild(i).Find("PlayMusic").gameObject.SetActive(true);

            SelectedMusic(i); //　楽曲再生の実行と停止を行う（1フレーム毎）
            GetScoresController(i);

            // カードを縮小する
            for (var cnt = 0; cnt < pos.Length; cnt++)
            {
                if (i == cnt) continue;
                transform.GetChild(cnt).localScale = Vector2.Lerp(transform.GetChild(cnt).localScale,
                    new Vector2(0.8f, 0.7f), 0.1f);
                transform.GetChild(cnt).Find("NoteLogo").localScale = Vector2.Lerp(
                    transform.GetChild(cnt).Find("NoteLogo").localScale, new Vector2(1.1f, 1.257f), 0.1f);
                transform.GetChild(cnt).Find("NoteLogo").gameObject.transform.position = Vector2.Lerp(
                    transform.GetChild(cnt).Find("NoteLogo").transform.position,
                    new Vector2(transform.GetChild(cnt).Find("NoteLogo").transform.position.x, 23), 0.08f);
                transform.GetChild(cnt).Find("Title").localScale = Vector2.Lerp(
                    transform.GetChild(cnt).Find("Title").localScale, new Vector2(1.1f, 1.257f), 0.08f);
                transform.GetChild(cnt).Find("Title").gameObject.transform.position = Vector2.Lerp(
                    transform.GetChild(cnt).Find("Title").transform.position,
                    new Vector2(transform.GetChild(cnt).Find("Title").transform.position.x, 23), 0.08f);
                transform.GetChild(cnt).Find("Artist").localScale = Vector2.Lerp(
                    transform.GetChild(cnt).Find("Artist").localScale, new Vector2(1f, 1.14f), 0.08f);
                transform.GetChild(cnt).Find("Artist").gameObject.transform.position = Vector2.Lerp(
                    transform.GetChild(cnt).Find("Artist").transform.position,
                    new Vector2(transform.GetChild(cnt).Find("Artist").transform.position.x, 19), 0.08f);
                transform.GetChild(cnt).Find("Artwork").localScale = Vector2.Lerp(
                    transform.GetChild(cnt).Find("Artwork").localScale, new Vector2(1.5f, 1.71f), 0.08f);
                transform.GetChild(cnt).Find("Artwork").gameObject.transform.position = Vector2.Lerp(
                    transform.GetChild(cnt).Find("Artwork").transform.position,
                    new Vector2(transform.GetChild(cnt).Find("Artwork").transform.position.x, -3), 0.1f);
                transform.GetChild(cnt).Find("Easy").gameObject.SetActive(false);
                transform.GetChild(cnt).Find("Basic").gameObject.SetActive(false);
                transform.GetChild(cnt).Find("Hard").gameObject.SetActive(false);
                transform.GetChild(cnt).Find("Demon").gameObject.SetActive(false);
                transform.GetChild(cnt).Find("PlayMusic").gameObject.SetActive(false);
            }
        }
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
            scale = (float)(Screen.height * 16) / (Screen.width * 9);
        background.sizeDelta = new Vector2(Screen.width * scale, Screen.height * scale);
    }

    /// <summary>
    /// 曲数だけカードを複製します．
    /// </summary>
    private void CardCloner()
    {
        for (var i = 1; i < _MusicData.Count; i++)
        {
            var clone = Instantiate(cardTemplate, cardTemplate.transform.parent, true);
            if (clone is null) continue;
            clone.transform.localPosition = cardTemplate.transform.localPosition;
            clone.transform.localScale = cardTemplate.transform.localScale;
        }

        _toggleGroup = new ToggleGroup[_MusicData.Count];
        for (var i = 0; i < _MusicData.Count; i++)
            _toggleGroup[i] = transform.GetChild(i).Find("Easy").GetComponent<ToggleGroup>();
    }

    /// <summary>
    /// カードの情報を設定します．
    /// </summary>
    private void SetCardInformation()
    {
        foreach (var music in _MusicData)
        {
            transform.GetChild(music.id - 1).Find("Title").GetComponent<Text>().text = _MusicData[music.id - 1].title;
            transform.GetChild(music.id - 1).Find("Artist").GetComponent<Text>().text = _MusicData[music.id - 1].artist;
            var artworkSprite =
                _assetBundle[music.id - 1].LoadAsset<Sprite>(_MusicData[music.id - 1].name + "_artwork");
            transform.GetChild(music.id - 1).Find("Artwork").GetComponent<Image>().sprite = artworkSprite;
        }
    }

    /// <summary>
    /// プレビュー曲をAssetBundleから読み込みます．
    /// </summary>
    private void SetPreviewMusic()
    {
        previewAudioClip = new AudioClip[_MusicData.Count];
        previewAudioSource = gameObject.GetComponents<AudioSource>();
        foreach (var music in _MusicData)
        {
            previewAudioClip[music.id - 1] = _assetBundle[music.id - 1].LoadAsset<AudioClip>(music.name + "_pre");
            previewAudioSource[music.id - 1].clip = previewAudioClip[music.id - 1];
        }
    }

    /// <summary>
    /// num番目の曲をループで再生します．
    /// </summary>
    private void SelectedMusic(int num)
    {
        for (var i = 0; i < pos.Length; i++)
        {
            if (i == num) // numは現在選択中の楽曲通し番号
            {
                if (!previewAudioSource[i].isPlaying) previewAudioSource[num].Play(); // 選択中の楽曲がプレビュー再生されていないとき楽曲を再生
            }
            else
            {
                if (previewAudioSource[i].isPlaying) // 非選択の楽曲がプレビュー再生されているとき
                    previewAudioSource[i].Stop(); // 非選択の楽曲の再生を停止
            }
        }
    }

    /// <summary>
    /// マイベストスコアと最高スコアの取得を管理します．
    /// </summary>
    public void GetScoresController(int selectedNum)
    {
        var selectedLevel = _toggleGroup[selectedNum].ActiveToggles()
            .First().GetComponentsInChildren<Text>()
            .First(t => t.name == "Label").text;
        if (selectedNumTmp != selectedNum || selectedLevel != selectedLevelTmp)
        {
            StartCoroutine(MyScoreNetworkProcess(_MusicData[selectedNum].name, selectedLevel));
            StartCoroutine(OnlineScoreNetworkProcess(_MusicData[selectedNum].name, selectedLevel));
        }

        selectedNumTmp = selectedNum;
        selectedLevelTmp = selectedLevel;
    }

    /// <summary>
    /// マイベストスコアを取得します．
    /// </summary>
    private IEnumerator MyScoreNetworkProcess(string selectedMusic, string selectedLevel)
    {
        var form = new WWWForm();
        form.AddField("token", PlayerPrefs.GetString("jwt"));
        form.AddField("music", selectedMusic);
        form.AddField("level", selectedLevel);
        var request = UnityWebRequest.Post(getMyScoreApiUri, form);
        yield return request.SendWebRequest();

        switch (request.result)
        {
            case UnityWebRequest.Result.Success:
                ApplyMyScore(request.downloadHandler.text);
                break;

            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.ProtocolError:
            case UnityWebRequest.Result.DataProcessingError:
                yourHighScoreText.text = "--------";
                break;

            case UnityWebRequest.Result.InProgress:
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// マイベストスコアを設定します．
    /// </summary>
    private void ApplyMyScore(string data)
    {
        var jsnData = JsonUtility.FromJson<MyScoreResponse>(data);

        if (jsnData.success && jsnData.data.Count != 0)
            yourHighScoreText.text = jsnData.data[0].score.ToString();
        else
            yourHighScoreText.text = "--------";
    }

    /// <summary>
    /// 最高スコアを取得します．
    /// </summary>
    private IEnumerator OnlineScoreNetworkProcess(string selectedMusic, string selectedLevel)
    {
        var form = new WWWForm();
        form.AddField("music", selectedMusic);
        form.AddField("level", selectedLevel);
        var request = UnityWebRequest.Post(getOnlineScoreApiUri, form);
        yield return request.SendWebRequest();

        switch (request.result)
        {
            case UnityWebRequest.Result.Success:
                ApplyOnlineScore(request.downloadHandler.text);
                break;

            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.ProtocolError:
            case UnityWebRequest.Result.DataProcessingError:
                onlineHighScoreText.text = "--------";
                break;

            case UnityWebRequest.Result.InProgress:
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// 最高スコアを設定します．
    /// </summary>
    private void ApplyOnlineScore(string data)
    {
        var jsnData = JsonUtility.FromJson<MyScoreResponse>(data);

        if (jsnData.success && jsnData.data.Count != 0)
            onlineHighScoreText.text = jsnData.data[0].score.ToString();
        else
            onlineHighScoreText.text = "--------";
    }
}