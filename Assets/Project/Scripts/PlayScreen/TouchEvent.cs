using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchEvent : MonoBehaviour
{
    public static int[] OnTouch = { 0, 0, 0, 0, 0 };

    public void Touch(int num) //クリック・タッチ中
    {
        OnTouch[num] = 1;
    }

    public void Up(int num) //クリックまたはタッチを離したとき
    {
        OnTouch[num] = 0;
    }
}