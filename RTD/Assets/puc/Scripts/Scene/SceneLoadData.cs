using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadData : MonoBehaviour
{
    public static SceneLoadData Instance;

    [Header("������ ��Ż �̸� (��Ż �̵� �� ������)")]
    public string LastPortalName;

    [Header("VillageScene�� ���� �������κ��� �����ߴ��� ����")]
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
            Destroy(gameObject); // �ߺ� ����
        }
    }

    /// <summary>
    /// VillageScene�� "���� ���ۿ��� ���� ����"�ߴ����� �Ǵ��մϴ�.
    /// </summary>
    public bool IsVillageStartRequired()
    {
        var scene = SceneManager.GetActiveScene();
        return scene.name == "VillageScene" && EnteredFromGameStart;
    }
}