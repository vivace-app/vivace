using Project.Scripts.LoginScene.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.LoginScene
{
    /// <summary>
    /// Login Scene で呼び出す動作をここに書きます。
    /// </summary>
    public class ProcessManager : MonoBehaviour
    {
        private readonly Auth _auth = new Auth();

        private void Start()
        {
            _auth.Start();
        }

        private void Update()
        {
            _auth.Update();
        }

        private void OnDestroy()
        {
            _auth.OnDestroy();
        }

        public void OnClickSignInWithApple() => _auth.OnClickSignInWithApple();
        public void OnClickSignInWithGoogleButton() => _auth.OnClickSignInWithGoogleButton();
        public void OnClickUpdateDisplayNameButton() => _auth.OnClickUpdateDisplayNameButton();
        public void OnClickSignOutButton() => _auth.OnClickSignOutButton();
        public void OnClickIgnoreAndPlayButton() => SceneManager.LoadScene("StartupScene");
    }
}