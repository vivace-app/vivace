using Tools.PlayStatus;
using UnityEngine;
using UnityEngine.UI;

namespace SelectScene
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleHandler : MonoBehaviour
    {
        [SerializeField] private Graphic offGraphic;
        [SerializeField] private Level level;

        private Toggle _toggle;
        private PlayStatusHandler _playStatusHandler;

        private void Start()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(OnValueChanged);
            offGraphic.enabled = !_toggle.isOn;

            _playStatusHandler = new PlayStatusHandler();
            if (_toggle.isOn) PlayStatusHandler.SetSelectedLevel(level);
        }

        private void OnValueChanged(bool value)
        {
            if (offGraphic != null) offGraphic.enabled = !value;
            if (_toggle.isOn) PlayStatusHandler.SetSelectedLevel(level);
        }
    }
}