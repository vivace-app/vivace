using System;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

public class NotesScript : MonoBehaviour
{
    // --- Attach from Unity --------------------------------------------------------------
    public int lineNum;
    // ------------------------------------------------------------------------------------

    // --- Instance -----------------------------------------------------------------------
    public PlayScreenProcessManager _playScreenProcessManager;
    // ------------------------------------------------------------------------------------

    private int isInLineLevel = 0;
    private int currentTouch = 0;
    private KeyCode _lineKey; // Key code of this notes.
    private Vector3 deltaPosition; // Vector falling every frame.

    // ====================================================================================

    void Start()
    {
        _playScreenProcessManager = GameObject.Find("ProcessManager").GetComponent<PlayScreenProcessManager>(); // Instance <- PlayScreenProcessManager.cs
        _lineKey = GameUtil.GetKeyCodeByLineNum(lineNum); // Get Key Code.
        this.transform.localScale -= new Vector3(0.285f, 0, 0.055f); // Move to initial position.
        deltaPosition = PlayScreenProcessManager._deltaPosition; // Set fall vector per frame.
    }

    void Update()
    {
        if (PlayScreenProcessManager._isPlaying)
        {
            this.transform.position += deltaPosition * Time.deltaTime;
            if (this.transform.position.z < -9.4)
            {
                _playScreenProcessManager.MissTimingFunc(lineNum);
                Destroy(this.gameObject);
            }
            if (isInLineLevel >= 1 && isInLineLevel <= 5 && !PlayScreenProcessManager._autoPlay) CheckInput(_lineKey);
            else currentTouch = 0; // Prevent Long press.
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "GoodJudge" || other.gameObject.tag == "GreatJudge" || other.gameObject.tag == "PerfectJudge") isInLineLevel++;
        if (other.gameObject.tag == "PerfectJudge" && PlayScreenProcessManager._autoPlay) AutoPlayFunc();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "GoodJudge" || other.gameObject.tag == "GreatJudge" || other.gameObject.tag == "PerfectJudge") isInLineLevel++;
    }

    async void AutoPlayFunc()
    {
        await Task.Delay(20);
        _playScreenProcessManager.PerfectTimingFunc(lineNum);
        Destroy(this.gameObject);
    }

    void CheckInput(KeyCode key)
    {
        if (Input.GetKeyDown(key) || currentTouch < TouchEvent.OnTouch[lineNum])
        {
            currentTouch = TouchEvent.OnTouch[lineNum];
            switch (isInLineLevel)
            { //1：Good，2：Great，3：Perfect，4：Great，5：Good
                case 1:
                    _playScreenProcessManager.GoodTimingFunc(lineNum);
                    Destroy(this.gameObject);
                    break;
                case 2:
                    _playScreenProcessManager.GreatTimingFunc(lineNum);
                    Destroy(this.gameObject);
                    break;
                case 3:
                    _playScreenProcessManager.PerfectTimingFunc(lineNum);
                    Destroy(this.gameObject);
                    break;
                case 4:
                    _playScreenProcessManager.GreatTimingFunc(lineNum);
                    Destroy(this.gameObject);
                    break;
                case 5:
                    _playScreenProcessManager.GoodTimingFunc(lineNum);
                    Destroy(this.gameObject);
                    break;
            }
        }
    }
}