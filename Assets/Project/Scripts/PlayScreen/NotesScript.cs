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
    private Rigidbody rigidBody; // Rigidbody for Physics
    private bool stopNotesFlag = false; // Check the timing to move notes

    // ====================================================================================

    void Start()
    {
        _playScreenProcessManager = GameObject.Find("ProcessManager").GetComponent<PlayScreenProcessManager>(); // Instance <- PlayScreenProcessManager.cs
        rigidBody = GetComponent<Rigidbody>();
        _lineKey = GameUtil.GetKeyCodeByLineNum(lineNum); // Get Key Code.
        this.transform.localScale -= new Vector3(0.285f, 0, 0.055f); // Move to initial position.
        deltaPosition = PlayScreenProcessManager._deltaPosition; // Set fall vector per frame.
        rigidBody.AddForce(0f, -0.625f * _playScreenProcessManager.notesSpeedIndex, -0.625f * _playScreenProcessManager.notesSpeedIndex * (float)Math.Sqrt(3), ForceMode.VelocityChange);
    }

    void Update()
    {
        if (PlayScreenProcessManager._isPlaying)
        {
            //this.transform.position += deltaPosition * Time.deltaTime;
            if (stopNotesFlag)
            {
                rigidBody.AddForce(0f, -0.625f * _playScreenProcessManager.notesSpeedIndex, -0.625f * _playScreenProcessManager.notesSpeedIndex * (float)Math.Sqrt(3), ForceMode.VelocityChange);
                stopNotesFlag = false;
            }
            if (this.transform.position.z < -10.4)
            {
                _playScreenProcessManager.MissTimingFunc();
                Destroy(this.gameObject);
            }
            if (isInLineLevel >= 1 && isInLineLevel <= 5 && !PlayScreenProcessManager._autoPlay) CheckInput(_lineKey);
            else currentTouch = 0; // Prevent Long press.
        }
        if (!PlayScreenProcessManager._isPlaying && !stopNotesFlag)
        {
            rigidBody.velocity = Vector3.zero;
            stopNotesFlag = true;
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
        _playScreenProcessManager.PerfectTimingFunc();
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
                    _playScreenProcessManager.GoodTimingFunc();
                    Destroy(this.gameObject);
                    break;
                case 2:
                    _playScreenProcessManager.GreatTimingFunc();
                    Destroy(this.gameObject);
                    break;
                case 3:
                    _playScreenProcessManager.PerfectTimingFunc();
                    Destroy(this.gameObject);
                    break;
                case 4:
                    _playScreenProcessManager.GreatTimingFunc();
                    Destroy(this.gameObject);
                    break;
                case 5:
                    _playScreenProcessManager.GoodTimingFunc();
                    Destroy(this.gameObject);
                    break;
            }
        }
    }
}