using System;
using UnityEngine;

public class NotesScript : MonoBehaviour
{

    private int isInLineLevel = 0;
    // -- Temporary Variable. -------------------------------------------------------------
    private float speed = 4.0f;
    // ------------------------------------------------------------------------------------

    void Start()
    {
        // this.transform.localScale -= new Vector3 (0.285f, 0, 0.05f);
        this.transform.localScale -= new Vector3(0.285f, 0, 0.055f);
    }

    void Update()
    {
        if (PlayScreenProcessManager._isPlaying == true)
        {
            this.transform.position += (Vector3.down + Vector3.back * (float)Math.Sqrt(3)) * Time.deltaTime * speed;
            if (this.transform.position.z < -9.3) Destroy(this.gameObject);
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
}