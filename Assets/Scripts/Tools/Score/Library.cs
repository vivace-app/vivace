using System;

namespace Tools.Score
{
    public static partial class ScoreHandler
    {
        private static int _combo;
        private static float _totalScore;
        private static int _perfect;
        private static int _great;
        private static int _good;
        private static int _miss;

        private static int _totalNotes;
        private static float _baseScore;
        private static float[] _logSq;
        private const int SepPoint = 50;

        public enum Judge
        {
            Perfect,
            Great,
            Good,
            Miss
        }

        /// <summary>
        /// 区分求積法によるスコア計算の値を用意しておきます．
        /// </summary>
        private static void BaseScoreCalculation()
        {
            _logSq = new float[SepPoint];

            var logSqSum = 0f;

            var denominator = _totalNotes >= SepPoint ? SepPoint : _totalNotes;

            for (var i = 0; i < denominator; i++)
            {
                _logSq[i] = (float) Math.Log10(1 + 9 * ((float) i + 1) / denominator);
                logSqSum += _logSq[i];
            }

            _baseScore = 1000000 / (logSqSum + _totalNotes - denominator);
        }

        /// <summary>
        /// スコアを加算します．
        /// </summary>
        private static void _AddScore(Judge judge)
        {
            float magnitude;

            switch (judge)
            {
                case Judge.Perfect:
                    magnitude = 1f;
                    _combo++;
                    _perfect++;
                    break;
                case Judge.Great:
                    magnitude = 0.75f;
                    _combo++;
                    _great++;
                    break;
                case Judge.Good:
                    magnitude = 0.25f;
                    _combo = 0;
                    _good++;
                    break;
                case Judge.Miss:
                    magnitude = 0f;
                    _combo = 0;
                    _miss++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(judge), judge, null);
            }

            // コンボ数を更新
            OnComboChanged.Invoke(_combo);

            // 区分求積法による加算スコアの計算
            var scoreTemp = (_combo <= SepPoint) switch
            {
                true when _combo > 0 => _baseScore * _logSq[_combo - 1] * magnitude,
                true => _baseScore * _logSq[0] * magnitude,
                _ => _baseScore * magnitude
            };

            /* floatへの変更でAP時に100万点にならないので強制代入 */
            if (_perfect == _totalNotes) scoreTemp = 1000000 - _totalScore;

            _totalScore += scoreTemp;
            OnScoreChanged.Invoke(_totalScore);
        }
    }
}