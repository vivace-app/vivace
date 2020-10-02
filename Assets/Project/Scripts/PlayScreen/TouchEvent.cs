using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchEvent : MonoBehaviour
{
    public static bool[] OnTouch = { false, false, false, false, false };
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Touch(int num)
    {
        switch (num)
        {
            case 0:
                Debug.Log("lane D touched");
                break;
            case 1:
                Debug.Log("lane F touched");
                break;
            case 2:
                Debug.Log("lane G touched");
                break;
            case 3:
                Debug.Log("lane H touched");
                break;
            case 4:
                Debug.Log("lane J touched");
                break;
        }
        OnTouch[num] = true;
    }

    public void Up(int num)
    {
        switch (num)
        {
            case 0:
                Debug.Log("lane D up");
                break;
            case 1:
                Debug.Log("lane F up");
                break;
            case 2:
                Debug.Log("lane G up");
                break;
            case 3:
                Debug.Log("lane H up");
                break;
            case 4:
                Debug.Log("lane J up");
                break;
        }
        OnTouch[num] = false;
    }
}