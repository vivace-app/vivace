using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.LoginScreen
{
    public class Presenter : MonoBehaviour
    {
        public static Presenter instance;

        public InputField displayNameInputField;
        public Text displayNameText;
        public Text uidText;

        private void Awake()
        {
            if (instance == null) instance = this;
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