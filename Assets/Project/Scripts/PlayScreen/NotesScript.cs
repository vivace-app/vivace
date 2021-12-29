using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Project.Scripts.PlayScreen
{
    public class NotesScript : MonoBehaviour
    {
        // --- Attach from Unity --------------------------------------------------------------
        public int lineNum;
        // ------------------------------------------------------------------------------------

        // --- Instance -----------------------------------------------------------------------
        public PlayScreenProcessManager playScreenProcessManager;
        // ------------------------------------------------------------------------------------

        private int _isInLineLevel; // Which zone the notes exist. 機能全面変更予定
        private int _currentTouch;
        private bool _isAutoPlay;
        private KeyCode _lineKey; // Key code of this notes.
        private Rigidbody _rigidBody; // Rigidbody for Physics
        private bool _stopNotesFlag; // Check the timing to move notes

        // ====================================================================================

        private void Start()
        {
            _isAutoPlay = PlayScreenProcessManager.IsAutoPlay;
            playScreenProcessManager = GameObject.Find("ProcessManager").GetComponent<PlayScreenProcessManager>(); // Instance <- PlayScreenProcessManager.cs
            _rigidBody = GetComponent<Rigidbody>();
            _lineKey = GameUtil.GetKeyCodeByLineNum(lineNum); // Get Key Code.
            transform.localScale -= new Vector3(0.285f, 0, 0.055f); // Move to initial position.
            _rigidBody.AddForce(0f, -0.625f * playScreenProcessManager.notesSpeedIndex, -0.625f * playScreenProcessManager.notesSpeedIndex * (float)Math.Sqrt(3), ForceMode.VelocityChange);
        }

        private void Update()
        {
            switch (PlayScreenProcessManager.IsPlaying)
            {
                // Playing
                case true:
                {
                    //this.transform.position += deltaPosition * Time.deltaTime;
                    if (_stopNotesFlag)
                    {
                        _rigidBody.AddForce(0f, -0.625f * playScreenProcessManager.notesSpeedIndex, -0.625f * playScreenProcessManager.notesSpeedIndex * (float)Math.Sqrt(3), ForceMode.VelocityChange);
                        _stopNotesFlag = false;
                    }
                    if (transform.position.z < -10.4)
                    {
                        playScreenProcessManager.MissTimingFunc();
                        Destroy(gameObject);
                    }
                    if (_isInLineLevel >= 1 && _isInLineLevel <= 5 && !_isAutoPlay) CheckInput(_lineKey);
                    else _currentTouch = 0; // Prevent Long press.
                    break;
                }
                // Paused
                case false when !_stopNotesFlag:
                    _rigidBody.velocity = Vector3.zero;
                    _stopNotesFlag = true;
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "GoodJudge" || other.gameObject.tag == "GreatJudge" || other.gameObject.tag == "PerfectJudge") _isInLineLevel++;
            if (other.gameObject.tag == "PerfectJudge" && PlayScreenProcessManager.IsAutoPlay) AutoPlayFunc();
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "GoodJudge" || other.gameObject.tag == "GreatJudge" || other.gameObject.tag == "PerfectJudge") _isInLineLevel++;
        }

        public async void AutoPlayFunc()
        {
            await Task.Delay(20);
            playScreenProcessManager.PerfectTimingFunc();
            Destroy(gameObject);
        }

        private void CheckInput(KeyCode key)
        {
            var getCurrentTouch = TouchEvent.OnTouch[lineNum];
            if (_currentTouch >= getCurrentTouch && !Input.GetKeyDown(key)) return;
            _currentTouch = getCurrentTouch;
            switch (_isInLineLevel)
            { // 1：Good，2：Great，3：Perfect，4：Great，5：Good
                case 1:
                case 5:
                    playScreenProcessManager.GoodTimingFunc();
                    Destroy(gameObject);
                    break;
                case 2:
                case 4:
                    playScreenProcessManager.GreatTimingFunc();
                    Destroy(gameObject);
                    break;
                case 3:
                    playScreenProcessManager.PerfectTimingFunc();
                    Destroy(gameObject);
                    break;
            }
        }
    }
}