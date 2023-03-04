using System;
using System.Collections;
using Firebase.Auth;

namespace Tools.Authentication
{
    public partial class AuthenticationHandler
    {
        public void Start(EventHandler authStateChangedHandler) => _InitializeFirebase(authStateChangedHandler);

        public void Update() => _UpdateSignInWithApple();

        public void OnDestroy(EventHandler authStateChangedHandler) => _DestroyFirebase(authStateChangedHandler);


        public void SignInWithApple() => _SignInWithApple();

        public void SignInWithGoogle() => _SignInWithGoogle();

        public void SignInWithAnonymously() => _SignInWithAnonymously();

        public IEnumerator GenerateCustomToken() => _GenerateCustomToken();

        public void UpdateDisplayName(string displayName) => _UpdateDisplayName(displayName);

        public void SignOut() => _SignOut();

        public FirebaseUser GetUser() => _user;

        public event Action<string> OnErrorOccured;
    }
}