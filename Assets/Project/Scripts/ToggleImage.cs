using UnityEngine;
using UnityEngine.UI;

// 難易度のToggleの非アクティブ時の画像を切り替えます
namespace Project.Scripts
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleImage : MonoBehaviour
    {
        public Graphic offGraphic;

        private void Start()
        {
            var toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnValueChanged);
            offGraphic.enabled = !toggle.isOn;
        }

        public void OnValueChanged(bool value)
        {
            if (offGraphic != null) offGraphic.enabled = !value;
        }
    }
}