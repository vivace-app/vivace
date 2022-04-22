using System;
using System.Collections;
using Project.Scripts.Tools.Firestore.Model;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Project.Scripts.Tools.CloudStorageHandler;

namespace Project.Scripts.Tools.AssetBundleHandler
{
    public partial class Main
    {
        private int _downloadedAssetBundles;
        private int _totalAssetBundles;

        private static AssetBundle[] _assetBundles;
        private static Music[] _musics;

        private int GetCompletionRate() => _downloadedAssetBundles * 100 / _totalAssetBundles;

        private IEnumerator DownloadAssetBundles()
        {
            _totalAssetBundles = _musics?.Length ?? 0;
            _assetBundles = new AssetBundle[_totalAssetBundles];

            void DownloadCompleted()
            {
                _downloadedAssetBundles++;
                OnCompletionRateChanged?.Invoke(GetCompletionRate());
                if (_downloadedAssetBundles == _totalAssetBundles)
                    OnDownloadCompleted?.Invoke();
            }

            if (_musics == null)
                OnDownloadCompleted?.Invoke();
            else
                foreach (var music in _musics)
                    yield return DownloadAssetBundle(music, DownloadCompleted);
        }

        private static IEnumerator DownloadAssetBundle(Music music, Action end)
        {
            // Get all the cached versions
            // var hash = Hash128.Parse($"{music.Name}_{music.Version}");
            // var listOfCachedVersions = new List<Hash128>();
            // Caching.GetCachedVersions(music.Name, listOfCachedVersions);
            // foreach (var cachedVersions in listOfCachedVersions.Where(cachedVersions => cachedVersions == hash))
            // {
            //     Debug.Log($"{music.Title}のキャッシュが見つかりました: {cachedVersions}");
            //     end.Invoke();
            //     yield break;
            // }

            var c = new CachedAssetBundle
            {
                name = music.Name,
                hash = Hash128.Parse($"{music.Name}_{music.Version}")
            };

            var ie = GenerateDownloadUrl(music);
            yield return ie;
            var downloadUrl = (Uri)ie.Current;

            while (!Caching.ready)
                yield return null;

            using var request = UnityWebRequestAssetBundle.GetAssetBundle(downloadUrl, c);
            yield return request.SendWebRequest();

            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    if (request.downloadHandler is DownloadHandlerAssetBundle handler)
                    {
                        _assetBundles[music.Id - 1] = handler.assetBundle;
                        Debug.Log("アセットをDLしました: " + music.Id);
                    }

                    end.Invoke();
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

        private static IEnumerator GenerateDownloadUrl(Music music)
        {
            var storage = new CloudStorage();
            IEnumerator ie;
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    ie = storage.FetchTheDownloadURL(music.AssetBundleIos);
                    yield return ie;
                    yield return (Uri)ie.Current;
                    break;
                case RuntimePlatform.Android:
                    ie = storage.FetchTheDownloadURL(music.AssetBundleAndroid);
                    yield return ie;
                    yield return (Uri)ie.Current;
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    ie = storage.FetchTheDownloadURL(music.AssetBundleStandaloneOsxUniversal);
                    yield return ie;
                    yield return (Uri)ie.Current;
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    ie = storage.FetchTheDownloadURL(music.AssetBundleStandaloneWindows64);
                    yield return ie;
                    yield return (Uri)ie.Current;
                    break;
                default:
                    Debug.LogError("対応したAssetBundleが見つかりませんでした");
                    yield return new Uri("");
                    break;
            }
        }
    }
}