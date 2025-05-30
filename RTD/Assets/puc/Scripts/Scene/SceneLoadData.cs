using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadData : MonoBehaviour
{
    public static SceneLoadData Instance;

    [Header("마지막 포탈 이름 (포탈 이동 시 설정됨)")]
    public string LastPortalName;

    [Header("VillageScene이 게임 시작으로부터 진입했는지 여부")]
    public bool EnteredFromGameStart = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }

    /// <summary>
    /// VillageScene이 "게임 시작에서 직접 진입"했는지를 판단합니다.
    /// </summary>
    public bool IsVillageStartRequired()
    {
        var scene = SceneManager.GetActiveScene();
        return scene.name == "VillageScene" && EnteredFromGameStart;
    }
}