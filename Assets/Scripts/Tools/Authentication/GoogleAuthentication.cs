using Firebase.Auth;
using Google;
using Tools.Config;

namespace Tools.Authentication
{
    public partial class AuthenticationHandler
    {
        private static void InitializeSignInWithGoogle()
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = ConfigProvider.Firebase.clientId
            };
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