using UnityEngine;
using UnityEngine.UI;

namespace SelectScreen
{
    public class SettingDialog : MonoBehaviour
    {
        // --- Attach from Unity --------------------------------------------------------------
        public GameObject settingPanel;
        public Slider notesFallSpeedSlider;
        public Slider timingAdjustmentSlider;
        public Toggle notesTouchSoundToggle;
        public Toggle lowGraphicsModeToggle;
        // ------------------------------------------------------------------------------------

        public void SettingTappedController()
        {
            settingPanel.SetActive(true);
            notesFallSpeedSlider.value = PlayerPrefs.GetInt("NotesFallSpeed", 5);
            timingAdjustmentSlider.value = PlayerPrefs.GetInt("TimingAdjustment", 5);
            notesTouchSoundToggle.isOn = PlayerPrefs.GetInt("NotesTouchSound", 1) == 1;
            lowGraphicsModeToggle.isOn = PlayerPrefs.GetInt("LowGraphicsMode", 0) == 1;
        }

        public void OnNotesFallSpeedSliderChanged()
        {
            PlayerPrefs.SetInt("NotesFallSpeed", (int) notesFallSpeedSlider.value);
        }

        public void OnTimingAdjustmentSliderChanged()
        {
            PlayerPrefs.SetInt("TimingAdjustment", (int) timingAdjustmentSlider.value);
        }

        public void OnNotesTouchSoundToggleChanged()
        {
            PlayerPrefs.SetInt("NotesTouchSound", notesTouchSoundToggle.isOn ? 1 : 0);
        }

        public void OnLowGraphicsModeToggleChanged()
        {
            PlayerPrefs.SetInt("LowGraphicsMode", lowGraphicsModeToggle.isOn ? 1 : 0);
        }

        public void SaveTappedController()
        {
            settingPanel.SetActive(false);
            PlayerPrefs.Save();
        }
    }
}