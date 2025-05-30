using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinimapController : MonoBehaviour
{
    [Header("Minimap UI")]
    public RectTransform minimapRect;
    public RectTransform playerIcon;

    [Header("Portal")]
    public GameObject portalIconPrefab;
    public PortalDatabase portalDatabase;

    [Header("NPC")]
    public GameObject npcIconPrefab;

    private Transform player;
    private float mapRadiusWorld = 8f;
    private float mapUIRadius;

    private List<PortalSpawnData> portalData = new();
    private List<GameObject> portalIcons = new();

    private List<Transform> npcList = new();
    private List<GameObject> npcIcons = new();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    IEnumerator Start()
    {
        yield return new WaitUntil(() => GameObject.FindWithTag("Player") != null);
        player = GameObject.FindWithTag("Player").transform;

        mapUIRadius = minimapRect.sizeDelta.x / 2f;

        ReloadAll();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReloadAll();
    }

    void ReloadAll()
    {
        LoadPortalData();
        ClearPortalIcons();
        CreatePortalIcons();

        LoadNpcData();
    }

    void LoadPortalData()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        portalData = portalDatabase.spawnPoints.FindAll(p => p.sceneName == currentScene);
    }

    void ClearPortalIcons()
    {
        foreach (var icon in portalIcons)
        {
            if (icon != null) Destroy(icon);
        }
        portalIcons.Clear();
    }

    void CreatePortalIcons()
    {
        foreach (var portal in portalData)
        {
            GameObject icon = Instantiate(portalIconPrefab, minimapRect);
            icon.SetActive(true);
            portalIcons.Add(icon);
        }
    }

    void LoadNpcData()
    {
        foreach (var icon in npcIcons)
        {
            if (icon != null) Destroy(icon);
        }
        npcList.Clear();
        npcIcons.Clear();

        if (npcIconPrefab == null)
        {
            Debug.LogWarning("⚠ npcIconPrefab is not assigned.");
            return;
        }

        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");

        foreach (GameObject npc in npcs)
        {
            if (npc == null) continue;

            npcList.Add(npc.transform);

            GameObject icon = Instantiate(npcIconPrefab, minimapRect);
            icon.SetActive(true);
            npcIcons.Add(icon);
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // ✅ 1. 플레이어는 중앙 고정
        playerIcon.anchoredPosition = Vector2.zero;
        playerIcon.localRotation = Quaternion.Euler(0, 0, -player.eulerAngles.y);

        // ✅ 2. 포탈 아이콘 위치 갱신
        for (int i = 0; i < portalData.Count; i++)
        {
            var data = portalData[i];

            if (data.portalId == "VillageStart")
            {
                portalIcons[i].SetActive(false);
                continue;
            }

            Vector2 offset = data.spawnPosition - player.position;

            if (offset.magnitude > mapRadiusWorld)
            {
                portalIcons[i].SetActive(false);
                continue;
            }

            Vector2 iconPos = (offset / mapRadiusWorld) * mapUIRadius;
            portalIcons[i].SetActive(true);
            portalIcons[i].GetComponent<RectTransform>().anchoredPosition = iconPos;
        }

        // ✅ 3. NPC 아이콘 위치 갱신
        int count = Mathf.Min(npcList.Count, npcIcons.Count);
        for (int i = 0; i < count; i++)
        {
            if (npcList[i] == null || npcIcons[i] == null) continue;

            Vector2 offset = npcList[i].position - player.position;

            if (offset.magnitude > mapRadiusWorld)
            {
                npcIcons[i].SetActive(false);
                continue;
            }

            Vector2 iconPos = (offset / mapRadiusWorld) * mapUIRadius;
            npcIcons[i].SetActive(true);
            npcIcons[i].GetComponent<RectTransform>().anchoredPosition = iconPos;
        }
    }
}