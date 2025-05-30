using UnityEngine;

public class TitleBGMManager : MonoBehaviour
{
    [Header("공용 BGM")]
    public AudioClip sharedBgm; // 타이틀/오프닝/캐릭터 선택 공통 BGM

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