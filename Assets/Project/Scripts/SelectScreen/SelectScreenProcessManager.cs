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
    public static float _notesSpeedIndex = 5.0f;
    public static int _starttimingIndex = 0;

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
}