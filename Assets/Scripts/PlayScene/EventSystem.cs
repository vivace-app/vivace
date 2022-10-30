using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayScene
{
    public class EventSystem : MonoBehaviour
    {
        [SerializeField] private int lineNum;

        public void PointerDown(BaseEventData _) => ProcessManager.JudgeTiming(lineNum, 1);
    }
}