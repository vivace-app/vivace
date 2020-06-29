// using System.Threading.Tasks;
// using System.Collections;
using System;
using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;

public class NotesScript : MonoBehaviour {
    private float speed = 2.0f;
    // public float moveTime;
    // public GameObject destination;
    // private new Transform transform;
    // private float time = 0f;
    // private Vector3 v_start;
    // private Vector3 v_destinaton;

    // void Start () {
    //     transform = GetComponent<Transform> ();
    //     v_destinaton = destination.transform.position;
    //     v_start = this.transform.position;
    // }

    // void Update () {
    //     var v = time / moveTime;
    //     transform.position = Vector3.Lerp(v_start, v_destinaton, v);
    //     time += Time.deltaTime;
    // }

    // private void OnCollisionEnter(Collision collisionInfo)
    // {
    //     if (destination = collisionInfo.gameObject) {
    //         Debug.Log(time);
    //     }
    // }

    void Update () {
        this.transform.position += (Vector3.down + Vector3.back * (float) Math.Sqrt (3)) * Time.deltaTime * speed;
        // Debug.Log ((Vector3.down + Vector3.back * (float) Math.Sqrt (3)));
        if (this.transform.position.z < -9.3) {
            Destroy (this.gameObject);
            // _gameManager._combo = 0;
        }
    }
}