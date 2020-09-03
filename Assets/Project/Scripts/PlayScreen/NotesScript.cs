using System;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

public class NotesScript : MonoBehaviour
{

    private int isInLineLevel = 0;
    private PlayScreenProcessManager _gameManager;
    private KeyCode _lineKey;
    public int lineNum;
    // -- Temporary Variable. -------------------------------------------------------------
    private float speed = 0.6f;
    // ------------------------------------------------------------------------------------

    void Start()
    {
        // this.transform.localScale -= new Vector3 (0.285f, 0, 0.05f);
        this.transform.localScale -= new Vector3(0.285f, 0, 0.055f);
        _gameManager = GameObject.Find("ProcessManager").GetComponent<PlayScreenProcessManager>(); //インスタンスに GameController.cs 情報を格納
        _lineKey = GameUtil.GetKeyCodeByLineNum(lineNum); //ノーツに割り当てられているキーを取得
    }

    void Update()
    {
        if (PlayScreenProcessManager._isPlaying == true)
        {
            this.transform.position += (Vector3.down + Vector3.back * (float)Math.Sqrt(3)) * Time.deltaTime * speed * PlayScreenProcessManager._notesSpeedIndex;
            if (this.transform.position.z < -9.3)
            {
                PlayScreenProcessManager.MissTimingFunc(lineNum); //Missのときの関数
                Destroy(this.gameObject);
            }
            if (isInLineLevel >= 1 && PlayScreenProcessManager._autoPlay == false) CheckInput(_lineKey); //キーを押されるかのチェック
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "GoodJudge")
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
            if (PlayScreenProcessManager._autoPlay == true) AutoPlayFunc(); //自動プレイ
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "GoodJudge")
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

    async void AutoPlayFunc()
    {
        await Task.Delay(20);
        Destroy(this.gameObject);
        _gameManager.PerfectTimingFunc(lineNum);
        //Debug.Log("Autoplayed!");
    }

    void CheckInput(KeyCode key)
    {
        if (Input.GetKeyDown(key) /*|| TouchCheck.CheckTouch (lineNum, _touchInput)*/)
        { //キーの入力が確認できたら
            Debug.Log("Key pushed!");
            switch (isInLineLevel) //1：Good，2：Great，3：Perfect
            {
                case 1:
                    Destroy(this.gameObject);
                    _gameManager.GoodTimingFunc(lineNum); //Goodのときの関数
                    break;
                case 2:
                    Destroy(this.gameObject);
                    _gameManager.GreatTimingFunc(lineNum); //Greatのときの関数
                    break;
                case 3:
                    Destroy(this.gameObject);
                    _gameManager.PerfectTimingFunc(lineNum); //Perfectのときの関数
                    break;
            }
            //_laneEffect[lineNum].PlayLaneEffect ();
        }
    }
}