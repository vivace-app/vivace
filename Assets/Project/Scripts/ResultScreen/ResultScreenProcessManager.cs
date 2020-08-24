using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultScreenProcessManager : MonoBehaviour
{
    private void PlayScreenTransition()
    {
        SceneManager.LoadScene("PlayScene");
    }

    private void SelectScreenTransition()
    {
        SceneManager.LoadScene("SelectScene");
    }

    public void RetryButtonTappedController()
    {
        PlayScreenTransition();
    }

    public void ExitButtonTappedController()
    {
        SelectScreenTransition();
    }
}
