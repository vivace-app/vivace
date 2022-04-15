using Firebase.Auth;
using Google;
using UnityEngine;

namespace Project.Scripts.LoginScene.Authentication
{
    /// <summary>
    /// Googleでサインインの処理関係です。
    /// </summary>
    public partial class Auth
    {
        private static void InitializeSignInWithGoogle()
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = EnvDataStore.GoogleClientId
            };
        }

        private void SignInWithGoogle()
        {
            var signIn = GoogleSignIn.DefaultInstance.SignIn();

            signIn.ContinueWith(task =>
            {
                if (task.IsCanceled)
                    Debug.Log("Sign in with Google was canceled.");
                else if (task.IsFaulted)
                    Debug.LogError("Sign in with Google was error.");
                else
                    _auth.SignInAndRetrieveDataWithCredentialAsync(
                        GoogleAuthProvider.GetCredential(task.Result.IdToken, null));
            });
        }
    }
}