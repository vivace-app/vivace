using System.Text;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Firebase.Auth;
using UnityEngine;

namespace Project.Scripts.LoginScreen
{
    public partial class Authentication
    {
        private IAppleAuthManager _appleAuthManager;

        private void InitializeSignInWithApple()
        {
            if (!AppleAuthManager.IsCurrentPlatformSupported) return;
            var deserializer = new PayloadDeserializer();
            _appleAuthManager = new AppleAuthManager(deserializer);
        }
        
        private void UpdateSignInWithApple() => _appleAuthManager?.Update();

        public void SignInWithApple()
        {
            var rawNonce = GenerateRandomString(32);
            var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);
            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName, nonce);
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
                    Debug.Log("AppleSignIn was error.");
                    Debug.Log(e.GetAuthorizationErrorCode());
                }
            );
        }
    }
}