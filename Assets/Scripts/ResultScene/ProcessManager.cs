using UnityEngine;
using UnityEngine.SceneManagement;

namespace ResultScene
{
    public class ProcessManager : MonoBehaviour
    {
        private void Start()
        {
            if (Application.isEditor) LocaleSetting.ChangeSelectedLocale("ja");

            View.Instance.RetryButtonAction = () => SceneManager.LoadScene("PlayScene");
            View.Instance.ExitButtonAction = () => SceneManager.LoadScene("SelectScene");
        }
    }
}