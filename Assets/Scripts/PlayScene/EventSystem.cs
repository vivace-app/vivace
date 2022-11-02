using PlayScene;
using UnityEngine;
using UnityEngine.EventSystems;

public class LineJudger : MonoBehaviour
{
    [SerializeField] int lineNum = 0;
    public bool isTouched { get; private set; } = false;
    public void BeginDrag(PointerEventData data)
    {
        //Debug.Log("BeginDrag: " + lineNum + "," + data.position.y + "," + data.pointerId);
        //CoordYPresever.AddCoordY(data.position.y, lineNum);
    }

    public void Drop(BaseEventData baseEventData)
    {
        if (baseEventData is not PointerEventData pointerEventData) return;

        bool noteFound = false;
        //Debug.Log("Drop: " + lineNum + "," + data.position.y + "," + data.pointerId);
        int result = CoordYPresever.isFlick(pointerEventData.position.y, lineNum);
        if (result == 1) // 長押しからフリック
        {
            // Debug.Log("Flick Up!");
            noteFound = ProcessManager.JudgeTiming(lineNum, 3);
            if(!noteFound) ProcessManager.JudgeTiming(lineNum, 2);
        }
        else // 長押し離す
        {
            ProcessManager.JudgeTiming(lineNum, 2);
        }
        isTouched = false;
    }

    public void PointerDown(BaseEventData baseEventData)
    {
        if (baseEventData is not PointerEventData pointerEventData) return;

        bool noteFound = false;
        Debug.Log("PointerDown: " + lineNum);
        noteFound = ProcessManager.JudgeTiming(lineNum, 1);
        if(!noteFound) ProcessManager.JudgeTiming(lineNum, 2);
        CoordYPresever.AddCoordY(pointerEventData.position.y, lineNum);
        isTouched = true;
    }

    public void PointerUp(BaseEventData baseEventData)
    {
        if (baseEventData is not PointerEventData pointerEventData) return;

        bool noteFound = false;
        int result = CoordYPresever.isFlick(pointerEventData.position.y, lineNum);
        if (result == 1) // フリック
        {
            // Debug.Log("Flick Up!");
            noteFound = ProcessManager.JudgeTiming(lineNum, 3);
            if(!noteFound) ProcessManager.JudgeTiming(lineNum, 2);
        }
        else // 長押し離す
        {
            ProcessManager.JudgeTiming(lineNum, 2);
        }
        isTouched = false;
    }
    
    public void PointerEnter(BaseEventData baseEventData)
    {
        if (baseEventData is not PointerEventData pointerEventData) return;

        //Debug.Log("PointerEnter: " + lineNum + "," + data.position.y + "," + data.pointerId);
        CoordYPresever.AddCoordY(pointerEventData.position.y, lineNum);
        isTouched = true;
    }

    public void PointerExit(BaseEventData baseEventData)
    {
        isTouched = false;
    }
}
