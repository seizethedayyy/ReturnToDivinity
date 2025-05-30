using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [Header("BGM")]
    public Toggle bgmToggle;
    public Slider bgmSlider;

    [Header("SFX")]
    public Toggle sfxToggle;
    public Slider sfxSlider;

    [Header("버튼")]
    public Button saveButton;
    public Button closeButton;

    [Header("효과음")]
    public AudioClip buttonClickSfx;

    private IEnumerator Start()
    {
        yield return null; // AudioManager 초기화 대기

        // 저장된 설정 불러오기
        bgmToggle.isOn = PlayerPrefs.GetInt("BGM_ON", 1) == 1;
        sfxToggle.isOn = PlayerPrefs.GetInt("SFX_ON", 1) == 1;
        bgmSlider.value = PlayerPrefs.GetFloat("BGM_VOL", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFX_VOL", 1f);

        // AudioManager에 반영
        ApplySettings();

        // 슬라이더 값 변경 시 실시간 적용
        bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBgmVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSfxVolume);

        // Toggle 값 변경 시 바로 적용
        bgmToggle.onValueChanged.AddListener(AudioManager.Instance.ToggleBgm);
        sfxToggle.onValueChanged.AddListener(AudioManager.Instance.ToggleSfx);

        // 버튼 클릭 시 설정 저장 및 닫기
        saveButton.onClick.AddListener(SaveSettings);
        closeButton.onClick.AddListener(CloseUI);
    }

    private void ApplySettings()
    {
        AudioManager.Instance.ToggleBgm(bgmToggle.isOn);
        AudioManager.Instance.ToggleSfx(sfxToggle.isOn);
        AudioManager.Instance.SetBgmVolume(bgmSlider.value);
        AudioManager.Instance.SetSfxVolume(sfxSlider.value);
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.sfxSource != null && buttonClickSfx != null)
        {
            AudioManager.Instance.PlaySfx(buttonClickSfx);
        }
        else
        {
            Debug.LogWarning("[OptionUI] 효과음 재생 실패: AudioManager 또는 SFXSource 또는 AudioClip이 누락됨");
        }
    }

    public void SaveSettings()
    {
        PlayClickSound();

        PlayerPrefs.SetInt("BGM_ON", bgmToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("SFX_ON", sfxToggle.isOn ? 1 : 0);
        PlayerPrefs.SetFloat("BGM_VOL", bgmSlider.value);
        PlayerPrefs.SetFloat("SFX_VOL", sfxSlider.value);
        PlayerPrefs.Save();

        gameObject.SetActive(false);
    }

    public void CloseUI()
    {
        PlayClickSound();
        OptionManager.Instance.HideOption();
    }
}