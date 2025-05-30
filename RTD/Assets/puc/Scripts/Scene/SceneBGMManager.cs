using UnityEngine;

public class SceneBGMManager : MonoBehaviour
{
    [Header("������ ���� BGM")]
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