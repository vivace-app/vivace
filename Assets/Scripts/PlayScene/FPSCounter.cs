using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.PlayScene
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 1.0f)] private float everyCalculationTime = 0.5f;
        [SerializeField] private Text fpsText;

        private int _frameCount;
        private float _prevTime;

        private void Start()
        {
            _frameCount = 0;
            _prevTime = 0.0f;
            fpsText.text = "-----";
        }

        private void Update()
        {
            _frameCount++;
            var time = Time.realtimeSinceStartup - _prevTime;

            if (!(time >= everyCalculationTime)) return;
            fpsText.text = (_frameCount / time).ToString(CultureInfo.InvariantCulture) + "fps";

            _frameCount = 0;
            _prevTime = Time.realtimeSinceStartup;
        }
    }
}