using CriWare;
using UnityEngine;

namespace PlayScene
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager instance;
    
        [SerializeField] private CriAtomSource criAtomSourceGood;
        [SerializeField] private CriAtomSource criAtomSourceGreat;
        [SerializeField] private CriAtomSource criAtomSourcePerfect;
        [SerializeField] private CriAtomSource criAtomSourceFlick;
        
        private void Awake()
        {
            if (instance == null) instance = this;
        }

        public void PlayGood() => criAtomSourceGood.Play();
        public void PlayGreat() => criAtomSourceGreat.Play();
        public void PlayPerfect() => criAtomSourcePerfect.Play();
        public void PlayFlick() => criAtomSourceFlick.Play();
    }
}