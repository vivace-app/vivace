using System.Collections;
using System.IO;
using Project.Scripts.Tools.Firestore.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using AB = Project.Scripts.Tools.AssetBundleHandler.Main;
using Auth = Project.Scripts.Tools.Authentication.Main;
using DB = Project.Scripts.Tools.Firestore.Main;

namespace Project.Scripts.LoginScene
{
    /// <summary>
    /// Login Scene で呼び出す動作をここに書きます。
    /// </summary>
    public class ProcessManager : MonoBehaviour
    {
        private readonly Auth _auth = new();

        private void Start()
        {
            _auth.Start();
        }

        private void Update()
        {
            _auth.Update();
        }

        private void OnDestroy()
        {
            _auth.OnDestroy();
        }

        public void OnClickSignInWithApple() => _auth.OnClickSignInWithApple();
        public void OnClickSignInWithGoogleButton() => _auth.OnClickSignInWithGoogleButton();
        public void OnClickSignInWithAnonymouslyButton() => _auth.OnClickSignInWithAnonymouslyButton();
        public void OnClickUpdateDisplayNameButton() => _auth.OnClickUpdateDisplayNameButton();

        public void OnClickSignOutButton() => _auth.OnClickSignOutButton();

        public void OnClickIgnoreAndPlayButton() => StartCoroutine(TransitionToSelectScene());

        private IEnumerator TransitionToSelectScene()
        {
            var db = new DB();

            // Check License
            var ie = db.GetIsValidLicenseCoroutine();
            yield return StartCoroutine(ie);
            var isValidLicense = ie.Current != null && (bool) ie.Current;

            if (!isValidLicense)
            {
                // TODO: 最新のバージョンを使用するようにポップアップ
                Debug.LogError("最新のバージョンをご使用ください");
                Application.Quit(); // TMP
            }

            // Get Music List
            ie = db.GetMusicListCoroutine();
            yield return StartCoroutine(ie);
            var musicList = (Music[]) ie.Current;

            // Cache Setting
            var cachePath = Path.Combine(Application.persistentDataPath, "cache");
            Directory.CreateDirectory(cachePath);
            var cache = Caching.AddCache(cachePath);
            Caching.currentCacheForWriting = cache;

            // Download Asset Bundles
            var assetBundleHandler = new AB(musicList);
            assetBundleHandler.OnCompletionRateChanged += rate => View.instance.CompletionRateText = $"DL: {rate}%";
            assetBundleHandler.OnDownloadCompleted += () => SceneManager.LoadScene("SelectScene");
            var downloadEnumerator = assetBundleHandler.Download();
            yield return StartCoroutine(downloadEnumerator);
        }
    }
}