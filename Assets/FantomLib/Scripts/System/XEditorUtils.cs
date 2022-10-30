using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FantomLib
{

#if UNITY_EDITOR

    /// <summary>
    /// エディタ上のユーティリティ
    /// 2020/10/18 Fantom
    /// http://fantom1x.blog130.fc2.com/blog-entry-373.html
    /// </summary>
    public static partial class XEditorUtils
    {
        //Game View の解像度の取得済み キャッシュ
        static string _gameViewScreenRes;       //解像度文字列
        static Vector2Int _gameViewResolution;  //Parse した w, h

        /// <summary>
        /// Game View の解像度を取得する
        /// </summary>
        /// <returns></returns>
        public static Vector2Int GetGameViewResolution()
        {
            var screenRes = UnityStats.screenRes;  //"1280x720" 等
            if (screenRes != _gameViewScreenRes)
            {
                _gameViewScreenRes = screenRes;
                var res = UnityStats.screenRes.Split('x');
                _gameViewResolution = new Vector2Int(int.Parse(res[0]), int.Parse(res[1]));
            }
            return _gameViewResolution;
        }
    }

#endif

}
