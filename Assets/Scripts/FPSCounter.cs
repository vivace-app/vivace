#if DEBUG
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace PlayScene
{
    /// <summary>
    /// 現在のFPSをTextに表示します
    /// 【値の説明】
    /// everyCalculationTime: 指定した秒ごとに計測結果を反映する（小さいほど高頻度）
    /// displayText: 値を表示するTextをアタッチ
    /// </summary>
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 1.0f)] private float everyCalculationTime = 0.5f;
        [SerializeField] private Text displayText;

        private int _frameCount;
        private float _prevTime;

        private void Update()
        {
            _frameCount++;
            var time = Time.realtimeSinceStartup - _prevTime;

            if (!(time >= everyCalculationTime)) return;
            displayText.text = (_frameCount / time).ToString(CultureInfo.InvariantCulture) + "fps";

            _frameCount = 0;
            _prevTime = Time.realtimeSinceStartup;
        }
    }
}
#endif