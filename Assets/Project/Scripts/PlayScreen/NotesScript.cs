using System;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

public class NotesScript : MonoBehaviour
{
    private PlayScreenProcessManager _gameManager;
    public int lineNum;
    private int isInLineLevel = 0;
    private int currentTouch = 0;
    private KeyCode _lineKey;

    void Start()
    {
        this.transform.localScale -= new Vector3(0.285f, 0, 0.055f);
        _gameManager = GameObject.Find("ProcessManager").GetComponent<PlayScreenProcessManager>(); //インスタンスに GameController.cs 情報を格納
        _lineKey = GameUtil.GetKeyCodeByLineNum(lineNum);
    }

    void Update()
    {
        if (PlayScreenProcessManager._isPlaying)
        {
            this.transform.position += (Vector3.down + Vector3.back * (float)Math.Sqrt(3)) * Time.deltaTime * 0.6f * PlayScreenProcessManager._notesSpeedIndex;
            if (this.transform.position.z < -12)
            {
                _gameManager.MissTimingFunc(lineNum);
                Destroy(this.gameObject);
            }
            if (isInLineLevel >= 1 && !PlayScreenProcessManager._autoPlay) CheckInput(_lineKey);
            else currentTouch = TouchEvent.OnTouch[lineNum]; //長押し対策
        }
    }

    void OnTriggerEnter(Collider other) //オブジェクトと衝突するたび，インクリメント
    {
        if (other.gameObject.tag == "GoodJudge") isInLineLevel++;
        if (other.gameObject.tag == "GreatJudge") isInLineLevel++;
        if (other.gameObject.tag == "PerfectJudge")
        {
            isInLineLevel++;
            if (PlayScreenProcessManager._autoPlay) AutoPlayFunc();
        }
    }

    void OnTriggerExit(Collider other) //オブジェクトから脱出するたびにインクリメント
    {
        if (other.gameObject.tag == "GoodJudge") isInLineLevel++;
        if (other.gameObject.tag == "GreatJudge") isInLineLevel++;
        if (other.gameObject.tag == "PerfectJudge") isInLineLevel++;
    }

    async void AutoPlayFunc()
    {
        await Task.Delay(20);
        _gameManager.PerfectTimingFunc(lineNum);
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
                    _gameManager.GoodTimingFunc(lineNum);
                    Destroy(this.gameObject);
                    break;
                case 2:
                    Destroy(this.gameObject);
                    _gameManager.GreatTimingFunc(lineNum);
                    break;
                case 3:
                    _gameManager.PerfectTimingFunc(lineNum);
                    Destroy(this.gameObject);
                    break;
                case 4:
                    _gameManager.GreatTimingFunc(lineNum);
                    Destroy(this.gameObject);
                    break;
                case 5:
                    _gameManager.GoodTimingFunc(lineNum);
                    Destroy(this.gameObject);
                    break;
            }
        }
    }
}