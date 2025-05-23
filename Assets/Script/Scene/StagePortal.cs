using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StagePortal : MonoBehaviour
{
    [Header("이 포탈의 고유 이름 (PlayerSpawnManager와 연결됨)")]
    public string portalName = "ToStageA";

    [Header("이동할 씬 이름 (Build Settings에 등록된 이름)")]
    public string destinationSceneName = "StageScene";

    [Header("다음 씬에서 매칭할 포탈 이름")]
    public string targetPortalName = "FromStageA";

    [Header("현재 씬에 플레이어가 소환될 위치")]
    public Transform spawnPoint;

    private bool hasTriggered = false;
    private Collider2D portalCollider;

    private float spawnIgnoreTime = 2f; // 씬 전환 직후 무시할 시간
    private float spawnTime;

    private void OnEnable()
    {
        spawnTime = Time.time;
    }

    private void Awake()
    {
        portalCollider = GetComponent<Collider2D>();
        if (portalCollider != null)
            portalCollider.enabled = false; // 초기 비활성화
    }

    public void EnablePortalAfterSpawn()
    {
        StartCoroutine(EnableWithDelay());
    }

    private IEnumerator EnableWithDelay()
    {
        yield return new WaitForSeconds(0.1f); // 플레이어 스폰 후 잠시 대기
        if (portalCollider != null)
        {
            portalCollider.enabled = true;
            Debug.Log($"[포탈] {portalName} 콜라이더 활성화됨");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[포탈 디버그] 충돌 발생 → other.name: {other.name}, 태그: {other.tag}");

        // ✅ VillageStart 포탈은 이동하지 않음
        if (portalName == "VillageStart")
        {
            Debug.Log("[포탈] VillageStart는 이동 기능 없이 시작 위치만 제공");
            return;
        }

        if (Time.time - spawnTime < spawnIgnoreTime) return;
        if (hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;

        if (SceneLoadData.Instance != null)
        {
            SceneLoadData.Instance.LastPortalName = targetPortalName;
            Debug.Log($"[포탈] 다음 씬 LastPortalName 설정 완료: {targetPortalName}");
        }

        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return null;
        SceneManager.LoadScene(destinationSceneName);
    }
}