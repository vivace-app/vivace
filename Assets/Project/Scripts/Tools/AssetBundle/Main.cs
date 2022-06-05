using System;
using System.Collections;
using Project.Scripts.Tools.Firestore.Model;

namespace Project.Scripts.Tools.AssetBundle
{
    public partial class AssetBundleHandler
    {
        public AssetBundleHandler(Music[] musics) => _musics = musics;

        /// <summary>
        ///   ダウンロードした AssetBundle を返します.
        ///   AssetBundle のダウンロード完了後に アクセスする必要があります.
        /// </summary>
        public static UnityEngine.AssetBundle[] GetAssetBundles() => _assetBundles;

        /// <summary>
        ///   AssetBundle に対応する Musicリスト を返します.
        /// </summary>
        public static Music[] GetMusics() => _musics;


        /// <summary>
        ///   Musicリスト にある AssetBundle をダウンロードします.
        /// </summary>
        public IEnumerator DownloadCoroutine() => DownloadAssetBundles();


        /// <summary>
        ///   AssetBundle のダウンロード完了率が 変更された際に呼び出されます.
        ///   0~100の値 (Int32) が返されます.
        /// </summary>
        public event Action<int> OnCompletionRateChanged;

        /// <summary>
        ///   AssetBundle のダウンロードが 完了した際に呼び出されます.
        /// </summary>
        public event Action OnDownloadCompleted;

        /// <summary>
        ///   AssetBundle のダウンロード中に エラーが発生した際に呼び出されます.
        ///   エラーの内容 (String) が返されます.
        /// </summary>
        public event Action<string> OnErrorOccured;
    }
}