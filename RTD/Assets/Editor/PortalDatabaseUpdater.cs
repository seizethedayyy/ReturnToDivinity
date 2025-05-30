using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Linq;

public class PortalDatabaseUpdater : EditorWindow
{
    private PortalDatabase portalDatabase;

    [MenuItem("Tools/Portal Database Updater")]
    public static void ShowWindow()
    {
        GetWindow<PortalDatabaseUpdater>("Portal Database Updater");
    }

    private void OnGUI()
    {
        GUILayout.Label("포탈 위치 자동 등록기", EditorStyles.boldLabel);
        portalDatabase = (PortalDatabase)EditorGUILayout.ObjectField("Portal Database", portalDatabase, typeof(PortalDatabase), false);

        if (GUILayout.Button("현재 씬의 StagePortal 정보 반영") && portalDatabase != null)
        {
            UpdatePortalDatabase();
        }
    }

    private void UpdatePortalDatabase()
    {
        var portals = Object.FindObjectsByType<StagePortal>(FindObjectsSortMode.None);
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        int updateCount = 0;
        foreach (var portal in portals)
        {
            if (portal.spawnPoint == null)
            {
                Debug.LogWarning($"[PortalUpdater] {portal.portalName} 의 spawnPoint가 비어있습니다.");
                continue;
            }

            var existing = portalDatabase.spawnPoints.FirstOrDefault(d =>
                d.portalId.Trim() == portal.portalName.Trim() &&
                d.sceneName.Trim() == sceneName.Trim()
            );

            if (existing != null)
            {
                existing.spawnPosition = portal.spawnPoint.position;
                Debug.Log($"[갱신] {portal.portalName} @ {sceneName} → {portal.spawnPoint.position}");
            }
            else
            {
                portalDatabase.spawnPoints.Add(new PortalSpawnData
                {
                    portalId = portal.portalName,
                    sceneName = sceneName,
                    spawnPosition = portal.spawnPoint.position
                });
                Debug.Log($"[추가] {portal.portalName} @ {sceneName} → {portal.spawnPoint.position}");
            }

            updateCount++;
        }

        EditorUtility.SetDirty(portalDatabase);
        AssetDatabase.SaveAssets();

        Debug.Log($"[PortalUpdater] 총 {updateCount}개의 포탈 데이터를 반영하였습니다.");
    }
}