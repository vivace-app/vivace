using System;
using System.Collections;
using UnityEngine;

public class NotesScript : MonoBehaviour
{

    private int isInLineLevel = 0;
    private ProcessManager _gameManager;
    private KeyCode _lineKey;
    public int lineNum;
    // -- Temporary Variable. -------------------------------------------------------------
    private float speed = 4.0f;
    // ------------------------------------------------------------------------------------

    void Start()
    {
        // this.transform.localScale -= new Vector3 (0.285f, 0, 0.05f);
        this.transform.localScale -= new Vector3(0.285f, 0, 0.055f);
        _gameManager = GameObject.Find ("GameController").GetComponent<PlayScreenProcessManager> (); //インスタンスに GameController.cs 情報を格納
        _lineKey = GameUtil.GetKeyCodeByLineNum (lineNum); //ノーツに割り当てられているキーを取得
    }

    void Update()
    {
        if (_gameManager._isPlaying == true)
        {
            this.transform.position += (Vector3.down + Vector3.back * (float)Math.Sqrt(3)) * Time.deltaTime * speed;
            if (this.transform.position.z < -9.3) Destroy(this.gameObject);
            if (_gameManager._autoPlay == true && isInLineLevel == 4) //自動プレイ
            {
                _gameManager.PerfectTimingFunc(lineNum);
                Destroy(this.gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "BadJudge")
        {
            isInLineLevel++;
            //Debug.Log("Bad OK.");
        }
        if (other.gameObject.tag == "GreatJudge")
        {
            isInLineLevel++;
            //Debug.Log("Great OK.");
        }
        if (other.gameObject.tag == "PerfectJudge")
        {
            isInLineLevel++;
            //Debug.Log("Perfect OK.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "BadJudge")
        {
            isInLineLevel--;
            //Debug.Log("Bad No.");
        }
        if (other.gameObject.tag == "GreatJudge")
        {
            isInLineLevel--;
            //Debug.Log("Great No.");
        }
        if (other.gameObject.tag == "PerfectJudge")
        {
            isInLineLevel--;
            //Debug.Log("Perfect No.");
        }
    }

    void CheckInput(KeyCode key)
    {
        if (Input.GetKeyDown(key) /*|| TouchCheck.CheckTouch (lineNum, _touchInput)*/)
        { //キーの入力が確認できたら
            switch (isInLineLevel)
            {
                case 1:
                    _gameManager.SoundEffect(2);
                    Destroy(this.gameObject);
                    break;
                case 2:
                    _gameManager.GreatTimingFunc(lineNum);
                    Destroy(this.gameObject);
                    break;
                case 3:
                    _gameManager.PerfectTimingFunc(lineNum);
                    Destroy(this.gameObject);
                    break;
            }
            //_laneEffect[lineNum].PlayLaneEffect ();
        }
    }
}