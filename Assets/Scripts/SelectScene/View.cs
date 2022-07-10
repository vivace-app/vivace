using Unity.VectorGraphics;
using UnityEngine;

namespace SelectScene
{
    public class View : MonoBehaviour
    {
        public static View Instance;

        [SerializeField] private RectTransform artworkBackgroundSvgImage;
        [SerializeField] private RectTransform artworkBackgroundBottomSvgImage;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            InitializeArtworkBackground();
        }

        private void InitializeArtworkBackground()
        {
            if (Screen.width * 1080 >= Screen.height * 1920)
            {
                // 横長スクリーンの場合 
                Debug.Log("横長");
                var setBackgroundHeight = -900f * (Screen.height / 2048f) + 920f;
                artworkBackgroundSvgImage.sizeDelta =
                    new Vector2(artworkBackgroundSvgImage.sizeDelta.x, setBackgroundHeight);

                var backgroundPosition = artworkBackgroundSvgImage.anchoredPosition;
                backgroundPosition.y = -107f - setBackgroundHeight / 2f;
                artworkBackgroundSvgImage.anchoredPosition = backgroundPosition;

                var setBackgroundBottomHeight = setBackgroundHeight * 0.45f;
                artworkBackgroundBottomSvgImage.sizeDelta =
                    new Vector2(setBackgroundBottomHeight * 5f, setBackgroundBottomHeight);

                var backgroundBottomPosition = artworkBackgroundBottomSvgImage.anchoredPosition;
                backgroundBottomPosition.y = -107f - setBackgroundHeight - setBackgroundBottomHeight / 2f;
                artworkBackgroundBottomSvgImage.anchoredPosition = backgroundBottomPosition;
            }
            else
            {
                // 縦長スクリーンの場合 
                Debug.Log("縦長");
                var setBackgroundHeight = 2439f * (Screen.height / 2048f) - 985.3f;
                artworkBackgroundSvgImage.sizeDelta =
                    new Vector2(artworkBackgroundSvgImage.sizeDelta.x, setBackgroundHeight);

                var backgroundPosition = artworkBackgroundSvgImage.anchoredPosition;
                backgroundPosition.y = -107f - setBackgroundHeight / 2f;
                artworkBackgroundSvgImage.anchoredPosition = backgroundPosition;

                artworkBackgroundBottomSvgImage.sizeDelta =
                    new Vector2(setBackgroundHeight + 400f, artworkBackgroundBottomSvgImage.sizeDelta.y);

                var backgroundBottomPosition = artworkBackgroundBottomSvgImage.anchoredPosition;
                backgroundBottomPosition.y = -107f - setBackgroundHeight - 89.5f;
                artworkBackgroundBottomSvgImage.anchoredPosition = backgroundBottomPosition;
            }
        }
    }
}