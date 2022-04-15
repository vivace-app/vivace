using Firebase.Auth;

namespace Project.Scripts.Authentication
{
    /// <summary>
    /// ユーザ認証周りのライブラリです。
    /// </summary>
    public partial class Main
    {
        public FirebaseUser User { get; private set; }
        
        public void Start() => InitializeFirebase(); // required
        public void Update() => UpdateSignInWithApple(); // only when making a sign-in call with Apple
        public void OnDestroy() => DestroyFirebase(); // required

        public void OnClickSignInWithApple() => SignInWithApple();
        public void OnClickSignInWithGoogleButton() => SignInWithGoogle();
        public void OnClickUpdateDisplayNameButton() => UpdateDisplayName();
        public void OnClickSignOutButton() => SignOut();
    }
}