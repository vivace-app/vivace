using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Screen;

namespace Project.Scripts.LoginScreen
{
    public class LoginScreenProcessManager : MonoBehaviour
    {
        public Text showVersion;
        public RectTransform background;
        private bool _touchableFlag;
        private bool _playableFlag;

        // ------------------------------------------------------------------------------------

        private const string ThisVersion = EnvDataStore.ThisVersion;

        // ------------------------------------------------------------------------------------

        private FirebaseApp app;
        private FirebaseAuth auth;
        private IAppleAuthManager appleAuthManager;
        private FirebaseUser user;

        public async void Start()
        {
            ScreenResponsive();
            showVersion.text = "Ver." + ThisVersion;
            
            await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    app = FirebaseApp.DefaultInstance;
                    auth = FirebaseAuth.DefaultInstance;
                    auth.StateChanged += AuthStateChanged;
                    AuthStateChanged(this, null);

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                    StartWithSIWA();
                }
                else
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    // Firebase Unity SDK is not safe to use here.
                }
            });
        }

        void AuthStateChanged(object sender, EventArgs eventArgs)
        {
            if (auth.CurrentUser != user)
            {
                bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
                if (!signedIn && user != null)
                {
                    Debug.Log("Signed out " + user.UserId);
                }

                user = auth.CurrentUser;
                if (signedIn)
                {
                    Debug.Log("Signed in " + user.UserId);
                    Debug.Log("DisplayName " + user.DisplayName);
                    Debug.Log("Email " + user.Email);
                    Debug.Log("PhotoUrl " + user.PhotoUrl);
                    Debug.Log("IsEmailVerified " + user.IsEmailVerified);
                    Debug.Log("ProviderId " + user.ProviderId);
                    Debug.Log("UserId " + user.UserId);
                }
            }
        }

        private void Update()
        {
            UpdateWithSIWA();
        }

        private void ScreenResponsive()
        {
            var scale = 1f;
            if (width < 1920)
                scale = 1.5f;
            if (width < height)
                scale = height * 16 / (width * 9);
            background.sizeDelta = new Vector2(width * scale, height * scale);
        }

        // Start()で呼び出す初期化処理
        private void StartWithSIWA()
        {
            // Sign In with Apple認証がサポートされている端末なら認証用のインスタンスを初期化する
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                var deserializer = new PayloadDeserializer();
                this.appleAuthManager = new AppleAuthManager(deserializer);
            }

            Invoke(nameof(SignInWithApple), 3.5f);
        }

        // Update()で呼び出す処理
        private void UpdateWithSIWA()
        {
            // SignInwithAppleを成功させる為にAppleAuthManagerのUpdate()をUpdate()で呼び続ける必要がある
            if (this.appleAuthManager != null)
            {
                this.appleAuthManager.Update();
            }
        }

        private static string GenerateRandomString(int length)
        {
            if (length <= 0)
            {
                throw new Exception("Expected nonce to have positive length");
            }

            const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
            var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
            var result = string.Empty;
            var remainingLength = length;

            var randomNumberHolder = new byte[1];
            while (remainingLength > 0)
            {
                var randomNumbers = new List<int>(16);
                for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++)
                {
                    cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                    randomNumbers.Add(randomNumberHolder[0]);
                }

                for (var randomNumberIndex = 0; randomNumberIndex < randomNumbers.Count; randomNumberIndex++)
                {
                    if (remainingLength == 0)
                    {
                        break;
                    }

                    var randomNumber = randomNumbers[randomNumberIndex];
                    if (randomNumber < charset.Length)
                    {
                        result += charset[randomNumber];
                        remainingLength--;
                    }
                }
            }

            return result;
        }

        private static string GenerateSHA256NonceFromRawNonce(string rawNonce)
        {
            var sha = new SHA256Managed();
            var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
            var hash = sha.ComputeHash(utf8RawNonce);

            var result = string.Empty;
            for (var i = 0; i < hash.Length; i++)
            {
                result += hash[i].ToString("x2");
            }

            return result;
        }

        // Sign In with Apple認証の実行
        public void SignInWithApple()
        {
            Debug.Log("ON CLICKED");
            var rawNonce = GenerateRandomString(32);
            var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);
            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName, nonce);
            // iOSクライアントでSIWA認証を実行し、成功時のcredentialとNonceを使用してFirebaseAuthを実行する
            this.appleAuthManager.LoginWithAppleId(loginArgs,
                credential =>
                {
                    var appleIdCredential = credential as IAppleIDCredential;
                    Debug.Log(String.Format("appleIdCredential:{0} | rawNonce:{1}", appleIdCredential, rawNonce));

                    if (appleIdCredential != null)
                    {
                        Debug.Log(String.Format("appleIdCredential:{0} | rawNonce:{1}", appleIdCredential, rawNonce));
                        var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                        var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                        
                        auth.SignInAndRetrieveDataWithCredentialAsync(
                            Firebase.Auth.OAuthProvider.GetCredential("apple.com", identityToken, rawNonce, authorizationCode)).ContinueWithOnMainThread(
                            HandleSignInWithUser);
                        
                        // Credential firebaseCredential =
                        //     OAuthProvider.GetCredential("apple.com", identityToken, rawNonce, authorizationCode);
                        // SignInOrLinkCredentialAsync(firebaseCredential);
                    }
                },
                error =>
                {
                    // Something went wrong
                    var authorizationErrorCode = error.GetAuthorizationErrorCode();
                    Debug.Log("SignInWithApple error:" + authorizationErrorCode);
                }
            );
        }

        // ゲストユーザーで認証済みの時はリンクさせる、新規の場合はそのまま認証完了
        // private void SignInOrLinkCredentialAsync(Credential firebaseCredential)
        // {
        //     var task = auth.SignInWithCredentialAsync(firebaseCredential);
        //     if (task.Exception != null)
        //         Debug.Log("GC Credential Task - Exception: " + task.Exception.Message);
        //
        //     task.ContinueWithOnMainThread(HandleSignInWithUser);
        // }
        
        // Called when a sign-in without fetching profile data completes.
        private static void HandleSignInWithUser(Task<Firebase.Auth.SignInResult> task)
        {
            if (task.IsCanceled)
            {
                Debug.Log("Firebase auth was canceled");
            }
            else if (task.IsFaulted)
            {
                Debug.Log("Firebase auth failed");
            }
            else
            {
                var firebaseUser = task.Result.Info;
                Debug.Log("Firebase auth completed | User ID:" + firebaseUser.UserName);
            }
            
            // string indent = new String(' ', indentLevel * 2);
            // var metadata = result.Meta;
            // if (metadata != null)
            // {
            //     Debug.Log(String.Format("{0}Created: {1}", indent, metadata.CreationTimestamp));
            //     Debug.Log(String.Format("{0}Last Sign-in: {1}", indent, metadata.LastSignInTimestamp));
            // }
            // var info = result.Info;
            // if (info != null)
            // {
            //     Debug.Log(String.Format("{0}Additional User Info:", indent));
            //     Debug.Log(String.Format("{0}  User Name: {1}", indent, info.UserName));
            //     Debug.Log(String.Format("{0}  Provider ID: {1}", indent, info.ProviderId));
            //     DisplayProfile<string>(info.Profile, indentLevel + 1);
            // }
        }
        
        // Log the result of the specified task, returning true if the task
        // completed successfully, false otherwise.
        protected bool LogTaskCompletion(Task task, string operation)
        {
            bool complete = false;
            if (task.IsCanceled)
            {
                Debug.Log(operation + " canceled.");
            }
            else if (task.IsFaulted)
            {
                Debug.Log(operation + " encounted an error.");
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    string authErrorCode = "";
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        authErrorCode = String.Format("AuthError.{0}: ",
                            ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                    }
                    Debug.Log(authErrorCode + exception.ToString());
                }
            }
            else if (task.IsCompleted)
            {
                Debug.Log(operation + " completed");
                complete = true;
            }
            return complete;
        }
    }
}