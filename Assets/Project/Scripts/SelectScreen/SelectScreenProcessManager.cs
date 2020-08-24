using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class SelectScreenProcessManager : MonoBehaviour
{
    public ToggleGroup toggleGroup;

    private async void ScreenTransition()
    {
        Debug.Log("さぁ，はじまるドン！");
        await Task.Delay(1000);
        SceneManager.LoadScene("PlayScene");
    }

    public void PlayButtonTappedController()
    {
        string selectedLabel = toggleGroup.ActiveToggles()
            .First().GetComponentsInChildren<Text>()
            .First(t => t.name == "Label").text;

        Debug.Log("selected " + selectedLabel);
        ScreenTransition();
    }
}