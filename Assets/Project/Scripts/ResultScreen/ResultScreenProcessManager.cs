using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public Text RsltPerfects, RsltGreats, RsltGoods, RsltMisss, RsltTotal;

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
        // --- TEMP ---------------------------------------------------------------------------
        //PlayScreenProcessManager.r_perfects = 100;
        //PlayScreenProcessManager.r_greats = 63;
        //PlayScreenProcessManager.r_goods = 37;
        //PlayScreenProcessManager.r_misss = 17;
        //PlayScreenProcessManager.r_score = 867553;
        // ------------------------------------------------------------------------------------

        StartCoroutine(GetTopTenNetworkProcess());
        CountsDelayer();
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
    private async void CountsAnime(int sep, double value, Text scoreboard)
    {
        double valueshow = 0;
        scoreboard.text = ((int)Math.Round(valueshow, 0, MidpointRounding.AwayFromZero)).ToString("D"); //表示を初期化

        for (int i = 0; i < sep; i++) //sep分割したものを33ミリ秒ごとにsep回加算()
        {
            valueshow += value / sep;
            scoreboard.text = ((int)Math.Round(valueshow, 0, MidpointRounding.AwayFromZero)).ToString("D"); //四捨五入して型変換を行い表示を更新
            await Task.Delay(33);
        }
    }
    private async void CountsDelayer()
    {
        CountsAnime(15, PlayScreenProcessManager.r_perfects, RsltPerfects);
        await Task.Delay(250);
        CountsAnime(15, PlayScreenProcessManager.r_greats, RsltGreats);
        await Task.Delay(250);
        CountsAnime(15, PlayScreenProcessManager.r_goods, RsltGoods);
        await Task.Delay(250);
        CountsAnime(15, PlayScreenProcessManager.r_misss, RsltMisss);
        await Task.Delay(250);
        CountsAnime(45, PlayScreenProcessManager.r_score, RsltTotal);
    }
}