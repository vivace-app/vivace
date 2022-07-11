using Tools.AssetBundle;
using Tools.Firestore.Model;
using UnityEngine;

namespace SelectScene
{
    public class ProcessManager : MonoBehaviour
    {
        private AssetBundle[] _assetBundles;
        private AudioClip[] _previewAudioClips;
        private AudioSource[] _previewAudioSources;
        private Music[] _musics;

        private float _artworkDistance;
        private float[] _artworkPositions;
        private float _scrolledPosition;


        private void Start()
        {
            _assetBundles = AssetBundleHandler.GetAssetBundles();
            _musics = AssetBundleHandler.GetMusics();

            _artworkPositions = new float[_musics.Length];
            _artworkDistance = 1f / (_artworkPositions.Length - 1f);
            for (var i = 0; i < _artworkPositions.Length; i++)
            {
                _artworkPositions[i] = _artworkDistance * i; //TODO: リファクタ
            }

            ArtworkCloner();
            AttachPreviewMusic();
        }

        private void Update()
        {
            if (Input.GetMouseButton(0)) // スクロール中，そのスクロール量を格納する．
                _scrolledPosition = View.Instance.Scrollbar;
            else // クリックを離したときに，_scrolledPosition の値を参考に，最も近いカードを中央に持ってくる．
                foreach (var t in _artworkPositions)
                    if (_scrolledPosition < t + _artworkDistance / 2 && _scrolledPosition > t - _artworkDistance / 2)
                        View.Instance.Scrollbar = Mathf.Lerp(View.Instance.Scrollbar, t, 0.1f);

            for (var i = 0; i < _artworkPositions.Length; i++)
            {
                if (!(_scrolledPosition < _artworkPositions[i] + _artworkDistance / 2) ||
                    !(_scrolledPosition > _artworkPositions[i] - _artworkDistance / 2)) continue;

                // カードを拡大する
                // View.Instance.ArtworkContentGameObject.transform.GetChild(i).localScale =
                //     Vector2.Lerp(View.Instance.ArtworkContentGameObject.transform.GetChild(i).localScale,
                //         new Vector2(1.6f, 1.6f), 0.1f);
                View.Instance.ArtworkContentGameObject.transform.GetChild(i).GetComponent<RectTransform>()
                    .sizeDelta = new Vector2(View.Instance.ArtworkHeight * 1.5f, View.Instance.ArtworkHeight * 1.5f);

                // var position = artworkBackgroundBottomRectTransform.anchoredPosition;
                // position.y -= _artworkBackgroundHeight * 0.7f - (497f - 106.8f);
                // artworkBackgroundBottomRectTransform.anchoredPosition = position;

                // View.Instance.ArtworkContentGameObject.transform.GetChild(i).

                // SelectedMusic(i); //　楽曲再生の実行と停止を行う（1フレーム毎）
                // GetScoresController(i);

                // カードを縮小する
                for (var cnt = 0; cnt < _artworkPositions.Length; cnt++)
                {
                    if (i == cnt) continue;
                    // View.Instance.ArtworkContentGameObject.transform.GetChild(cnt).localScale = Vector2.Lerp(
                    //     View.Instance.ArtworkContentGameObject.transform.GetChild(cnt).localScale,
                    //     new Vector2(1f, 1f), 0.1f);
                    View.Instance.ArtworkContentGameObject.transform.GetChild(cnt).GetComponent<RectTransform>()
                        .sizeDelta = new Vector2(View.Instance.ArtworkHeight, View.Instance.ArtworkHeight);
                }
            }
        }

        private void ArtworkCloner()
        {
            for (var i = 1; i < _musics.Length; i++)
            {
                var clone = Instantiate(View.Instance.ArtworkTemplateGameObject,
                    View.Instance.ArtworkTemplateGameObject.transform.parent, true);
                if (clone is null) continue;
                clone.transform.localPosition = View.Instance.ArtworkTemplateGameObject.transform.localPosition;
                clone.transform.localScale = View.Instance.ArtworkTemplateGameObject.transform.localScale;
            }
            for (var i = 0; i < _artworkPositions.Length; i++)
            {
                View.Instance.ArtworkContentGameObject.transform.GetChild(i).GetComponent<RectTransform>()
                    .sizeDelta = new Vector2(View.Instance.ArtworkHeight, View.Instance.ArtworkHeight);
            }
        }

        private void AttachPreviewMusic()
        {
            _previewAudioClips = new AudioClip[_musics.Length];
            _previewAudioSources = new AudioSource[_musics.Length];
            foreach (var music in _musics)
            {
                _previewAudioClips[music.Id - 1] =
                    _assetBundles[music.Id - 1].LoadAsset<AudioClip>(music.Name + "_pre");
                // _previewAudioSources[music.Id - 1].clip = _previewAudioClips[music.Id - 1];
            }
        }
    }
}