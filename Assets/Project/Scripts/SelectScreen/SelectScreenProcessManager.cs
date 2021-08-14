using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class SelectScreenProcessManager : MonoBehaviour
{
    public ToggleGroup toggleGroup;
    public static string selectedLevel;

    private void ScreenTransition()
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
            "https://lab316.github.io/app-static-page/ja/privacy.html?company=vivace,%20inc.&department=%E9%96%8B%E7%99%BA%E6%8B%85%E5%BD%93%E8%80%85&email=developer.ikep%40gmail.com");
    }

    public void SupportButtonTappedController()
    {
        Application.OpenURL("https://forms.gle/qS1RevqH7iHvjk6B6");
    }
}