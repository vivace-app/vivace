using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.Scripts.SelectScreen
{
    public class SelectScreenProcessManager : MonoBehaviour
    {
        public ToggleGroup toggleGroup;
        public static string selectedLevel;

        private Firebase.Auth.FirebaseAuth _auth;
        private Firebase.Auth.FirebaseUser _user;

        private void Start() => InitializeFirebase();

        private void InitializeFirebase()
        {
            Debug.Log("Setting up Firebase Auth");
            _auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            _auth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, null);
            
            var user = _auth.CurrentUser;
            if (user == null) return;
            var displayName = user.DisplayName;
            var userId = user.UserId;

            Debug.Log("name: " + displayName);
            Debug.Log("uid: " + userId);
        }

        private void AuthStateChanged(object sender, EventArgs eventArgs)
        {
            if (_auth.CurrentUser == _user) return;
            var signedIn = _user != _auth.CurrentUser && _auth.CurrentUser != null;
            if (!signedIn && _user != null) Debug.Log("Signed out " + _user.UserId);

            _user = _auth.CurrentUser;
            if (signedIn) Debug.Log("Signed in " + _user.UserId);
        }

        private void OnDestroy()
        {
            _auth.StateChanged -= AuthStateChanged;
            _auth = null;
        }

        private static void ScreenTransition()
        {
            SceneManager.LoadScene("PlayScene");
        }

        public void PlayButtonTappedController()
        {
            selectedLevel = toggleGroup.ActiveToggles()
                .First().GetComponentsInChildren<Text>()
                .First(t => t.name == "Label").text;
            ScreenTransition();
        }

        public void PrivacyPolicyButtonTappedController()
        {
            Application.OpenURL(
                "https://github.com/vivace-app/vivace/wiki/VIVACE-%E3%82%A2%E3%83%97%E3%83%AA%E3%82%B1%E3%83%BC%E3%82%B7%E3%83%A7%E3%83%B3-%E3%83%97%E3%83%A9%E3%82%A4%E3%83%90%E3%82%B7%E3%83%BC%E3%83%9D%E3%83%AA%E3%82%B7%E3%83%BC");
        }

        public void SupportButtonTappedController()
        {
            Application.OpenURL("https://forms.gle/qS1RevqH7iHvjk6B6");
        }
    }
}