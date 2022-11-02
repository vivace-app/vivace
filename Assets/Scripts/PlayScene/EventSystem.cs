using Model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayScene
{
    public class EventSystem : MonoBehaviour
    {
        [SerializeField] private int lineNum;

        public void Drop(BaseEventData baseEventData) => OnExit(baseEventData);
        public void PointerUp(BaseEventData baseEventData) => OnExit(baseEventData);

        public void PointerDown(BaseEventData baseEventData)
        {
            if (baseEventData is not PointerEventData pointerEventData) return;

            var isSucceed = ProcessManager.JudgeTiming(lineNum, QueuedNote.NoteType.Normal);
            /* 通常ノーツが見当たらなかった場合、ロングノーツを疑う */
            if (!isSucceed) ProcessManager.JudgeTiming(lineNum, QueuedNote.NoteType.Long);
            CoordYPreserver.AddCoordY(pointerEventData.position.y, lineNum);
        }

        public void PointerEnter(BaseEventData baseEventData)
        {
            if (baseEventData is not PointerEventData pointerEventData) return;

            CoordYPreserver.AddCoordY(pointerEventData.position.y, lineNum);
        }

        #region private void OnExit

        private void OnExit(BaseEventData baseEventData)
        {
            if (baseEventData is not PointerEventData pointerEventData) return;

            var isFlick = CoordYPreserver.IsFlick(pointerEventData.position.y, lineNum);
            if (isFlick) // 長押ししてフリック
            {
                var isSucceed = ProcessManager.JudgeTiming(lineNum, QueuedNote.NoteType.Flick);
                /* フリックノーツが見当たらなかった場合、ロングノーツを疑う */
                if (!isSucceed) ProcessManager.JudgeTiming(lineNum, QueuedNote.NoteType.Long);
            }
            else // 長押しして離す
            {
                ProcessManager.JudgeTiming(lineNum, QueuedNote.NoteType.Long);
            }
        }

        #endregion
    }
}