using System;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.LoginScreen
{
    public class GraphicInitializer : MonoBehaviour
    {
        public Text credit;
        public Text version;
        
        private void Start()
        {
            InitializeCredit();
            InitializeVersion();
        }
        
        private void InitializeCredit() => credit.text = $"(c) {DateTime.Now.Year} VIVACE PROJECT";
        private void InitializeVersion() => version.text = $"Ver.{EnvDataStore.ThisVersion}";
    }
}