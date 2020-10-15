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
    public RectTransform Background;
    public Text[] ScoreListName = new Text[9];
    public Text[] ScoreListScore = new Text[9];
    public Text Res_Perfects, Res_Greats, Res_Goods, Res_Misss, Res_Total;

    // ------------------------------------------------------------------------------------

    static readonly string topTenScoreApiUri = EnvDataStore.topTenScoreApiUri;
    static readonly string[] musicTitles = MusicTitleDataStore.musicTitles;

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
        ScreenResponsive();
        StartCoroutine(GetTopTenNetworkProcess());
        CountsDelayer();
    }

    private void ScreenResponsive()
    {
        float scale = 1f;
        if (Screen.width < Screen.height)
            scale = (Screen.height * 16) / (Screen.width * 9);
        Background.sizeDelta = new Vector2(Screen.width * scale, Screen.height * scale);
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
        form.AddField("music", musicTitles[SwipeMenu.selectedNumTmp]);
        form.AddField("level", SelectScreenProcessManager.selectedLevel);
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
                ScoreListName[cnt].text = x.name;
                ScoreListScore[cnt++].text = x.score.ToString();
            }
            for (int i = cnt; i < 9; i++)
            {
                ScoreListName[i].text = "-";
                ScoreListScore[i].text = "-";
            }
        }
        else
        {
            for (int i = 0; i < 9; i++)
            {
                ScoreListName[i].text = "-";
                ScoreListScore[i].text = "-";
            }
            Debug.LogError("データの取得に失敗しました．");
        }
    }

    private async void CountsAnimation(int sep, double value, Text scoreboard)
    {
        double valueShow = 0;

        for (int i = 0; i <= sep; i++) //sep分割したものを33ミリ秒ごとにsep回加算()
        {
            scoreboard.text = ((int)Math.Round(valueShow, 0, MidpointRounding.AwayFromZero)).ToString("D");
            valueShow += value / sep;
            await Task.Delay(33);
        }
    }

    private async void CountsDelayer()
    {
        CountsAnimation(15, PlayScreenProcessManager.res_perfects, Res_Perfects);
        await Task.Delay(250);
        CountsAnimation(15, PlayScreenProcessManager.res_greats, Res_Greats);
        await Task.Delay(250);
        CountsAnimation(15, PlayScreenProcessManager.res_goods, Res_Goods);
        await Task.Delay(250);
        CountsAnimation(15, PlayScreenProcessManager.res_misss, Res_Misss);
        await Task.Delay(250);
        CountsAnimation(45, PlayScreenProcessManager.res_score, Res_Total);
    }
}