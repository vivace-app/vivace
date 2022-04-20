using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Auth = Project.Scripts.Tools.Authentication.Main;

namespace Project.Scripts.SelectScreen
{
    public class SelectScreenProcessManager : MonoBehaviour
    {
        public ToggleGroup toggleGroup;
        public static string selectedLevel;

        private readonly Auth _auth = new Auth();

        private void Start()
        {
            _auth.Start();

            var user = _auth.User;
            if (user != null)
            {
                var displayName = user.DisplayName;
                var userId = user.UserId;

                Debug.Log("name: " + displayName);
                Debug.Log("uid: " + userId);
            }
        }

        private void Update()
        {
            _auth.Update();
        }

        private void OnDestroy()
        {
            _auth.OnDestroy();
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