using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DownloadProcessManager : MonoBehaviour
{
    // --- Attach from Unity --------------------------------------------------------------
    public RectTransform background;
    public Text downloadPercentage;

    public Text showVersion;
    // ------------------------------------------------------------------------------------

    // --- External variables -------------------------------------------------------------
    public static AssetBundle[] AssetBundle; // 参照: SwipeMenu.
    public static List<MusicList> MusicData; // 参照: SwipeMenu.
    // ------------------------------------------------------------------------------------

    // --- Environment variables ----------------------------------------------------------
    private static readonly string ThisVersion = EnvDataStore.thisVersion;

    private static readonly string DownloadCheckApiUri = EnvDataStore.downloadCheckApiUri;
    // ------------------------------------------------------------------------------------

    private int _musicCounts = 999999;
    private int _downloadedMusicCounts;

    // ====================================================================================

    [Serializable]
    public class DownloadCheckResponse
    {
        public bool success;
        public List<MusicList> music;
    }

    [Serializable]
    public class MusicList
    {
        public int id;
        public string name;
        public string title;
        public string artist;
        public string asset_bundle_ios;
        public string asset_bundle_android;
        public string updated_at;
    }

    // ====================================================================================

    private void Start()
    {
        showVersion.text = "Ver." + ThisVersion;
        BackgroundCover();
        StartCoroutine(DownloadCheckNetworkProcess());
    }

    private void Update()
    {
        if (_downloadedMusicCounts == _musicCounts) SelectScreenTransition();
    }

    /// <summary>
    /// 背景画像を画面いっぱいに広げます．
    /// </summary>
    private void BackgroundCover()
    {
        var scale = 1f;
        if (Screen.width < 1920)
            scale = 1.5f;
        if (Screen.width < Screen.height)
            scale = (float)(Screen.height * 16) / (Screen.width * 9);
        background.sizeDelta = new Vector2(Screen.width * scale, Screen.height * scale);
    }

    /// <summary>
    /// ダウンロードするAssetBundle一覧を取得します．
    /// </summary>
    private IEnumerator DownloadCheckNetworkProcess()
    {
        var request = UnityWebRequest.Get(DownloadCheckApiUri);
        yield return request.SendWebRequest();

        switch (request.result)
        {
            case UnityWebRequest.Result.Success:
                ResponseCheck(request.downloadHandler.text);
                break;

            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.ProtocolError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError("ネットワークに接続できません．(" + request.error + ")");
                SceneManager.LoadScene("StartupScene");
                break;

            case UnityWebRequest.Result.InProgress:
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// サーバからのレスポンスを処理します．
    /// </summary>
    private void ResponseCheck(string data)
    {
        var jsnData = JsonUtility.FromJson<DownloadCheckResponse>(data);

        if (jsnData.success)
            DownloadAssetBundles(jsnData);
        else
            Debug.Log("通信に失敗しました");
    }

    /// <summary>
    /// SelectSceneに遷移します．
    /// </summary>
    private static void SelectScreenTransition()
    {
        SceneManager.LoadScene("SelectScene");
    }

    /// <summary>
    /// AssetBundle一覧のダウンロード管理を行います．
    /// </summary>
    private void DownloadAssetBundles(DownloadCheckResponse jsnData)
    {
        string downloadUrl = null;
        MusicData = jsnData.music;
        _musicCounts = MusicData.Count;
        AssetBundle = new AssetBundle[_musicCounts];
        foreach (var music in MusicData)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    downloadUrl = "https://drive.google.com/uc?id=" +
                                  music.asset_bundle_ios.Replace("https://drive.google.com/file/d/", "")
                                      .Replace("/view?usp=sharing", "") + "&usp=sharing";
                    break;
                case RuntimePlatform.Android:
                    downloadUrl = "https://drive.google.com/uc?id=" +
                                  music.asset_bundle_android.Replace("https://drive.google.com/file/d/", "")
                                      .Replace("/view?usp=sharing", "") + "&usp=sharing";
                    break;
                default:
                    Debug.LogError("AssetBundleを使用するため，iOSまたはAndroidでデバッグを行ってください．");
                    Application.Quit();
                    break;
            }

            StartCoroutine(DownloadAssetBundle(music, downloadUrl));
        }
    }

    /// <summary>
    /// AssetBundleをダウンロードします．
    /// </summary>
    private IEnumerator DownloadAssetBundle(MusicList music, string downloadUrl)
    {
        Debug.Log(music.title + "のダウンロードを開始します");
        using var request = UnityWebRequestAssetBundle.GetAssetBundle(downloadUrl);
        yield return request.SendWebRequest();

        switch (request.result)
        {
            case UnityWebRequest.Result.Success:
                var handler = request.downloadHandler as DownloadHandlerAssetBundle;
                if (handler != null)
                {
                    AssetBundle[music.id - 1] = handler.assetBundle;
                }

                _downloadedMusicCounts++;
                downloadPercentage.text = _downloadedMusicCounts + "/" + _musicCounts;
                break;

            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.ProtocolError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError("ネットワークに接続できません．(" + request.error + ")");
                SceneManager.LoadScene("StartupScene");
                break;

            case UnityWebRequest.Result.InProgress:
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
}