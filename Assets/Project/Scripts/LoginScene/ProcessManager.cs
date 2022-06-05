using System;
using System.Collections;
using System.IO;
using Project.Scripts.Tools.AssetBundle;
using Project.Scripts.Tools.Authentication;
using Project.Scripts.Tools.Firestore;
using Project.Scripts.Tools.Firestore.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.LoginScene
{
    public class ProcessManager : MonoBehaviour
    {
        private readonly AuthenticationHandler _auth = new();

        private void Start()
        {
            _auth.Start(_setUserData);
            _setUserData(this, null);
        }

        private void Update()
        {
            _auth.Update();
        }

        private void OnDestroy()
        {
            _auth.OnDestroy(_setUserData);
        }

        public void OnClickSignInWithApple() => _auth.SignInWithApple();
        public void OnClickSignInWithGoogleButton() => _auth.SignInWithGoogle();
        public void OnClickSignInWithAnonymouslyButton() => _auth.SignInWithAnonymously();
        public void OnClickUpdateDisplayNameButton() => _auth.UpdateDisplayName(View.Instance.DisplayNameInputField);

        public void OnClickSignOutButton() => _auth.SignOut();

        public void OnClickIgnoreAndPlayButton() => StartCoroutine(_TransitionToSelectScene());

        private void _setUserData(object sender, EventArgs eventArgs)
        {
            var user = _auth.GetUser();
            View.Instance.UidText = user?.UserId ?? "No credentials";
            View.Instance.DisplayNameText = user?.DisplayName ?? "No Name";
        }

        private IEnumerator _TransitionToSelectScene()
        {
            var db = new FirestoreHandler();

            // Check License
            var ie = db.GetIsSupportedVersionCoroutine(EnvDataStore.ThisVersion);
            yield return ie;
            var isValidLicense = ie.Current != null && (bool)ie.Current;

            if (!isValidLicense)
            {
                // TODO: 最新のバージョンを使用するようにポップアップ
                Debug.LogError("最新のバージョンをご使用ください");
                Application.Quit(); // TMP
            }

            // Get Music List
            ie = db.GetMusicListCoroutine();
            yield return StartCoroutine(ie);
            var musicList = (Music[])ie.Current;

            // Cache Setting
            var cachePath = Path.Combine(Application.persistentDataPath, "cache");
            Directory.CreateDirectory(cachePath);
            var cache = Caching.AddCache(cachePath);
            Caching.currentCacheForWriting = cache;

            // Download Asset Bundles
            var assetBundleHandler = new AssetBundleHandler(musicList);
            assetBundleHandler.OnCompletionRateChanged += rate => View.Instance.CompletionRateText = $"DL: {rate}%";
            assetBundleHandler.OnDownloadCompleted += () => SceneManager.LoadScene("SelectScene");
            assetBundleHandler.OnErrorOccured += error =>
            {
                // TODO: エラーを出力する
                SceneManager.LoadScene("StartupScene");
            };
            var downloadEnumerator = assetBundleHandler.DownloadCoroutine();
            yield return StartCoroutine(downloadEnumerator);
        }
    }
}