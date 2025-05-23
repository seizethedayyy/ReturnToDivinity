using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Unity.Cinemachine;

public class PlayerSpawnManager : MonoBehaviour
{
    public PortalDatabase portalDatabase;

    private GameObject player;
    private Rigidbody2D playerRb;
    private CinemachineCamera virtualCamera;

    private bool confinerResetDone = false;

    private void Awake()
    {
        RemoveDuplicateEventSystems();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        confinerResetDone = false;
        StartCoroutine(DelayedResetCameraConfiner());
    }

    private IEnumerator DelayedResetCameraConfiner()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        yield return StartCoroutine(ResetCameraConfiner());
    }

    private void Start()
    {
        StartCoroutine(WaitForCameraAndSpawn());
    }

    private void RemoveDuplicateEventSystems()
    {
        var eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        if (eventSystems.Length > 1)
        {
            for (int i = 1; i < eventSystems.Length; i++)
            {
                Destroy(eventSystems[i].gameObject);
                Debug.Log("[이벤트시스템] 중복된 EventSystem 제거 완료");
            }
        }
    }

    private IEnumerator WaitForCameraAndSpawn()
    {
        LoadingUIManager.Instance?.ShowLoading();

        try
        {
            float waitTime = 0f;
            float timeout = 3f;

            while (virtualCamera == null && waitTime < timeout)
            {
                var cameras = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
                if (cameras.Length > 1)
                {
                    for (int i = 1; i < cameras.Length; i++)
                    {
                        Destroy(cameras[i].gameObject);
                        Debug.Log("[스폰] 중복 CinemachineCamera 제거 완료");
                    }

                    virtualCamera = cameras[0];
                    HandleCameraRootPersistence(virtualCamera);
                }
                else if (cameras.Length == 1)
                {
                    virtualCamera = cameras[0];
                    HandleCameraRootPersistence(virtualCamera);
                }

                waitTime += Time.unscaledDeltaTime;
                yield return null;
            }

            if (virtualCamera == null)
                Debug.LogWarning("[스폰] CinemachineCamera를 찾지 못했습니다. 스폰은 계속 진행됩니다.");

            yield return StartCoroutine(ApplySpawn());
        }
        finally
        {
            LoadingUIManager.Instance?.HideLoading();
        }
    }

    private void HandleCameraRootPersistence(CinemachineCamera cam)
    {
        var parent = cam.transform.root;

        // 이미 DontDestroyOnLoad 씬에 존재한다면 중복 방지
        bool isAlreadyPersistent = parent.gameObject.scene.name == "DontDestroyOnLoad";
        if (isAlreadyPersistent)
        {
            Debug.Log($"[카메라] Settings({parent.name})는 이미 DontDestroyOnLoad 상태 → 생략");
            return;
        }

        // DontDestroyOnLoad에 있는 동일 이름 오브젝트가 있는지 직접 확인
        var existing = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (var t in existing)
        {
            if (t != parent && t.name == parent.name && t.gameObject.scene.name == "DontDestroyOnLoad")
            {
                Debug.Log($"[스폰] DontDestroyOnLoad에 동일한 {parent.name}가 이미 존재 → 현재 생성된 오브젝트 제거");
                Destroy(parent.gameObject);  // 현재 새로 생성된 Settings 제거
                return;
            }
        }

        DontDestroyOnLoad(parent.gameObject);
        Debug.Log($"[스폰] Settings({parent.name})에 DontDestroyOnLoad 적용 완료");
    }

    private IEnumerator ApplySpawn()
    {
        yield return null;

        float timer = 0f;
        while ((player == null || !player.activeInHierarchy || player.GetComponent<Rigidbody2D>() == null) && timer < 2f)
        {
            player = GameObject.FindWithTag("Player");
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (player == null && SelectedCharacterData.Instance != null)
        {
            GameObject prefab = SelectedCharacterData.Instance.selectedCharacterPrefab;
            if (prefab != null)
            {
                player = prefab;
                player.tag = "Player";
                Debug.Log("[스폰] 선택된 캐릭터 프리팹 연결 완료");
            }
        }

        if (player == null || portalDatabase == null)
        {
            Debug.LogWarning("[스폰] Player 또는 PortalDatabase 연결 실패");
            LoadingUIManager.Instance?.HideLoading();  // 🔸 여기에도 명시적 보장
            yield break;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        string lastPortalId = SceneLoadData.Instance?.LastPortalName;

        if (SceneLoadData.Instance != null && SceneLoadData.Instance.IsVillageStartRequired())
        {
            lastPortalId = "VillageStart";
            SceneLoadData.Instance.EnteredFromGameStart = false;
            Debug.Log("[게임시작] VillageScene → VillageStart 위치 사용");
        }

        var spawnData = portalDatabase.GetSpawnData(lastPortalId, currentScene);
        if (spawnData != null)
        {
            Vector3 spawnPos = spawnData.spawnPosition;

            playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                yield return new WaitForFixedUpdate();
                playerRb.MovePosition(spawnPos);
            }

            player.transform.position = spawnPos;

            if (virtualCamera != null)
            {
                if (virtualCamera.Follow != player.transform)
                {
                    virtualCamera.Follow = player.transform;
                    Debug.Log("[스폰] CinemachineCamera의 Follow 대상 설정됨");
                }

                if (virtualCamera.LookAt != player.transform)
                {
                    virtualCamera.LookAt = player.transform;
                    Debug.Log("[스폰] CinemachineCamera의 LookAt 대상도 설정됨");
                }
            }
            else
            {
                StartCoroutine(WaitUntilCameraFoundAndAssignFollow());
            }

            Debug.Log($"[스폰 완료] 위치 = {player.transform.position}, 활성 상태 = {player.activeSelf}");
        }
        else
        {
            Debug.LogWarning($"[스폰] SpawnData 매칭 실패 → PortalId: {lastPortalId}, Scene: {currentScene}");
        }

        yield return null;

        foreach (var portal in Object.FindObjectsByType<StagePortal>(FindObjectsSortMode.None))
        {
            portal.EnablePortalAfterSpawn();
            Debug.Log($"[포탈] EnablePortalAfterSpawn 호출 → {portal.portalName}");
        }

        yield return new WaitForSeconds(0.2f);
        if (SceneLoadData.Instance != null)
        {
            SceneLoadData.Instance.LastPortalName = null;
            Debug.Log("[스폰] LastPortalName 초기화 완료");
        }
    }

    private IEnumerator ResetCameraConfiner()
    {
        if (confinerResetDone) yield break;

        float timeout = 3f;
        float elapsed = 0f;
        GameObject ground = null;
        Collider2D groundCollider = null;

        while (elapsed < timeout)
        {
            ground = GameObject.FindWithTag("Ground");
            if (ground != null)
            {
                groundCollider = ground.GetComponent<Collider2D>();
                if (groundCollider != null) break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (virtualCamera != null && groundCollider != null)
        {
            var confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
            if (confiner == null)
            {
                confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner2D>();
                Debug.Log("[카메라] Confiner가 없어 자동 추가됨");
            }

            confiner.BoundingShape2D = groundCollider;
            confiner.InvalidateBoundingShapeCache();
            Debug.Log("[카메라] Confiner 재설정 완료");
            confinerResetDone = true;
        }
        else
        {
            Debug.LogWarning("[카메라] Ground 또는 Collider2D를 찾지 못해 Confiner 설정 실패");
        }
    }

    private IEnumerator WaitUntilCameraFoundAndAssignFollow()
    {
        float waitTime = 0f;
        float maxWait = 3f;

        while (virtualCamera == null && waitTime < maxWait)
        {
            var found = Object.FindFirstObjectByType<CinemachineCamera>();
            if (found != null)
            {
                virtualCamera = found;
                SceneManager.MoveGameObjectToScene(virtualCamera.gameObject, SceneManager.GetActiveScene());

                virtualCamera.Follow = player?.transform;
                virtualCamera.LookAt = player?.transform;

                StartCoroutine(ResetCameraConfiner());

                virtualCamera.gameObject.SetActive(false);
                yield return null;
                virtualCamera.gameObject.SetActive(true);

                Debug.Log("[스폰] 나중에 등장한 CinemachineCamera에 Follow 및 Confiner 재설정 완료");
                yield break;
            }

            waitTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if (virtualCamera == null)
        {
            Debug.LogWarning("[스폰] 일정 시간 내 카메라 찾기 실패 → Follow 설정 불가");
        }
    }
}