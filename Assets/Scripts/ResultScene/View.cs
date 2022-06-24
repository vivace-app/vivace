using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ResultScene
{
    public class View : MonoBehaviour
    {
        public static View Instance;

        [SerializeField] private CustomButton retryButton;
        [SerializeField] private CustomButton exitButton;

        [SerializeField] private TextMeshProUGUI[] nicknameText;
        [SerializeField] private TextMeshProUGUI[] scoreText;
        [SerializeField] private TextMeshProUGUI perfectScoreText;
        [SerializeField] private TextMeshProUGUI greatScoreText;
        [SerializeField] private TextMeshProUGUI goodScoreText;
        [SerializeField] private TextMeshProUGUI missScoreText;
        [SerializeField] private TextMeshProUGUI totalScoreText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public Action RetryButtonAction
        {
            set => retryButton.onClickCallback = value;
        }

        public Action ExitButtonAction
        {
            set => exitButton.onClickCallback = value;
        }

        public string[] NameText
        {
            set
            {
                foreach (var (item, index) in nicknameText.Select((item, index) => (item, index)))
                    item.text = value[index];
            }
        }

        public string[] ScoreText
        {
            set
            {
                foreach (var (item, index) in scoreText.Select((item, index) => (item, index)))
                    item.text = value[index];
            }
        }

        public string PerfectScoreText
        {
            set => perfectScoreText.text = value;
        }

        public string GreatScoreText
        {
            set => greatScoreText.text = value;
        }

        public string GoodScoreText
        {
            set => goodScoreText.text = value;
        }

        public string MissScoreText
        {
            set => missScoreText.text = value;
        }

        public string TotalScoreText
        {
            set => totalScoreText.text = value;
        }
    }
}