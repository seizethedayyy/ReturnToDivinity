using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SfxEntry
{
    public string key;        // 고유 키 이름 (예: "PlayerAttack1")
    public AudioClip clip;    // 실제 효과음 파일
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("오디오 믹서")]
    public AudioMixer audioMixer;
    public AudioMixerGroup bgmGroup;
    public AudioMixerGroup sfxGroup;

    [Header("오디오 소스")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("효과음 등록 및 관리")]
    public List<SfxEntry> sfxEntries = new List<SfxEntry>();
    private Dictionary<string, AudioClip> sfxMap = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (bgmSource != null) bgmSource.outputAudioMixerGroup = bgmGroup;
            if (sfxSource != null) sfxSource.outputAudioMixerGroup = sfxGroup;

            InitializeSfxMap();
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    private void InitializeSfxMap()
    {
        sfxMap.Clear();
        foreach (var entry in sfxEntries)
        {
            if (!string.IsNullOrEmpty(entry.key) && entry.clip != null && !sfxMap.ContainsKey(entry.key))
            {
                sfxMap.Add(entry.key, entry.clip);
            }
        }
    }

    private void Start()
    {
        // 저장된 설정 불러와서 반영
        bool bgmOn = PlayerPrefs.GetInt("BGM_ON", 1) == 1;
        bool sfxOn = PlayerPrefs.GetInt("SFX_ON", 1) == 1;
        float bgmVol = PlayerPrefs.GetFloat("BGM_VOL", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFX_VOL", 1f);

        ToggleBgm(bgmOn);
        ToggleSfx(sfxOn);
        SetBgmVolume(bgmVol);
        SetSfxVolume(sfxVol);
    }

    // 🔊 기존 AudioClip 직접 재생 방식 (UI 버튼 등에서 사용 중)
    public void PlaySfx(AudioClip clip)
    {
        if (clip != null && sfxSource != null && !sfxSource.mute)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // 🔊 키 기반 등록 효과음 재생 방식 (PlayerAttack1 등)
    public void PlaySfx(string key)
    {
        if (sfxMap.TryGetValue(key, out AudioClip clip) && sfxSource != null && !sfxSource.mute)
        {
            sfxSource.PlayOneShot(clip);
            Debug.Log($"[AudioManager] SFX 재생: {key}");
        }
        else
        {
            Debug.LogWarning($"[AudioManager] 재생 실패: 등록되지 않은 SFX 키 or SFX 비활성화 ({key})");
        }
    }

    // 🔊 BGM 재생
    public void PlayBgm(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public bool IsBgmPlaying()
    {
        return bgmSource != null && bgmSource.isPlaying;
    }

    // 🎚️ 볼륨 설정
    public void SetBgmVolume(float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("BGMVolume", dB);
    }

    public void SetSfxVolume(float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("SFXVolume", dB);
    }

    // 🔇 온/오프
    public void ToggleBgm(bool isOn)
    {
        if (bgmSource != null)
        {
            bgmSource.mute = !isOn;
        }
    }

    public void ToggleSfx(bool isOn)
    {
        if (sfxSource != null)
        {
            sfxSource.mute = !isOn;
        }
    }
}