using System;
using DG.Tweening;
using Project.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StartupScene
{
    public class View : MonoBehaviour
    {
        public static View Instance;

        [SerializeField] private GameObject accountLinkageModal;
        [SerializeField] private GameObject nicknameRegistrationModal;
        [SerializeField] private GameObject communicationErrorModal;
        [SerializeField] private GameObject needsUpdateModal;

        [SerializeField] private CustomButton signInWithAppleCustomButton;
        [SerializeField] private CustomButton signInWithGoogleCustomButton;
        [SerializeField] private CustomButton signInWithAnonymouslyCustomButton;
        [SerializeField] private CustomButton nicknameRegistrationSaveCustomButton;
        [SerializeField] private CustomButton communicationErrorReloadCustomButton;
        [SerializeField] private CustomButton needsUpdateReloadCustomButton;
        
        [SerializeField] private CriAtomSource startAudioSource;

        [SerializeField] private TMP_InputField displayNameInputField;
        
        [SerializeField] private TextMeshProUGUI displayNameErrorText;
        [SerializeField] private TextMeshProUGUI uidText;
        [SerializeField] private TextMeshProUGUI versionText;
        
        [SerializeField] private Text completionRateText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            InitializeVersion();
        }
        
        private void InitializeVersion() => versionText.text = $"ver.{Application.version}";

        public bool setAccountLinkageModalVisible
        {
            set
            {
                accountLinkageModal.SetActive(value);
                accountLinkageModal.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutCubic);
            }
        }

        public bool setNicknameRegistrationModalVisible
        {
            set
            {
                nicknameRegistrationModal.SetActive(value);
                accountLinkageModal.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutCubic);
            }
        }

        public bool setCommunicationErrorModalVisible
        {
            set => communicationErrorModal.SetActive(value);
        }
        
        public bool setNeedsUpdateModalVisible
        {
            set => needsUpdateModal.SetActive(value);
        }

        public Action setOnClickSignInWithAppleCustomButtonAction
        {
            set => signInWithAppleCustomButton.onClickCallback = value;
        }

        public Action setOnClickSignInWithGoogleCustomButtonAction
        {
            set => signInWithGoogleCustomButton.onClickCallback = value;
        }

        public Action setOnClickSignInWithAnonymouslyCustomButtonAction
        {
            set => signInWithAnonymouslyCustomButton.onClickCallback = value;
        }

        public Action setOnClickNicknameRegistrationSaveCustomButtonCustomButtonAction
        {
            set => nicknameRegistrationSaveCustomButton.onClickCallback = value;
        }

        public CriAtomSource StartAudioSource => startAudioSource;

        public string DisplayNameInputField
        {
            get => displayNameInputField.text;
            set => displayNameInputField.text = value;
        }

        public string DisplayNameErrorText
        {
            set => displayNameErrorText.text = value;
        }

        public string UidText
        {
            set => uidText.text = value;
        }
        
        public string CompletionRateText
        {
            set => completionRateText.text = value;
        }
    }
}