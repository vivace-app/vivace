using CriWare;
using UnityEngine;

namespace PlayScene
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager instance;
    
        [SerializeField] private CriAtomSource criAtomSourceBad;
        [SerializeField] private CriAtomSource criAtomSourceGreat;
        [SerializeField] private CriAtomSource criAtomSourcePerfect;

        public void Awake() => instance ??= this;

        public void PlayBad() => criAtomSourceBad.Play();
        public void PlayGreat() => criAtomSourceGreat.Play();
        public void PlaySuccess() => criAtomSourcePerfect.Play();
    }
}