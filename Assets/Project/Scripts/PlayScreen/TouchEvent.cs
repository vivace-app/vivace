using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchEvent : MonoBehaviour
{
    public static int[] OnTouch = { 0, 0, 0, 0, 0 };

    public void Touch(int num) //クリック・タッチ中
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
        OnTouch[num] = 1;
    }

    public void Up(int num) //クリックまたはタッチを離したとき
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
        OnTouch[num] = 0;
    }
}