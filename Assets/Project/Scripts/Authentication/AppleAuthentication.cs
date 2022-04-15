using System.Collections.Generic;
using System.Text;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Firebase.Auth;
using UnityEngine;

namespace Project.Scripts.Authentication
{
    /// <summary>
    /// Appleでサインインの処理関係です。
    /// </summary>
    public partial class Main
    {
        private IAppleAuthManager _appleAuthManager;

        private void InitializeSignInWithApple()
        {
            if (!AppleAuthManager.IsCurrentPlatformSupported) return;
            var deserializer = new PayloadDeserializer();
            _appleAuthManager = new AppleAuthManager(deserializer);
        }

        private void UpdateSignInWithApple() => _appleAuthManager?.Update();

        private void SignInWithApple()
        {
            switch (Application.platform)
            {
                // iOS
                case RuntimePlatform.IPhonePlayer:
                {
                    var rawNonce = GenerateRandomString(32);
                    var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);
                    var loginArgs =
                        new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName, nonce);
                    _appleAuthManager.LoginWithAppleId(loginArgs,
                        credential =>
                        {
                            if (!(credential is IAppleIDCredential appleIdCredential)) return;
                            var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                            var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);

                            _auth.SignInAndRetrieveDataWithCredentialAsync(
                                OAuthProvider.GetCredential("apple.com", identityToken, rawNonce,
                                    authorizationCode));
                        },
                        e =>
                        {
                            Debug.LogError("Sign in with Apple was error.");
                            Debug.LogError(e.GetAuthorizationErrorCode());
                        }
                    );
                    break;
                }
                // Android
                case RuntimePlatform.Android:
                {
                    var providerData = new FederatedOAuthProviderData();

                    providerData.ProviderId = "apple.com";
                    providerData.CustomParameters = new Dictionary<string, string>();
                    providerData.CustomParameters.Add("language", "ja");

                    var provider = new FederatedOAuthProvider();
                    provider.SetProviderData(providerData);

                    _auth.SignInWithProviderAsync(provider).ContinueWith(task =>
                    {
                        if (task.IsCanceled) Debug.Log("Sign in with Apple was canceled.");
                        else if (task.IsFaulted) Debug.LogError("Sign in with Apple was error.");
                    });
                    break;
                }
                default:
                    Debug.LogError("No applicable platform.");
                    break;
            }
        }
    }
}