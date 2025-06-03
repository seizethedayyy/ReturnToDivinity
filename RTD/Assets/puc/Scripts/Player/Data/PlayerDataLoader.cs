// -------------------------------------------------------
// 파일 경로: Assets/puc/Scripts/Player/PlayerDataLoader.cs
// -------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using GameData;  // PlayerData / PlayerDataRoot가 정의된 네임스페이스

public class PlayerDataLoader : MonoBehaviour
{
    // 로드된 모든 플레이어 데이터 목록 (Json → List<PlayerData> 형태)
    public static List<PlayerData> LoadedPlayerData { get; private set; }

    [Tooltip("Google Sheets → Apps Script가 반환하는 JSON URL")]
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
        // GET 요청으로 JSON을 가져옴
        using (var www = UnityEngine.Networking.UnityWebRequest.Get(jsonUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("[PlayerDataLoader] 데이터 로딩 실패: " + www.error);
                yield break;
            }

            // JSON 파싱 (Newtonsoft.Json 사용)
            PlayerDataRoot wrapper = JsonConvert.DeserializeObject<PlayerDataRoot>(www.downloadHandler.text);
            LoadedPlayerData = wrapper.data;

            // 로드 결과 로그
            if (LoadedPlayerData == null || LoadedPlayerData.Count == 0)
            {
                Debug.LogError("[PlayerDataLoader] LoadedPlayerData가 비어 있습니다. JSON 구조를 확인하세요.");
            }
            else
            {
                Debug.Log($"[PlayerDataLoader] 로드 완료: 총 {LoadedPlayerData.Count}개 아이템");
                foreach (var pd in LoadedPlayerData)
                {
                    Debug.Log($"→ Loaded ID = {pd.id}");
                }
            }
        }
    }

    /// <summary>
    /// ID에 해당하는 PlayerData 객체를 반환합니다.  
    /// LoadedPlayerData가 준비되지 않았거나, 일치하는 ID가 없으면 null을 반환합니다.
    /// </summary>
    public static PlayerData GetPlayerDataById(string id)
    {
        if (LoadedPlayerData == null) return null;
        return LoadedPlayerData.Find(p => p.id == id);
    }
}
