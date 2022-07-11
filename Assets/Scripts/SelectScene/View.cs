using UnityEngine;

namespace SelectScene
{
    public class View : MonoBehaviour
    {
        public static View Instance;

        [SerializeField] private RectTransform artworkBackgroundRectTransform;
        [SerializeField] private RectTransform artworkBackgroundBottomRectTransform;
        [SerializeField] private RectTransform canvasRectTransform;

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
            var artworkBackgroundHeight = canvasRectTransform.sizeDelta.y - 700;
            artworkBackgroundRectTransform.sizeDelta = new Vector2(artworkBackgroundRectTransform.sizeDelta.x,
                artworkBackgroundHeight * 0.7f);
            artworkBackgroundBottomRectTransform.sizeDelta =
                new Vector2(artworkBackgroundHeight + 300, artworkBackgroundHeight * 0.3f);

            var bottomPosition = artworkBackgroundBottomRectTransform.anchoredPosition;
            bottomPosition.y -= artworkBackgroundHeight * 0.7f - (497f - 106.8f);
            artworkBackgroundBottomRectTransform.anchoredPosition = bottomPosition;
        }
    }
}