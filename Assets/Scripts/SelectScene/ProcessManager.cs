using System;
using System.Collections;
using Firebase.Auth;
using Tools.AssetBundle;
using Tools.Authentication;
using Tools.Firestore.Model;
using UnityEngine;
using UnityEngine.UI;

namespace SelectScene
{
    public class ProcessManager : MonoBehaviour
    {
        private readonly AuthenticationHandler _auth = new();

        private AssetBundle[] _assetBundles;

        private AudioClip[] _previewAudioClips;

        // private AudioSource[] _previewAudioSources;
        private Music[] _musics;

        private float _artworkDistance;
        private float[] _artworkPositions;
        private float _scrolledPosition;
        private int _selectedMusic;


        private void Start()
        {
            if (Application.isEditor) LocaleSetting.ChangeSelectedLocale("ja");

            _auth.Start(_setUserData);
            _setUserData(this, null);

            _assetBundles = AssetBundleHandler.GetAssetBundles();
            _musics = AssetBundleHandler.GetMusics();

            _artworkPositions = new float[_musics.Length];
            _artworkDistance = 1f / (_artworkPositions.Length - 1f);
            for (var i = 0; i < _artworkPositions.Length; i++)
            {
                _artworkPositions[i] = _artworkDistance * i; // TODO: リファクタ
            }

            ArtworkCloner();
            AttachArtworks();
            AttachPreviewMusics();
            View.Instance.Achievement = new[] { 3, 2, 1, 1 };
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
                View.Instance.ArtworkContentGameObject.transform.GetChild(i).GetComponent<RectTransform>()
                    .sizeDelta = new Vector2(View.Instance.ArtworkHeight * 1.5f, View.Instance.ArtworkHeight * 1.5f);

                PlaySelectedMusic(i);
                DisplayMusicData(i);

                // カードを縮小する
                for (var cnt = 0; cnt < _artworkPositions.Length; cnt++)
                {
                    if (i == cnt) continue;
                    View.Instance.ArtworkContentGameObject.transform.GetChild(cnt).GetComponent<RectTransform>()
                        .sizeDelta = new Vector2(View.Instance.ArtworkHeight, View.Instance.ArtworkHeight);
                }
            }
        }

        private void _setUserData(object sender, EventArgs eventArgs) => StartCoroutine(nameof(GetAuth));

        private IEnumerator GetAuth()
        {
            var user = _auth.GetUser();
            var iEnumerator = _auth.GenerateCustomToken();
            yield return iEnumerator;

            View.Instance.NicknameText = user?.DisplayName;
            View.Instance.ProfileCustomButton = () =>
                Application.OpenURL("http://localhost:3000/api/redirect/profile/" + iEnumerator.Current);
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

        private void AttachArtworks()
        {
            foreach (var music in _musics)
            {
                var artworkSprite =
                    _assetBundles[music.Id - 1].LoadAsset<Sprite>(music.Name + "_artwork");
                View.Instance.ArtworkContentGameObject.transform.GetChild(music.Id - 1).GetComponent<Image>().sprite =
                    artworkSprite;
            }
        }

        private void AttachPreviewMusics()
        {
            for (var i = 0; i < _musics.Length; i++) View.Instance.Musics.AddComponent<AudioSource>();

            _previewAudioClips = new AudioClip[_musics.Length];
            foreach (var music in _musics)
            {
                _previewAudioClips[music.Id - 1] =
                    _assetBundles[music.Id - 1].LoadAsset<AudioClip>(music.Name + "_pre");
                View.Instance.Musics.GetComponents<AudioSource>()[music.Id - 1].clip = _previewAudioClips[music.Id - 1];
            }
        }

        private void PlaySelectedMusic(int num) // numは現在選択中の楽曲通し番号
        {
            for (var i = 0; i < _artworkPositions.Length; i++)
            {
                if (i == num)
                {
                    if (!View.Instance.Musics.GetComponents<AudioSource>()[i].isPlaying)
                        View.Instance.Musics.GetComponents<AudioSource>()[i].Play();
                }
                else
                {
                    if (View.Instance.Musics.GetComponents<AudioSource>()[i].isPlaying)
                        View.Instance.Musics.GetComponents<AudioSource>()[i].Stop();
                }
            }
        }

        private void DisplayMusicData(int num) // numは現在選択中の楽曲通し番号
        {
            View.Instance.ArtistText = _musics[num].Artist;
            View.Instance.MusicTitleText = _musics[num].Title;
        }
    }
}