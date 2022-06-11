using System;
using Project.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace StartupScene
{
    public class View : MonoBehaviour
    {
        public static View Instance;

        [SerializeField] private InputField displayNameInputField;
        [SerializeField] private Text completionRateText;
        [SerializeField] private Text creditText;
        [SerializeField] private Text displayNameText;
        [SerializeField] private Text uidText;
        [SerializeField] private Text versionText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void InitializeCredit() => creditText.text = $"(c) {DateTime.Now.Year} VIVACE PROJECT";
        private void InitializeVersion() => versionText.text = $"Ver.{EnvDataStore.ThisVersion}";
        
        private void Start()
        {
            InitializeCredit();
            InitializeVersion();
        }

        public string CompletionRateText
        {
            set => completionRateText.text = value;
        }
        
        public string DisplayNameInputField
        {
            get => displayNameInputField.text;
            set => displayNameInputField.text = value;
        }

        public string DisplayNameText
        {
            set => displayNameText.text = value;
        }

        public string UidText
        {
            set => uidText.text = value;
        }
    }
}