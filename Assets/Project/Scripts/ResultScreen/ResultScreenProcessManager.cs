using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ResultScreenProcessManager : MonoBehaviour
{
    // --- TEMP ---------------------------------------------------------------------------
    string musicTitle = "shining_star";
    // ------------------------------------------------------------------------------------

    public Text[] scoreListName = new Text[9];
    public Text[] scoreListScore = new Text[9];

    // ------------------------------------------------------------------------------------

    static readonly string topTenScoreApiUri = EnvDataStore.topTenScoreApiUri;

    // ------------------------------------------------------------------------------------

    [Serializable]
    public class topTenScoreResponse
    {
        public bool success;
        public List<scoreList> data;
    }

    [Serializable]
    public class scoreList
    {
        public string name;
        public int score;
    }

    private void Start()
    {
        StartCoroutine(GetTopTenNetworkProcess());
    }

    private void PlayScreenTransition()
    {
        SceneManager.LoadScene("PlayScene");
    }

    private void SelectScreenTransition()
    {
        SceneManager.LoadScene("SelectScene");
    }

    public void RetryButtonTappedController()
    {
        PlayScreenTransition();
    }

    public void ExitButtonTappedController()
    {
        SelectScreenTransition();
    }

    IEnumerator GetTopTenNetworkProcess()
    {
        WWWForm form = new WWWForm();
        form.AddField("music", musicTitle);
        UnityWebRequest www = UnityWebRequest.Post(topTenScoreApiUri, form);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError("ネットワークに接続できません．(" + www.error + ")");
        }
        else
        {
            GetTopTenScore(www.downloadHandler.text);
        }
    }

    private void GetTopTenScore(string data)
    {
        topTenScoreResponse jsnData = JsonUtility.FromJson<topTenScoreResponse>(data);

        if (jsnData.success)
        {
            int cnt = 0;
            foreach (scoreList x in jsnData.data)
            {
                scoreListName[cnt].text = x.name;
                scoreListScore[cnt++].text = x.score.ToString();
            }
            for (int i = cnt; i < 9; i++)
            {
                scoreListName[i].text = "-";
                scoreListScore[i].text = "-";
            }
        }
        else if (!jsnData.success)
        {
            Debug.LogError("データの取得に失敗しました．");
        }
    }
}
