using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleImage : MonoBehaviour
{
    public Graphic offGraphic;

    void Start()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener((value) =>
        {
            OnValueChanged(value);
        });
        offGraphic.enabled = !toggle.isOn;
    }

    void OnValueChanged(bool value)
    {
        if (offGraphic != null)
        {
            offGraphic.enabled = !value;
        }
    }
}