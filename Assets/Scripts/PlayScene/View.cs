using System;
using TMPro;
using UnityEngine;

namespace PlayScene
{
    public class View : MonoBehaviour
    {
        public static View instance;

        [SerializeField] private AudioSource bgmAudioSource;

        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI scoreText;
        
        public void Awake() => instance ??= this;

        public AudioSource BgmAudioSource => bgmAudioSource;

        public AudioClip BgmAudioClip
        {
            set => bgmAudioSource.clip = value;
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