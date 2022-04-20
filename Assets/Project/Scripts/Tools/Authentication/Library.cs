using System;
using Firebase;
using Firebase.Auth;
using Project.Scripts.LoginScene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Tools.Authentication
{
    /// <summary>
    /// ユーザ認証周りのライブラリです。
    /// </summary>
    public partial class Main
    {
        private FirebaseAuth _auth;

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
            if (_auth.CurrentUser == User) return;
            var signedIn = User != _auth.CurrentUser && _auth.CurrentUser != null;

            // Sign out
            if (!signedIn && User != null) Debug.Log("Signed out: " + User.UserId);
            User = _auth.CurrentUser;

            // Sign in
            if (!signedIn) return;
            Debug.Log("Signed in: " + User.UserId);
            View.instance.UidText = User.UserId ?? "No credentials";
            View.instance.DisplayNameText = User.DisplayName ?? "No Name";
        }

        private void UpdateDisplayName()
        {
            if (User == null) return;
            var profile = new UserProfile
            {
                DisplayName = View.instance.DisplayNameInputField,
            };
            User.UpdateUserProfileAsync(profile).ContinueWith(task =>
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
                View.instance.UidText = User.UserId ?? "No credentials";
                View.instance.DisplayNameText = User.DisplayName ?? "No Name";
            });
        }

        private void SignOut()
        {
            _auth.SignOut();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}