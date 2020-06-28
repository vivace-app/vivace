using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectScreenProcessManager : MonoBehaviour {
    private async void ScreenTransition () {
        Debug.Log ("さぁ，はじまるドン！");
        await Task.Delay (1000);
        SceneManager.LoadScene ("PlayScene");
    }

    public void PlayButtonTappedController () {
        ScreenTransition ();
    }
}