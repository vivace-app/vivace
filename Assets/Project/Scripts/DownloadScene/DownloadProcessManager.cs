using System;
using System.Collections;
using System.Collections.Generic;
using Project.Scripts;
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
    private const string ThisVersion = EnvDataStore.ThisVersion;

    private const string DownloadCheckApiUri = EnvDataStore.ApiUri + "/downloadCheck";
    // ------------------------------------------------------------------------------------

    private int _musicCounts;
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
        public string asset_bundle_standalone_osx_universal;
        public string asset_bundle_standalone_windows_64;
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
        if (_musicCounts != 0 && _downloadedMusicCounts == _musicCounts) SelectScreenTransition();
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
        
        var cachePath = System.IO.Path.Combine(Application.persistentDataPath, "cache");
        System.IO.Directory.CreateDirectory(cachePath);
        var cache = Caching.AddCache(cachePath);
        Caching.currentCacheForWriting = cache;
        cache.expirationDelay = 200;
        
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
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    downloadUrl = "https://drive.google.com/uc?id=" +
                                  music.asset_bundle_standalone_osx_universal.Replace("https://drive.google.com/file/d/", "")
                                      .Replace("/view?usp=sharing", "") + "&usp=sharing";
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    downloadUrl = "https://drive.google.com/uc?id=" +
                                  music.asset_bundle_standalone_windows_64.Replace("https://drive.google.com/file/d/", "")
                                      .Replace("/view?usp=sharing", "") + "&usp=sharing";
                    break;
                default:
                    Debug.LogError("対応したAssetBundleが見つかりませんでした");
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
        var c = new CachedAssetBundle
        {
            name = music.name,
            hash = Hash128.Parse("hash128")
        };
        
        using var request = UnityWebRequestAssetBundle.GetAssetBundle(downloadUrl, c);
        yield return request.SendWebRequest();

        switch (request.result)
        {
            case UnityWebRequest.Result.Success:
                if (request.downloadHandler is DownloadHandlerAssetBundle handler)
                    AssetBundle[music.id - 1] = handler.assetBundle;

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