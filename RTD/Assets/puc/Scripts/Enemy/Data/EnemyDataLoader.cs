using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class EnemyDataLoader : MonoBehaviour
{
    public static List<EnemyData> LoadedEnemyData { get; private set; }
    public string jsonUrl;

    private static EnemyDataLoader instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LoadData());
    }

    private IEnumerator LoadData()
    {
        using (var www = new UnityEngine.Networking.UnityWebRequest(jsonUrl))
        {
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("[EnemyDataLoader] 데이터 로딩 실패: " + www.error);
                yield break;
            }

            var wrapper = JsonConvert.DeserializeObject<EnemyDataWrapper>(www.downloadHandler.text);
            LoadedEnemyData = wrapper.data;
        }
    }

    public static EnemyData GetEnemyDataById(string id)
    {
        return LoadedEnemyData?.Find(e => e.id == id);
    }
}

[System.Serializable]
public class EnemyDataWrapper
{
    public List<EnemyData> data;
}
