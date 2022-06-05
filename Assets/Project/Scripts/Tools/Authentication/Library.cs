using System;
using Firebase;
using Firebase.Auth;
using Project.Scripts.LoginScene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Tools.Authentication
{
    public partial class AuthenticationHandler
    {
        private FirebaseAuth _auth;
        private FirebaseUser _user;

        private void _InitializeFirebase(EventHandler authStateChangedHandler)
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    _auth = FirebaseAuth.DefaultInstance;
                    _auth.StateChanged += _AuthStateHandler;
                    _AuthStateHandler(this, null);
                    _auth.StateChanged += authStateChangedHandler;

                    // Firebase is ready to use.
                    InitializeSignInWithApple();
                    InitializeSignInWithGoogle();
                }
                else
                    OnErrorOccured.Invoke("Firebaseの初期化に失敗しました");
            });
        }

        private void _DestroyFirebase(EventHandler authStateChangedHandler)
        {
            _auth.StateChanged -= _AuthStateHandler;
            _auth.StateChanged -= authStateChangedHandler;
            _auth = null;
        }

        private void _AuthStateHandler(object sender, EventArgs eventArgs)
        {
            if (_auth.CurrentUser == _user) return;
            var signedIn = _user != _auth.CurrentUser && _auth.CurrentUser != null;

            // Sign out
            if (!signedIn && _user != null) Debug.Log("Signed out: " + _user.UserId);
            _user = _auth.CurrentUser;

            // Sign in
            if (!signedIn) return;
            Debug.Log("Signed in: " + _user.UserId);
        }

        private void _UpdateDisplayName(string displayName)
        {
            if (_user == null) return;
            var profile = new UserProfile
            {
                DisplayName = displayName
            };
            _user.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    OnErrorOccured.Invoke("ユーザ名の更新がキャンセルされました");
                    return;
                }

                if (!task.IsFaulted) return;
                OnErrorOccured.Invoke("ユーザ名の更新に失敗しました");
            });
        }

        private void _SignOut()
        {
            _auth.SignOut();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}