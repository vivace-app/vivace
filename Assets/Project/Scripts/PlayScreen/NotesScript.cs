// using System.Threading.Tasks;
using System;
// using System.Collections;
using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;

public class NotesScript : MonoBehaviour {
    private float speed = 4.0f;

    void Start () {
        this.transform.localScale -= new Vector3 (0.285f, 0, 0.05f);
    }

    void Update () {
        this.transform.position += (Vector3.down + Vector3.back * (float) Math.Sqrt (3)) * Time.deltaTime * speed;
        if (this.transform.position.z < -9.3) {
            Destroy (this.gameObject);
        }
    }
}