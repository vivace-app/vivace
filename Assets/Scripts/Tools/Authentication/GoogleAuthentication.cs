using CandyCoded.env;
using Firebase.Auth;
using Google;
using UnityEngine;

namespace Tools.Authentication
{
    public partial class AuthenticationHandler
    {
        private static void InitializeSignInWithGoogle()
        {
            if (env.TryParseEnvironmentVariable("CLIENT_ID", out string clientId))
            {
                GoogleSignIn.Configuration = new GoogleSignInConfiguration
                {
                    RequestIdToken = true,
                    WebClientId = clientId
                };
            }
            else
            {
                Debug.LogError(".env ファイル内の 'CLIENT_ID' が設定されていません");
            }
        }

        private void _SignInWithGoogle()
        {
            var signIn = GoogleSignIn.DefaultInstance.SignIn();

            signIn.ContinueWith(task =>
            {
                if (task.IsFaulted)
                    OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
                else
                    _auth.SignInAndRetrieveDataWithCredentialAsync(
                        GoogleAuthProvider.GetCredential(task.Result.IdToken, null));
            });
        }
    }
}