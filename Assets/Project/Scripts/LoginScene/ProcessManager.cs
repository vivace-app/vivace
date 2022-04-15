using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.LoginScene
{
    public class ProcessManager : MonoBehaviour
    {
        public void SceneTransition() => SceneManager.LoadScene("StartupScene");
    }
}