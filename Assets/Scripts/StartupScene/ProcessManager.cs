using System;
using System.Collections;
using System.IO;
using Tools.AssetBundle;
using Tools.Authentication;
using Tools.Firestore;
using Tools.Firestore.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StartupScene
{
    public class ProcessManager : MonoBehaviour
    {
        private readonly AuthenticationHandler _auth = new();

        private bool _hasPressedStartButton;
        private bool _isFirstRegistration;

        private void Start()
        {
            if (Application.isEditor) LocaleSetting.ChangeSelectedLocale("ja");

            _auth.Start(_setUserData);
            _setUserData(this, null);
            View.Instance.UidText = _auth.GetUser()?.UserId;
            View.Instance.setOnClickSignInWithAppleCustomButtonAction = () => _auth.SignInWithApple();
            View.Instance.setOnClickSignInWithGoogleCustomButtonAction = () => _auth.SignInWithGoogle();
            View.Instance.setOnClickSignInWithAnonymouslyCustomButtonAction = () => _auth.SignInWithAnonymously();
            View.Instance.setOnClickNicknameRegistrationSaveCustomButtonCustomButtonAction =
                () =>
                {
                    if (View.Instance.DisplayNameInputField.Length is 0 or > 10)
                        View.Instance.DisplayNameErrorText = "0文字以上10文字以内で入力してください";
                    else
                    {
                        _auth.UpdateDisplayName(View.Instance.DisplayNameInputField);
                        View.Instance.setNicknameRegistrationModalVisible = false;
                        StartCoroutine(_TransitionToSelectScene());
                    }
                };
        }

        private void Update()
        {
            _auth.Update();

            if (_hasPressedStartButton || Input.touchCount <= 0) return;
            OnPressedStartButton();
            _hasPressedStartButton = true;
        }

        private void OnDestroy()
        {
            _auth.OnDestroy(_setUserData);
        }

        public void OnClickSignOutButton()
        {
            _auth.SignOut();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void _setUserData(object sender, EventArgs eventArgs)
        {
            var user = _auth.GetUser();
            View.Instance.UidText = user?.UserId;
            Debug.Log(user?.UserId != null);
            if (user?.UserId != null)
            {
                View.Instance.setAccountLinkageModalVisible = false;
                Debug.Log("user.DisplayName == null");
                Debug.Log(user.DisplayName == null);
                Debug.Log(user.ProviderData);
                if (_isFirstRegistration)
                {
                    _hasPressedStartButton = true;
                    View.Instance.setNicknameRegistrationModalVisible = true;
                    View.Instance.DisplayNameInputField = user.DisplayName;
                }
                else if (_hasPressedStartButton)
                {
                    StartCoroutine(_TransitionToSelectScene());
                }
            }
        }

        private void OnPressedStartButton()
        {
            View.Instance.StartAudioSource.Play();
            var user = _auth.GetUser();
            if (user?.UserId != null)
                StartCoroutine(_TransitionToSelectScene());
            else
            {
                View.Instance.setAccountLinkageModalVisible = true;
                _isFirstRegistration = true;
            }
        }

        private IEnumerator _TransitionToSelectScene()
        {
            var db = new FirestoreHandler();

            // Check License
            var ie = db.GetIsSupportedVersionCoroutine(Application.version);
            yield return ie;
            var isValidLicense = ie.Current != null && (bool)ie.Current;

            if (!isValidLicense)
                View.Instance.setNeedsUpdateModalVisible = true;

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
            assetBundleHandler.OnCompletionRateChanged += rate => View.Instance.ProgressBar = rate / 100f;
            assetBundleHandler.OnDownloadCompleted += () => SceneManager.LoadScene("SelectScene");
            assetBundleHandler.OnErrorOccured += error => View.Instance.setCommunicationErrorModalVisible = true;
            var downloadEnumerator = assetBundleHandler.DownloadCoroutine();
            yield return StartCoroutine(downloadEnumerator);
        }
    }
}