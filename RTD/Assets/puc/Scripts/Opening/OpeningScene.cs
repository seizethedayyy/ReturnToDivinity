using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningScene : MonoBehaviour
{

    private bool isLoading = false;

    void Update()
    {
        if (Input.anyKeyDown && !isLoading)
        {
            StartCoroutine(LoadNextScene());
        }
    }

    private IEnumerator LoadNextScene()
    {
        isLoading = true;

        // 🔊 (선택) 효과음 재생
        // if (AudioManager.Instance != null && sfxClip != null)
        // {
        //     AudioManager.Instance.PlaySfx(sfxClip);
        // }

        yield return new WaitForSeconds(0.3f); // ⏳ 여기서 0.3초 대기
                        
        SceneManager.LoadScene("CharacterSelectScene");
    }
}