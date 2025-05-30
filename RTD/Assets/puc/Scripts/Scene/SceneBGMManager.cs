using UnityEngine;

public class SceneBGMManager : MonoBehaviour
{
    [Header("빌리지 전용 BGM")]
    public AudioClip villageBgm;

    private void Start()
    {
        if (AudioManager.Instance != null && villageBgm != null)
        {
            AudioManager.Instance.PlayBgm(villageBgm);
        }
    }

    private void OnDestroy()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBgm();
        }
    }
}