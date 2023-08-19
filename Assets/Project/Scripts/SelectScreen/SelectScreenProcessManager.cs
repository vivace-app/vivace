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
            Application.OpenURL("https://vivace-app.com");
        }

        public void SupportButtonTappedController()
        {
            Application.OpenURL("https://vivace-app.com/contact");
        }
    }
}