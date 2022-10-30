using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace PlayScene
{
    public class EventSystem : MonoBehaviour
    {
        [SerializeField] private int lineNum;

        public bool isTouched { get; private set; } = false;

        // public void Drop(BaseEventData baseEventData)
        // {
        //     if (baseEventData is not PointerEventData pointerEventData) return;
        //
        //     //Debug.Log("Drop: " + lineNum + "," + data.position.y + "," + data.pointerId);
        //     int result = CoordYPresever.isFlick(pointerEventData.position.y, lineNum);
        //     if (result == 1) // 長押しからフリック
        //     {
        //         Debug.Log("Flick Up!");
        //         ProcessManager.JudgeTiming(lineNum, 5);
        //     }
        //     else // 長押し離す
        //     {
        //         ProcessManager.JudgeTiming(lineNum, 2);
        //     }
        //     isTouched = false;
        // }
        public void PointerDown(BaseEventData _) => ProcessManager.JudgeTiming(lineNum, 1);
        
        public void PointerExit(BaseEventData _) => ProcessManager.JudgeTiming(lineNum, 2);

        public void OnSwipe(Vector2 vector2) => ProcessManager.JudgeTiming(lineNum, 3);
        
        public void OnLongClickStart() => ProcessManager.JudgeTiming(lineNum, 2);
        public void OnLongClickCancel() => ProcessManager.JudgeTiming(lineNum, 2);

        // public void PointerUp(BaseEventData baseEventData)
        // {
        //     if (baseEventData is not PointerEventData pointerEventData) return;
        //
        //     int result = CoordYPresever.isFlick(pointerEventData.position.y, lineNum);
        //     if (result == 1) // フリック
        //     {
        //         Debug.Log("Flick Up!");
        //         ProcessManager.JudgeTiming(lineNum, 5);
        //     }
        //     else // 長押し離す
        //     {
        //         ProcessManager.JudgeTiming(lineNum, 2);
        //     }
        //     isTouched = false;
        // }
        //
        // public void PointerEnter(BaseEventData baseEventData)
        // {
        //     if (baseEventData is not PointerEventData pointerEventData) return;
        //
        //     //Debug.Log("PointerEnter: " + lineNum + "," + data.position.y + "," + data.pointerId);
        //     CoordYPresever.AddCoordY(pointerEventData.position.y, lineNum);
        //     isTouched = true;
        // }
        //
        // public void PointerExit(BaseEventData baseEventData)
        // {
        //     isTouched = false;
        // }
    }
}