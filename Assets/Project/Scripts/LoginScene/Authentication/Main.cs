using System;
using Firebase;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.LoginScene.Authentication
{
    /// <summary>
    /// Login Scene で呼び出すユーザ認証周りのライブラリです。
    /// </summary>
    public partial class Auth
    {
        public void Start() => InitializeFirebase();
        public void Update() => UpdateSignInWithApple();
        public void OnDestroy() => DestroyFirebase();

        public void OnClickSignInWithApple() => SignInWithApple();
        public void OnClickSignInWithGoogleButton() => SignInWithGoogle();
        public void OnClickUpdateDisplayNameButton() => UpdateDisplayName();
        public void OnClickSignOutButton() => SignOut();

        private FirebaseAuth _auth;
        private FirebaseUser _user;

        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    _auth = FirebaseAuth.DefaultInstance;
                    _auth.StateChanged += AuthStateHandler;
                    AuthStateHandler(this, null);

                    // Firebase is ready to use.
                    InitializeSignInWithApple();
                    InitializeSignInWithGoogle();
                }
                else
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            });
        }

        private void DestroyFirebase()
        {
            _auth.StateChanged -= AuthStateHandler;
            _auth = null;
        }

        private void AuthStateHandler(object sender, EventArgs eventArgs)
        {
            if (_auth.CurrentUser == _user) return;
            var signedIn = _user != _auth.CurrentUser && _auth.CurrentUser != null;

            // Sign out
            if (!signedIn && _user != null) Debug.Log("Signed out: " + _user.UserId);
            _user = _auth.CurrentUser;

            // Sign in
            if (!signedIn) return;
            Debug.Log("Signed in: " + _user.UserId);
            View.instance.UidText = _user.UserId ?? "No credentials";
            View.instance.DisplayNameText = _user.DisplayName ?? "No Name";
        }

        private void UpdateDisplayName()
        {
            if (_user == null) return;
            var profile = new UserProfile
            {
                DisplayName = View.instance.DisplayNameInputField,
            };
            _user.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");
                View.instance.DisplayNameInputField = null;
                View.instance.UidText = _user.UserId ?? "No credentials";
                View.instance.DisplayNameText = _user.DisplayName ?? "No Name";
            });
        }

        private void SignOut()
        {
            _auth.SignOut();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}