using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserContainer : MonoBehaviour
{
    public string Name = "Default";
    public Text Displayed;

    void Start()
    {
        int len = 0;
        int x = 1000;

        Vector3 pos = this.gameObject.transform.position;
        if(Name != null) len = Name.Length;
        else Debug.Log("UserName Get Error.");

        Displayed.text = Name;
        Debug.Log("String Length: "+ len);
        //this.gameObject.transform.translate (0 + x, pos.y, pos.z);
    }
}