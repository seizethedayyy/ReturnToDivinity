using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PortalDatabase", menuName = "Spawn/Portal Database")]
public class PortalDatabase : ScriptableObject
{
    public List<PortalSpawnData> spawnPoints = new();

    public PortalSpawnData GetSpawnData(string portalId, string sceneName)
    {
        if (string.IsNullOrEmpty(portalId) || string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[PortalDatabase] portalId 또는 sceneName이 비어 있습니다.");
            return null;
        }

        portalId = portalId.Trim();
        sceneName = sceneName.Trim();

        var result = spawnPoints.FirstOrDefault(p =>
            p != null &&
            p.portalId.Trim() == portalId &&
            p.sceneName.Trim() == sceneName
        );

        return result;
    }
}