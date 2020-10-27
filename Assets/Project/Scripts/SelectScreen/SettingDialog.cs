using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingDialog : MonoBehaviour
{
    // --- Attach from Unity --------------------------------------------------------------
    public GameObject SettingPanel = null;
    public Slider NotesFallSpeedSlider;
    public Slider TimingAdjustmentSlider;
    public Toggle NotesTouchSoundToggle;
    public Toggle LowGraphicsModeToggle;
    // ------------------------------------------------------------------------------------

    public void SettingTappedController()
    {
        SettingPanel.SetActive(true);
        NotesFallSpeedSlider.value = PlayerPrefs.GetInt("NotesFallSpeed", 5);
        TimingAdjustmentSlider.value = PlayerPrefs.GetInt("TimingAdjustment", 5);
        NotesTouchSoundToggle.isOn = (PlayerPrefs.GetInt("NotesTouchSound", 1) == 1 ? true : false);
        LowGraphicsModeToggle.isOn = (PlayerPrefs.GetInt("LowGraphicsMode", 0) == 1 ? true : false);
    }

    public void OnNotesFallSpeedSliderChanged()
    {
        PlayerPrefs.SetInt("NotesFallSpeed", (int)NotesFallSpeedSlider.value);
    }

    public void OnTimingAdjustmentSliderChanged()
    {
        PlayerPrefs.SetInt("TimingAdjustment", (int)TimingAdjustmentSlider.value);
    }

    public void OnNotesTouchSoundToggleChanged()
    {
        PlayerPrefs.SetInt("NotesTouchSound", NotesTouchSoundToggle.isOn ? 1 : 0);
    }

    public void OnLowGraphicsModeToggleChanged()
    {
        PlayerPrefs.SetInt("LowGraphicsMode", LowGraphicsModeToggle.isOn ? 1 : 0);
    }

    public void SaveTappedController()
    {
        SettingPanel.SetActive(false);
        PlayerPrefs.Save();
    }
}