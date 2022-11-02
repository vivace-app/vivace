using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace PlayScene
{
    public class View : MonoBehaviour
    {
        public static View instance;

        [SerializeField] private AudioSource bgmAudioSource;
        [SerializeField] private CustomButton pauseCustomButton;
        [SerializeField] private CustomButton giveUpCustomButton;
        [SerializeField] private CustomButton cancelCustomButton;
        [SerializeField] private GameObject pauseModal;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI scoreText;
        
        public void Awake() => instance ??= this;

        public AudioSource BgmAudioSource => bgmAudioSource;

        public AudioClip BgmAudioClip
        {
            set => bgmAudioSource.clip = value;
        }

        public Action setOnClickPauseCustomButtonAction
        {
            set => pauseCustomButton.onClickCallback = value;
        }

        public Action setOnClickGiveUpCustomButtonAction
        {
            set => giveUpCustomButton.onClickCallback = value;
        }

        public Action setOnClickCancelCustomButtonAction
        {
            set => cancelCustomButton.onClickCallback = value;
        }

        public bool setPauseModalVisible
        {
            set
            {
                pauseModal.SetActive(value);
                pauseModal.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutCubic);
            }
        }

        public int ComboText
        {
            set => comboText.text = value.ToString("D");
        }

        public float ScoreText
        {
            set => scoreText.text = ((int) Math.Round(value, 0, MidpointRounding.AwayFromZero)).ToString("D7");
        }
    }
}