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

        public void Awake() => instance ??= this;

        public void PlayGood() => criAtomSourceGood.Play();
        public void PlayGreat() => criAtomSourceGreat.Play();
        public void PlayPerfect() => criAtomSourcePerfect.Play();
    }
}