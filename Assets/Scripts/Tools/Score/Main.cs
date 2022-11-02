using System;

namespace Tools.Score
{
    public static partial class ScoreHandler
    {
        public static void Initialize(int totalNotes)
        {
            _totalNotes = totalNotes;
            BaseScoreCalculation();
        }

        /// <summary>
        ///   ダウンロードした AssetBundle を返します.
        ///   AssetBundle のダウンロード完了後に アクセスする必要があります.
        /// </summary>
        public static void AddScore(Judge judge) => _AddScore(judge);


        /// <summary>
        ///   AssetBundle のダウンロード完了率が 変更された際に呼び出されます.
        ///   0~100の値 (Int32) が返されます.
        /// </summary>
        public static event Action<int> OnComboChanged;

        /// <summary>
        ///   AssetBundle のダウンロード完了率が 変更された際に呼び出されます.
        ///   0~100の値 (Int32) が返されます.
        /// </summary>
        public static event Action<float> OnScoreChanged;

        /// <summary>
        ///   AssetBundle のダウンロード中に エラーが発生した際に呼び出されます.
        ///   エラーの内容 (String) が返されます.
        /// </summary>
        public static event Action<string> OnErrorOccured;
    }
}