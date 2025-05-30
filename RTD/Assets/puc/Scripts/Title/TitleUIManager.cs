using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleUIManager : MonoBehaviour
{
    [Header("공용 버튼 클릭 사운드")]
    public AudioClip buttonClickSfx;

    private void Start()
    {
        CleanupInGameObjects(); // 타이틀씬 진입 시 InGameUI 제거

        // 타이틀 씬에서 OptionManager가 다시 OptionUI를 찾게 한다
        if (OptionManager.Instance != null)
        {
            OptionManager.Instance.SendMessage("RebindOptionUI"); // or 직접 호출 가능 시: .RebindOptionUI()
            Debug.Log("[TitleUIManager] OptionUI 재연결 요청 완료");
        }
    }

    /// <summary>
    /// 타이틀씬에서 DontDestroyOnLoad된 오브젝트 제거
    /// </summary>
    private void CleanupInGameObjects()
    {
        // ✅ InGameUI 제거
        GameObject inGameUI = GameObject.FindWithTag("InGameUI");
        if (inGameUI != null)
        {
            Destroy(inGameUI);
            Debug.Log("[TitleUIManager] InGameUIManager 제거 완료");
        }

        // ✅ 선택된 캐릭터 프리팹 및 데이터 제거
        if (SelectedCharacterData.Instance != null)
        {
            if (SelectedCharacterData.Instance.selectedCharacterPrefab != null)
            {
                Destroy(SelectedCharacterData.Instance.selectedCharacterPrefab);
                Debug.Log("[TitleUIManager] selectedCharacterPrefab 제거 완료");
            }

            Destroy(SelectedCharacterData.Instance.gameObject);
            Debug.Log("[TitleUIManager] SelectedCharacterData 오브젝트 제거 완료");
        }
    }

    /// <summary>
    /// 버튼 클릭 사운드 재생
    /// </summary>
    private void PlayClickSound()
    {
        if (AudioManager.Instance != null && buttonClickSfx != null)
        {
            AudioManager.Instance.PlaySfx(buttonClickSfx);
        }
        else
        {
            Debug.LogWarning("[TitleUIManager] 효과음 재생 실패: AudioManager 또는 AudioClip 누락");
        }
    }

    public void OnClickGameStart()
    {
        if (SceneLoadData.Instance != null)
        {
            SceneLoadData.Instance.EnteredFromGameStart = true;
            Debug.Log("[타이틀] 게임 시작 → EnteredFromGameStart = true");
        }

        StartCoroutine(GameStartDelayed());
    }

    private IEnumerator GameStartDelayed()
    {
        PlayClickSound();
        yield return new WaitForSeconds(0.3f); // 사운드 출력 시간 확보
        SceneManager.LoadScene("OpeningScene");
    }

    public void OnClickOption()
    {
        PlayClickSound();

        if (OptionManager.Instance == null)
        {
            Debug.LogError("[TitleUIManager] OptionManager.Instance가 null입니다!");
        }
        else
        {
            Debug.Log("OptionManager 호출 성공");
            OptionManager.Instance.ShowOption();
        }
    }

    public void OnClickExit()
    {
        StartCoroutine(ExitDelayed());
    }

    private IEnumerator ExitDelayed()
    {
        PlayClickSound();
        yield return new WaitForSeconds(0.3f); // 사운드 출력 시간 확보
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}