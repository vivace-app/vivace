using UnityEngine;
using UnityEngine.UI;

namespace SelectScene
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleHandler : MonoBehaviour
    {
        [SerializeField] private Graphic offGraphic;

        private void Start()
        {
            var toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnValueChanged);
            offGraphic.enabled = !toggle.isOn;
        }

        private void OnValueChanged(bool value)
        {
            if (offGraphic != null) offGraphic.enabled = !value;
        }
    }
}