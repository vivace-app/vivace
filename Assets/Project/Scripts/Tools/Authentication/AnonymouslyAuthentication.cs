using UnityEngine;

namespace Project.Scripts.Tools.Authentication
{
    /// <summary>
    /// 匿名認証でサインインの処理関係です。
    /// </summary>
    public partial class Main
    {
        private void SignInWithAnonymously()
        {
            _auth.SignInAnonymouslyAsync().ContinueWith(task =>
            {
                if (task.IsCanceled) Debug.Log("Sign in with Anonymously was canceled.");
                else if (task.IsFaulted) Debug.LogError("Sign in with Anonymously was error.");
            });
        }
    }
}