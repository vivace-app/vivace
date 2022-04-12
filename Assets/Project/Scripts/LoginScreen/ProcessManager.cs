using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.LoginScreen
{
    public class ProcessManager : MonoBehaviour
    {
        public void SceneTransition() => SceneManager.LoadScene("StartupScene");
    }
}