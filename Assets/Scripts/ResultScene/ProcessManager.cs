using UnityEngine;
using UnityEngine.SceneManagement;

namespace ResultScene
{
    public class ResultScreenProcessManager : MonoBehaviour
    {
        private void Start()
        {
            View.Instance.RetryButtonAction = () => SceneManager.LoadScene("PlayScene");
            View.Instance.ExitButtonAction = () => SceneManager.LoadScene("SelectScene");
        }
    }
}