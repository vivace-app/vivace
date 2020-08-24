using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeMenu : MonoBehaviour
{
    public GameObject scrollbar;
    float scroll_pos = 0;
    float distance;
    float[] pos;

    void Start()
    {
        pos = new float[transform.childCount];
        distance = 1f / (pos.Length - 1f);
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
        }
        else
        {
            for (int i = 0; i < pos.Length; i++)
            {
                if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
                {
                    scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                }
            }
        }
        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
            {
                transform.GetChild(i).localScale = Vector2.Lerp(transform.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);
                transform.GetChild(i).Find("NoteLogo").localScale = Vector2.Lerp(transform.GetChild(i).Find("NoteLogo").localScale, new Vector2(1f, 1f), 0.1f);
                transform.GetChild(i).Find("NoteLogo").gameObject.transform.position = Vector2.Lerp(transform.GetChild(i).Find("NoteLogo").transform.position, new Vector2(transform.GetChild(i).Find("NoteLogo").transform.position.x, 35), 0.08f);
                transform.GetChild(i).Find("Title").localScale = Vector2.Lerp(transform.GetChild(i).Find("Title").localScale, new Vector2(1f, 1f), 0.08f);
                transform.GetChild(i).Find("Title").gameObject.transform.position = Vector2.Lerp(transform.GetChild(i).Find("Title").transform.position, new Vector2(transform.GetChild(i).Find("Title").transform.position.x, 35), 0.08f);
                transform.GetChild(i).Find("Artist").localScale = Vector2.Lerp(transform.GetChild(i).Find("Artist").localScale, new Vector2(1f, 1f), 0.08f);
                transform.GetChild(i).Find("Artist").gameObject.transform.position = Vector2.Lerp(transform.GetChild(i).Find("Artist").transform.position, new Vector2(transform.GetChild(i).Find("Artist").transform.position.x, 30), 0.08f);
                transform.GetChild(i).Find("Artwork").localScale = Vector2.Lerp(transform.GetChild(i).Find("Artwork").localScale, new Vector2(1f, 1f), 0.08f);
                transform.GetChild(i).Find("Artwork").gameObject.transform.position = Vector2.Lerp(transform.GetChild(i).Find("Artwork").transform.position, new Vector2(transform.GetChild(i).Find("Artwork").transform.position.x, 10), 0.1f);
                transform.GetChild(i).Find("Easy").gameObject.SetActive(true);
                transform.GetChild(i).Find("Basic").gameObject.SetActive(true);
                transform.GetChild(i).Find("Hard").gameObject.SetActive(true);
                transform.GetChild(i).Find("Demon").gameObject.SetActive(true);
                transform.GetChild(i).Find("PlayMusic").gameObject.SetActive(true);
                for (int cnt = 0; cnt < pos.Length; cnt++)
                {
                    if (i != cnt)
                    {
                        transform.GetChild(cnt).localScale = Vector2.Lerp(transform.GetChild(cnt).localScale, new Vector2(0.8f, 0.7f), 0.1f);
                        transform.GetChild(cnt).Find("NoteLogo").localScale = Vector2.Lerp(transform.GetChild(cnt).Find("NoteLogo").localScale, new Vector2(1.1f, 1.257f), 0.1f);
                        transform.GetChild(cnt).Find("NoteLogo").gameObject.transform.position = Vector2.Lerp(transform.GetChild(cnt).Find("NoteLogo").transform.position, new Vector2(transform.GetChild(cnt).Find("NoteLogo").transform.position.x, 23), 0.08f);
                        transform.GetChild(cnt).Find("Title").localScale = Vector2.Lerp(transform.GetChild(cnt).Find("Title").localScale, new Vector2(1.1f, 1.257f), 0.08f);
                        transform.GetChild(cnt).Find("Title").gameObject.transform.position = Vector2.Lerp(transform.GetChild(cnt).Find("Title").transform.position, new Vector2(transform.GetChild(cnt).Find("Title").transform.position.x, 23), 0.08f);
                        transform.GetChild(cnt).Find("Artist").localScale = Vector2.Lerp(transform.GetChild(cnt).Find("Artist").localScale, new Vector2(1f, 1.14f), 0.08f);
                        transform.GetChild(cnt).Find("Artist").gameObject.transform.position = Vector2.Lerp(transform.GetChild(cnt).Find("Artist").transform.position, new Vector2(transform.GetChild(cnt).Find("Artist").transform.position.x, 19), 0.08f);
                        transform.GetChild(cnt).Find("Artwork").localScale = Vector2.Lerp(transform.GetChild(cnt).Find("Artwork").localScale, new Vector2(1.5f, 1.71f), 0.08f);
                        transform.GetChild(cnt).Find("Artwork").gameObject.transform.position = Vector2.Lerp(transform.GetChild(cnt).Find("Artwork").transform.position, new Vector2(transform.GetChild(cnt).Find("Artwork").transform.position.x, -3), 0.1f);
                        transform.GetChild(cnt).Find("Easy").gameObject.SetActive(false);
                        transform.GetChild(cnt).Find("Basic").gameObject.SetActive(false);
                        transform.GetChild(cnt).Find("Hard").gameObject.SetActive(false);
                        transform.GetChild(cnt).Find("Demon").gameObject.SetActive(false);
                        transform.GetChild(cnt).Find("PlayMusic").gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
