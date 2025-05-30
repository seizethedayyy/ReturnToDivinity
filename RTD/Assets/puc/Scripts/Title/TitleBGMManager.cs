using UnityEngine;

public class TitleBGMManager : MonoBehaviour
{
    [Header("���� BGM")]
    public AudioClip sharedBgm; // Ÿ��Ʋ/������/ĳ���� ���� ���� BGM

    private void Start()
    {
        if (AudioManager.Instance != null && sharedBgm != null)
        {
            if (!AudioManager.Instance.IsBgmPlaying())
            {
                AudioManager.Instance.PlayBgm(sharedBgm);
            }
        }
    }
}