using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance { get; private set; }

    [Header("옵션 UI 오브젝트")]
    public GameObject optionUIPopup;

    private void Start()
    {
        RebindOptionUI(); // 이미 OptionManager.cs에 정의된 함수일 가능성 높음
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 필요 시 활성화
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RebindOptionUI();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindOptionUI();
    }

    /// <summary>
    /// OptionUI를 다시 찾고, Canvas 카메라와 위치를 재설정한다
    /// </summary>
    private void RebindOptionUI()
    {
        if (optionUIPopup == null)
        {
            Debug.Log("[OptionManager] OptionUI 재탐색 실행");

            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.CompareTag("OptionUI") && obj.hideFlags == HideFlags.None)
                {
                    optionUIPopup = obj;
                    Debug.Log("[OptionManager] OptionUI 재연결 성공: " + obj.name);
                    break;
                }
            }

            if (optionUIPopup == null)
            {
                Debug.LogError("[OptionManager] OptionUI를 찾지 못했습니다. 태그 확인 필요.");
                return;
            }
        }

        // 1. Canvas가 카메라를 잃었을 경우 재연결
        Canvas canvas = optionUIPopup.GetComponentInChildren<Canvas>(true);
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvas.worldCamera = Camera.main;
            Debug.Log("[OptionManager] Canvas의 worldCamera 재연결");
        }

        // 2. 위치와 스케일 보정
        RectTransform rect = optionUIPopup.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// 옵션 UI 팝업 열기
    /// </summary>
    public void ShowOption()
    {
        if (optionUIPopup != null)
        {
            optionUIPopup.SetActive(true);
            Debug.Log("[OptionManager] 옵션 팝업 활성화");
        }
        else
        {
            Debug.LogError("[OptionManager] optionUIPopup이 null입니다. ShowOption 실패");
        }
    }

    /// <summary>
    /// 옵션 UI 팝업 닫기
    /// </summary>
    public void HideOption()
    {
        if (optionUIPopup != null)
        {
            optionUIPopup.SetActive(false);
        }
    }
}