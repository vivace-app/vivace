namespace Tools.Authentication
{
    public partial class AuthenticationHandler
    {
        private void _SignInWithAnonymously()
        {
            _auth.SignInAnonymouslyAsync().ContinueWith(task =>
            {
                if (task.IsFaulted) OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
            });
        }
    }
}