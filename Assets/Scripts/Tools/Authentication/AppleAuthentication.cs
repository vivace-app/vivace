using System.Collections.Generic;
using System.Text;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Firebase.Auth;
using UnityEngine;

namespace Project.Scripts.Tools.Authentication
{
    public partial class AuthenticationHandler
    {
        private IAppleAuthManager _appleAuthManager;

        private void InitializeSignInWithApple()
        {
            if (!AppleAuthManager.IsCurrentPlatformSupported) return;
            var deserializer = new PayloadDeserializer();
            _appleAuthManager = new AppleAuthManager(deserializer);
        }

        private void _UpdateSignInWithApple() => _appleAuthManager?.Update();

        private void _SignInWithApple()
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
                            if (credential is not IAppleIDCredential appleIdCredential) return;
                            var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                            var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);

                            _auth.SignInAndRetrieveDataWithCredentialAsync(
                                OAuthProvider.GetCredential("apple.com", identityToken, rawNonce,
                                    authorizationCode));
                        },
                        e => { OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください"); }
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
                        if (task.IsFaulted) OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
                    });
                    break;
                }
                default:
                    OnErrorOccured.Invoke("不明なプラットフォームです");
                    break;
            }
        }
    }
}