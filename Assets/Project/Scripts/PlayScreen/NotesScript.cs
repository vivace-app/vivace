using System;
using UnityEngine;

public class NotesScript : MonoBehaviour
{

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
        this.transform.position += (Vector3.down + Vector3.back * (float)Math.Sqrt(3)) * Time.deltaTime * speed;
        if (this.transform.position.z < -9.3) Destroy(this.gameObject);
    }

    void OnTriggerEnter()
    {
        Debug.Log("OnTriggerEnter!!");
    }
}