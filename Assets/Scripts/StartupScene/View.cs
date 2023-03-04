using System;
using CriWare;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        [SerializeField] private GameObject sliderGameObject;

        [SerializeField] private CustomButton signInWithAppleCustomButton;
        [SerializeField] private CustomButton signInWithGoogleCustomButton;
        [SerializeField] private CustomButton signInWithAnonymouslyCustomButton;
        [SerializeField] private CustomButton nicknameRegistrationSaveCustomButton;
        [SerializeField] private CustomButton communicationErrorReloadCustomButton;
        [SerializeField] private CustomButton needsUpdateReloadCustomButton;
        [SerializeField] private CustomButton criwareCustomButton;

        [SerializeField] private CriAtomSource startAudioSource;

        [SerializeField] private Slider slider;

        [SerializeField] private TMP_InputField displayNameInputField;

        [SerializeField] private TextMeshProUGUI displayNameErrorText;
        [SerializeField] private TextMeshProUGUI uidText;
        [SerializeField] private TextMeshProUGUI versionText;

        private const float SliderFillSpeed = 0.5f;
        private float _sliderTargetProgress;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            InitializeVersion();
            
            // Custom Buttons
            criwareCustomButton.onClickCallback = () => Application.OpenURL("https://www.cri-mw.co.jp");
            communicationErrorReloadCustomButton.onClickCallback =
                () => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            needsUpdateReloadCustomButton.onClickCallback = () => Application.OpenURL("https://coalabo.net/vivace-web");
        }

        private void Update()
        {
            if (slider.value < _sliderTargetProgress)
                slider.value += SliderFillSpeed * Time.deltaTime;
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
                nicknameRegistrationModal.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutCubic);
            }
        }

        public bool setCommunicationErrorModalVisible
        {
            set
            {
                communicationErrorModal.SetActive(value);
                communicationErrorModal.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutCubic);
            }
        }

        public bool setNeedsUpdateModalVisible
        {
            set
            {
                needsUpdateModal.SetActive(value);
                needsUpdateModal.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutCubic);
            }
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

        public float ProgressBar
        {
            set
            {
                _sliderTargetProgress = Mathf.Pow(value, 1 / 2f);
                if (_sliderTargetProgress > 0) sliderGameObject.SetActive(true);
            }
        }
    }
}