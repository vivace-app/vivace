using System;
using System.Collections;
using Project.Scripts.Tools.Firestore.Model;
using UnityEngine;

namespace Project.Scripts.Tools.AssetBundleHandler
{
    public partial class Main
    {
        // Constructor
        public Main(Music[] musics) => _musics = musics;

        public static AssetBundle[] GetAssetBundles() => _assetBundles;
        public static Music[] GetMusics() => _musics;

        public IEnumerator Download() => DownloadAssetBundles();

        public event Action<int> OnCompletionRateChanged;
        public event Action OnDownloadCompleted;
    }
}