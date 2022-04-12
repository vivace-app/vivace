using UnityEngine;

namespace Project.Scripts.LoginScreen
{
    public partial class Authentication : MonoBehaviour
    {
        private void Start()
        {
            InitializeFirebase();
        }

        private void Update()
        {
            UpdateSignInWithApple();
        }

        private void OnDestroy()
        {
            DestroyFirebase();
        }
    }
}