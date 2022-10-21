using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayScene
{
    public class EventSystem : MonoBehaviour
    {
        [SerializeField] private int lineNum;

        public void PointerDown(BaseEventData _) => PlaySceneProcessManager.JudgeTiming(lineNum, 1);
    }
}