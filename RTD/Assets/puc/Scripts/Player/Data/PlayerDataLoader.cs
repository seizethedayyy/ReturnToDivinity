// -------------------------------------------------------
// ���� ���: Assets/puc/Scripts/Player/PlayerDataLoader.cs
// -------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using GameData;  // PlayerData / PlayerDataRoot�� ���ǵ� ���ӽ����̽�

public class PlayerDataLoader : MonoBehaviour
{
    // �ε�� ��� �÷��̾� ������ ��� (Json �� List<PlayerData> ����)
    public static List<PlayerData> LoadedPlayerData { get; private set; }

    [Tooltip("Google Sheets �� Apps Script�� ��ȯ�ϴ� JSON URL")]
    public string jsonUrl;

    private static PlayerDataLoader instance;

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
        // GET ��û���� JSON�� ������
        using (var www = UnityEngine.Networking.UnityWebRequest.Get(jsonUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("[PlayerDataLoader] ������ �ε� ����: " + www.error);
                yield break;
            }

            // JSON �Ľ� (Newtonsoft.Json ���)
            PlayerDataRoot wrapper = JsonConvert.DeserializeObject<PlayerDataRoot>(www.downloadHandler.text);
            LoadedPlayerData = wrapper.data;

            // �ε� ��� �α�
            if (LoadedPlayerData == null || LoadedPlayerData.Count == 0)
            {
                Debug.LogError("[PlayerDataLoader] LoadedPlayerData�� ��� �ֽ��ϴ�. JSON ������ Ȯ���ϼ���.");
            }
            else
            {
                Debug.Log($"[PlayerDataLoader] �ε� �Ϸ�: �� {LoadedPlayerData.Count}�� ������");
                foreach (var pd in LoadedPlayerData)
                {
                    Debug.Log($"�� Loaded ID = {pd.id}");
                }
            }
        }
    }

    /// <summary>
    /// ID�� �ش��ϴ� PlayerData ��ü�� ��ȯ�մϴ�.  
    /// LoadedPlayerData�� �غ���� �ʾҰų�, ��ġ�ϴ� ID�� ������ null�� ��ȯ�մϴ�.
    /// </summary>
    public static PlayerData GetPlayerDataById(string id)
    {
        if (LoadedPlayerData == null) return null;
        return LoadedPlayerData.Find(p => p.id == id);
    }
}
