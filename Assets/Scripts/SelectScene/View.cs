using System;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SelectScene
{
    public class View : MonoBehaviour
    {
        public static View Instance;

        [SerializeField] private CustomButton menuOpenCustomButton;
        [SerializeField] private CustomButton menuCloseCustomButton;
        [SerializeField] private CustomButton profileCustomButton;
        [SerializeField] private CustomButton rankingCustomButton;
        [SerializeField] private CustomButton settingCustomButton;
        [SerializeField] private CustomButton translateCustomButton;
        [SerializeField] private CustomButton licenseCustomButton;
        [SerializeField] private CustomButton accountLinkageCustomButton;
        [SerializeField] private CustomButton betaCustomButton;
        [SerializeField] private CustomButton supportCustomButton;
        [SerializeField] private CustomButton titleCustomButton;

        [SerializeField] private GameObject artworkContentGameObject;
        [SerializeField] private GameObject artworkTemplateGameObject;
        [SerializeField] private GameObject menuModal;
        [SerializeField] private GameObject musics;

        [SerializeField] private HorizontalLayoutGroup artworkContentHorizontalLayoutGroup;

        [SerializeField] private Image easyAchievementImage;
        [SerializeField] private Image normalAchievementImage;
        [SerializeField] private Image hardAchievementImage;
        [SerializeField] private Image masterAchievementImage;

        [SerializeField] private RectTransform artworkBackgroundRectTransform;
        [SerializeField] private RectTransform artworkBackgroundBottomRectTransform;
        [SerializeField] private RectTransform parentCanvasRectTransform;

        [SerializeField] private Scrollbar scrollbar;

        [SerializeField] private Sprite allPerfectSprite;
        [SerializeField] private Sprite fullComboSprite;
        [SerializeField] private Sprite clearSprite;
        [SerializeField] private Sprite unPlayedSprite;

        [SerializeField] private TextMeshProUGUI artistText;
        [SerializeField] private TextMeshProUGUI musicTitleText;
        [SerializeField] private TextMeshProUGUI nicknameText;

        [SerializeField] private ToggleGroup toggleGroup;

        private float _artworkBackgroundHeight;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            InitializeArtworkBackground();
            InitializeArtworkSize();
            InitializeArtworkBothEndsPadding();

            // Custom Buttons
            menuOpenCustomButton.onClickCallback = () => setNeedsUpdateModalVisible = true;
            menuCloseCustomButton.onClickCallback = () => setNeedsUpdateModalVisible = false;
            titleCustomButton.onClickCallback = () => SceneManager.LoadScene("StartupScene");
        }

        private void InitializeArtworkBackground()
        {
            // アートワーク背景の上半分のサイズ指定
            _artworkBackgroundHeight = parentCanvasRectTransform.sizeDelta.y - 700;
            artworkBackgroundRectTransform.sizeDelta = new Vector2(artworkBackgroundRectTransform.sizeDelta.x,
                _artworkBackgroundHeight * 0.7f);
            artworkBackgroundBottomRectTransform.sizeDelta =
                new Vector2(_artworkBackgroundHeight + 300, _artworkBackgroundHeight * 0.3f);

            // アートワーク背景の下半分のサイズ指定
            var bottomPosition = artworkBackgroundBottomRectTransform.anchoredPosition;
            bottomPosition.y -= _artworkBackgroundHeight * 0.7f - (497f - 106.8f);
            artworkBackgroundBottomRectTransform.anchoredPosition = bottomPosition;
        }

        private void InitializeArtworkSize()
        {
            ArtworkHeight = _artworkBackgroundHeight * 0.5f;
        }

        private void InitializeArtworkBothEndsPadding()
        {
            var padding = (int)parentCanvasRectTransform.sizeDelta.x / 2 - (int)ArtworkHeight * 3 / 4;
            artworkContentHorizontalLayoutGroup.padding.left = padding;
            artworkContentHorizontalLayoutGroup.padding.right = padding;
        }

        public GameObject ArtworkContentGameObject => artworkContentGameObject;
        public GameObject ArtworkTemplateGameObject => artworkTemplateGameObject;
        public GameObject Musics => musics;
        public float ArtworkHeight { get; private set; }

        private bool setNeedsUpdateModalVisible
        {
            set
            {
                menuModal.SetActive(value);
                menuModal.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutCubic);
            }
        }

        public float Scrollbar
        {
            get => scrollbar.value;
            set => scrollbar.value = value;
        }

        public int[] Achievement
        {
            set
            {
                Image[] images =
                    { easyAchievementImage, normalAchievementImage, hardAchievementImage, masterAchievementImage };
                foreach (var image in images.Select((image, index) => new { Value = image, Index = index }))
                {
                    image.Value.sprite = value[image.Index] switch
                    {
                        3 => allPerfectSprite,
                        2 => fullComboSprite,
                        1 => clearSprite,
                        0 => unPlayedSprite,
                        _ => image.Value.sprite
                    };
                }
            }
        }

        public string ArtistText
        {
            set => artistText.text = value;
        }

        public string MusicTitleText
        {
            set => musicTitleText.text = value;
        }

        public string NicknameText
        {
            set => nicknameText.text = value + " さん";
        }

        public Action ProfileCustomButton
        {
            set => profileCustomButton.onClickCallback = value;
        }
    }
}