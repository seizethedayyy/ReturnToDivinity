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
        GUILayout.Label("��Ż ��ġ �ڵ� ��ϱ�", EditorStyles.boldLabel);
        portalDatabase = (PortalDatabase)EditorGUILayout.ObjectField("Portal Database", portalDatabase, typeof(PortalDatabase), false);

        if (GUILayout.Button("���� ���� StagePortal ���� �ݿ�") && portalDatabase != null)
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
                Debug.LogWarning($"[PortalUpdater] {portal.portalName} �� spawnPoint�� ����ֽ��ϴ�.");
                continue;
            }

            var existing = portalDatabase.spawnPoints.FirstOrDefault(d =>
                d.portalId.Trim() == portal.portalName.Trim() &&
                d.sceneName.Trim() == sceneName.Trim()
            );

            if (existing != null)
            {
                existing.spawnPosition = portal.spawnPoint.position;
                Debug.Log($"[����] {portal.portalName} @ {sceneName} �� {portal.spawnPoint.position}");
            }
            else
            {
                portalDatabase.spawnPoints.Add(new PortalSpawnData
                {
                    portalId = portal.portalName,
                    sceneName = sceneName,
                    spawnPosition = portal.spawnPoint.position
                });
                Debug.Log($"[�߰�] {portal.portalName} @ {sceneName} �� {portal.spawnPoint.position}");
            }

            updateCount++;
        }

        EditorUtility.SetDirty(portalDatabase);
        AssetDatabase.SaveAssets();

        Debug.Log($"[PortalUpdater] �� {updateCount}���� ��Ż �����͸� �ݿ��Ͽ����ϴ�.");
    }
}