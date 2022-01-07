using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Project.Scripts.SelectScreen
{
    public class SwipeMenu : MonoBehaviour
    {
        public GameObject scrollbar;
        private float _scrollPos;
        private float _distance;

        private float[] _pos;

        // private AudioSource[] _AudioSource; //プレビュー楽曲情報格納
        ////////private AudioSource[] _fullAudioSource; //フル楽曲情報格納
        // private static float[] _musicTime; //楽曲の再生時間を格納
        // [FormerlySerializedAs("DisplayedMusicTime")] public Text displayedMusicTime; //画面に表示される楽曲の再生時間
        public RectTransform background;
        // [FormerlySerializedAs("ScrollView")] public RectTransform scrollView;
        // [FormerlySerializedAs("ScrollViewPadding")] public HorizontalLayoutGroup scrollViewPadding;
        public Text yourHighScoreText;
        public Text onlineHighScoreText;
        public GameObject cardTemplate;
        private ToggleGroup[] _toggleGroup;
        public static int selectedNumTmp;
        private string _selectedLevelTmp;

        private AudioClip[] _previewAudioClip;
        private AudioSource[] _previewAudioSource;

        private List<DownloadProcessManager.MusicList> _musicData;
        private AssetBundle[] _assetBundle;

        // ------------------------------------------------------------------------------------

        private const string GetMyScoreApiUri = EnvDataStore.ApiUri + "/auth/myScore";
        private const string GetOnlineScoreApiUri = EnvDataStore.ApiUri + "/topScore";
    
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
            _musicData = DownloadProcessManager.MusicData;
            _pos = new float[_musicData.Count];
            _distance = 1f / (_pos.Length - 1f);
            for (var i = 0; i < _pos.Length; i++) _pos[i] = _distance * i;
            BackgroundCover();
            CardCloner();
            GetScoresController(0);
            SetCardInformation();
            SetPreviewMusic();
        }

        public void Update()
        {
            if (Input.GetMouseButton(0)) // クリック中は，scroll_posに横スクロールのx座標を格納し続ける．
                _scrollPos = scrollbar.GetComponent<Scrollbar>().value;
            else // クリックを離したときに，scroll_posの値を参考に，最も近いカードを中央に持ってくる．
                foreach (var t in _pos)
                    if (_scrollPos < t + _distance / 2 && _scrollPos > t - _distance / 2)
                        scrollbar.GetComponent<Scrollbar>().value =
                            Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, t, 0.1f);

            for (var i = 0; i < _pos.Length; i++)
            {
                if (!(_scrollPos < _pos[i] + _distance / 2) || !(_scrollPos > _pos[i] - _distance / 2)) continue;

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
                for (var cnt = 0; cnt < _pos.Length; cnt++)
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
            for (var i = 1; i < _musicData.Count; i++)
            {
                var clone = Instantiate(cardTemplate, cardTemplate.transform.parent, true);
                if (clone is null) continue;
                clone.transform.localPosition = cardTemplate.transform.localPosition;
                clone.transform.localScale = cardTemplate.transform.localScale;
            }

            _toggleGroup = new ToggleGroup[_musicData.Count];
            for (var i = 0; i < _musicData.Count; i++)
                _toggleGroup[i] = transform.GetChild(i).Find("Easy").GetComponent<ToggleGroup>();
        }

        /// <summary>
        /// カードの情報を設定します．
        /// </summary>
        private void SetCardInformation()
        {
            foreach (var music in _musicData)
            {
                transform.GetChild(music.id - 1).Find("Title").GetComponent<Text>().text = _musicData[music.id - 1].title;
                transform.GetChild(music.id - 1).Find("Artist").GetComponent<Text>().text = _musicData[music.id - 1].artist;
                var artworkSprite =
                    _assetBundle[music.id - 1].LoadAsset<Sprite>(_musicData[music.id - 1].name + "_artwork");
                transform.GetChild(music.id - 1).Find("Artwork").GetComponent<Image>().sprite = artworkSprite;
            }
        }

        /// <summary>
        /// プレビュー曲をAssetBundleから読み込みます．
        /// </summary>
        private void SetPreviewMusic()
        {
            _previewAudioClip = new AudioClip[_musicData.Count];
            _previewAudioSource = gameObject.GetComponents<AudioSource>();
            foreach (var music in _musicData)
            {
                _previewAudioClip[music.id - 1] = _assetBundle[music.id - 1].LoadAsset<AudioClip>(music.name + "_pre");
                _previewAudioSource[music.id - 1].clip = _previewAudioClip[music.id - 1];
            }
        }

        /// <summary>
        /// num番目の曲をループで再生します．
        /// </summary>
        private void SelectedMusic(int num)
        {
            for (var i = 0; i < _pos.Length; i++)
            {
                if (i == num) // numは現在選択中の楽曲通し番号
                {
                    if (!_previewAudioSource[i].isPlaying) _previewAudioSource[num].Play(); // 選択中の楽曲がプレビュー再生されていないとき楽曲を再生
                }
                else
                {
                    if (_previewAudioSource[i].isPlaying) // 非選択の楽曲がプレビュー再生されているとき
                        _previewAudioSource[i].Stop(); // 非選択の楽曲の再生を停止
                }
            }
        }

        /// <summary>
        /// マイベストスコアと最高スコアの取得を管理します．
        /// </summary>
        private void GetScoresController(int selectedNum)
        {
            var selectedLevel = _toggleGroup[selectedNum].ActiveToggles()
                .First().GetComponentsInChildren<Text>()
                .First(t => t.name == "Label").text;
            if (selectedNumTmp != selectedNum || selectedLevel != _selectedLevelTmp)
            {
                StartCoroutine(MyScoreNetworkProcess(_musicData[selectedNum].name, selectedLevel));
                StartCoroutine(OnlineScoreNetworkProcess(_musicData[selectedNum].name, selectedLevel));
            }

            selectedNumTmp = selectedNum;
            _selectedLevelTmp = selectedLevel;
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
            var request = UnityWebRequest.Post(GetMyScoreApiUri, form);
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
            var request = UnityWebRequest.Post(GetOnlineScoreApiUri, form);
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
}