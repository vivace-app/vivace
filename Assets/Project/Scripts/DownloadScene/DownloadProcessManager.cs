using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DownloadProcessManager : MonoBehaviour
{
    // --- Attach from Unity --------------------------------------------------------------
    public RectTransform Background;
    public Text Percent;
    public Text showVersion;
    // ------------------------------------------------------------------------------------

    // --- Environment variables ----------------------------------------------------------
    static readonly string thisVersion = EnvDataStore.thisVersion;
    static readonly string downloadCheckApiUri = EnvDataStore.downloadCheckApiUri;
    // ------------------------------------------------------------------------------------

    // ====================================================================================

    [Serializable]
    public class downloadCheckResponse
    {
        public bool success;
        public List<musicList> music;
    }

    [Serializable]
    public class musicList
    {
        public int id;
        public string name;
        public string url_easy;
        public string url_basic;
        public string url_hard;
        public string url_demon;
        public string url_music;
        public string url_music_preview;
        public string updated_at;
    }

    // ====================================================================================

    void Start()
    {
        this.showVersion.text = "Ver." + thisVersion;
        ScreenResponsive();
        StartCoroutine(DownloadCheckNetworkProcess());
    }

    private void ScreenResponsive()
    {
        float scale = 1f;
        if (Screen.width < 1920)
            scale = 1.5f;
        if (Screen.width < Screen.height)
            scale = (Screen.height * 16) / (Screen.width * 9);
        Background.sizeDelta = new Vector2(Screen.width * scale, Screen.height * scale);
    }

    IEnumerator DownloadCheckNetworkProcess()
    {
        UnityWebRequest www = UnityWebRequest.Get(downloadCheckApiUri);
        yield return www.SendWebRequest();
        if (www.isNetworkError)
        {
            Debug.LogError("ネットワークに接続できません．(" + www.error + ")");
        }
        else
        {
            ResponseCheck(www.downloadHandler.text);
        }
    }

    private void ResponseCheck(string data)
    {
        downloadCheckResponse jsnData = JsonUtility.FromJson<downloadCheckResponse>(data);

        if (jsnData.success)
            CountsDownloadItems(jsnData);
        else
            Debug.Log("通信に失敗しました");
    }

    private void SelectScreenTransition()
    {
        SceneManager.LoadScene("SelectScene");
    }

    private void CountsDownloadItems(downloadCheckResponse jsnData)
    {
        // PlayerPrefs.DeleteAll(); //ユーザ情報を初期化したい場合にコメントアウトを解除

        int key = 1;
        List<int> downloadList = new List<int>();

        foreach (musicList music in jsnData.music)
        {
            if (PlayerPrefs.GetString("music_" + key) == "" || PlayerPrefs.GetString("music_" + key) != music.updated_at)
            {
                Debug.Log("RESET: key=" + key);
                downloadList.Add(key);
                PlayerPrefs.SetString("music_" + key, music.updated_at);
            }
            key++;
        }

        if (!(downloadList?.Count > 0)) SelectScreenTransition();
        else Download(jsnData, downloadList);
    }

    private void Download(downloadCheckResponse jsnData, List<int> downloadList)
    {
        foreach (int downloadNum in downloadList)
        {
            this.Percent.text = "Download (" + downloadNum + " / " + downloadList.Count + ")";
            DownloadMusic(jsnData.music[downloadNum - 1].name, jsnData.music[downloadNum - 1].url_music);
            if (downloadNum == downloadList.Count)
                SelectScreenTransition(); //TODO: 同期関数に変更したい
        }
    }

    private void DownloadMusic(string fileName, string googleDriveUrl)
    {
        string downloadUrl = "https://drive.google.com/uc?id=" + googleDriveUrl.Replace("https://drive.google.com/file/d/", "").Replace("/view?usp=sharing", "") + "&usp=sharing";
        StartCoroutine(SaveMusic(fileName, downloadUrl));
        StartCoroutine(SaveMusicPreview(fileName, downloadUrl));
    }

    private IEnumerator SaveMusic(string fileName, string url)
    {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
        yield return www.Send();
        if (www.isNetworkError)
        {
            Debug.LogWarning("Audio error:" + www.error);
        }
        else
        {
            SavWav.Save(fileName, "/Project/Resources/Music/", ((DownloadHandlerAudioClip)www.downloadHandler).audioClip);
        }
    }

    private IEnumerator SaveMusicPreview(string fileName, string url)
    {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
        yield return www.Send();
        if (www.isNetworkError)
        {
            Debug.LogWarning("Audio error:" + www.error);
        }
        else
        {
            SavWav.Save(fileName, "/Project/Resources/MusicPreview/", ((DownloadHandlerAudioClip)www.downloadHandler).audioClip);
        }
    }
}