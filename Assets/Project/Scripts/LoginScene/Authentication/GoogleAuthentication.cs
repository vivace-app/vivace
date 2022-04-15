using Firebase.Auth;
using Google;
using UnityEngine;

namespace Project.Scripts.LoginScene.Authentication
{
    public partial class Authentication
    {
        private static void InitializeSignInWithGoogle()
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = EnvDataStore.GoogleClientId
            };
        }

        public void SignInWithGoogle()
        {
            var signIn = GoogleSignIn.DefaultInstance.SignIn();

            signIn.ContinueWith(task =>
            {
                if (task.IsCanceled)
                    Debug.Log("GoogleSignIn was canceled.");
                else if (task.IsFaulted)
                    Debug.Log("GoogleSignIn was error.");
                else
                    _auth.SignInAndRetrieveDataWithCredentialAsync(
                        GoogleAuthProvider.GetCredential(task.Result.IdToken, null));
            });
        }
    }
}