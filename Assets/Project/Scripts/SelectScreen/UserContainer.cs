using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserContainer : MonoBehaviour
{
    private string Name = "Developer";
    private int box_width_diff = 30; //1文字毎の変化基準幅
    private int standard_length = 7; //初期の幅は7文字仕様のため，7文字より何文字大きいか小さいかで幅を伸縮させる
    public Text Displayed; //表示される文字列

    void Start()
    {
        int len = 0; //ユーザ名の文字数
        float box_width = gameObject.GetComponent<RectTransform>().sizeDelta.x;
        float box_height = gameObject.GetComponent<RectTransform>().sizeDelta.y;
        float box_x = gameObject.GetComponent<RectTransform>().localPosition.x;
        float box_y = gameObject.GetComponent<RectTransform>().localPosition.y;
        float box_z = gameObject.GetComponent<RectTransform>().localPosition.z;
        //Debug.Log("default_box_width: "+ box_width);
        //Debug.Log("default_box_height: "+ box_height);

        if (Name != null) len = Name.Length; //ユーザ名があれば文字数を格納
        else len = 0; //ユーザ名が0文字の場合
        //Debug.Log("String Length: "+ len);

        RectTransform rt = gameObject.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(box_width + box_width_diff * (-standard_length + len), box_height); //箱のサイズを変更
        rt.localPosition = new Vector3(box_x - box_width_diff * (-standard_length + len) / 2.0f, box_y, box_z); //箱の位置を変更
        //Debug.Log("after_box_width: "+ box_width);

        Displayed.text = String.Concat("Welcome " + Name); //表示される文字列は"Welcome "も付加される
    }
}