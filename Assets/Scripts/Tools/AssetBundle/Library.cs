using System;
using System.Collections;
using Project.Scripts.Tools.CloudStorage;
using Project.Scripts.Tools.Firestore.Model;
using UnityEngine;
using UnityEngine.Networking;

namespace Project.Scripts.Tools.AssetBundle
{
    public partial class AssetBundleHandler
    {
        private int _downloadedAssetBundles;
        private int _totalAssetBundles;

        private static UnityEngine.AssetBundle[] _assetBundles;
        private static Music[] _musics;

        private int GetCompletionRate() => _downloadedAssetBundles * 100 / _totalAssetBundles;

        private IEnumerator DownloadAssetBundles()
        {
            _totalAssetBundles = _musics?.Length ?? 0;
            _assetBundles = new UnityEngine.AssetBundle[_totalAssetBundles];

            void DownloadCompleted()
            {
                _downloadedAssetBundles++;
                OnCompletionRateChanged.Invoke(GetCompletionRate());
                if (_downloadedAssetBundles == _totalAssetBundles)
                    OnDownloadCompleted.Invoke();
            }

            if (_musics == null)
                OnDownloadCompleted.Invoke();
            else
                foreach (var music in _musics)
                    yield return DownloadAssetBundle(music, DownloadCompleted, OnErrorOccured);
        }

        private static IEnumerator DownloadAssetBundle(Music music, Action end, Action<string> error)
        {
            var c = new CachedAssetBundle
            {
                name = music.Name,
                hash = Hash128.Parse($"{music.Name}_{music.Version}")
            };

            var ie = GenerateDownloadUrl(music, error);
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
                        _assetBundles[music.Id - 1] = handler.assetBundle;

                    end.Invoke();
                    break;

                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    error.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
                    break;

                case UnityWebRequest.Result.InProgress:
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private static IEnumerator GenerateDownloadUrl(Music music, Action<string> error)
        {
            var storage = new CloudStorageHandler();
            storage.OnErrorOccured += error.Invoke;
            IEnumerator ie;
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    ie = storage.GenerateDownloadURL(music.AssetBundleIos);
                    yield return ie;
                    yield return (Uri)ie.Current;
                    break;
                case RuntimePlatform.Android:
                    ie = storage.GenerateDownloadURL(music.AssetBundleAndroid);
                    yield return ie;
                    yield return (Uri)ie.Current;
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    ie = storage.GenerateDownloadURL(music.AssetBundleStandaloneOsxUniversal);
                    yield return ie;
                    yield return (Uri)ie.Current;
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    ie = storage.GenerateDownloadURL(music.AssetBundleStandaloneWindows64);
                    yield return ie;
                    yield return (Uri)ie.Current;
                    break;
                default:
                    error.Invoke("不明なプラットフォームです");
                    yield return new Uri("");
                    break;
            }
        }
    }
}