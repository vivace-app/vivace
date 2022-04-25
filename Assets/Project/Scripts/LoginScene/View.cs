﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.LoginScene
{
    /// <summary>
    /// Login Scene 内のコンテンツを書き換えます。
    /// </summary>
    public class View : MonoBehaviour
    {
        public static View instance;

        public InputField displayNameInputField;
        public Text completionRateText;
        public Text creditText;
        public Text displayNameText;
        public Text uidText;
        public Text versionText;

        private void Awake()
        {
            if (instance == null) instance = this;
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