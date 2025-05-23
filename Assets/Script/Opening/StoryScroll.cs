using UnityEngine;
using TMPro;

public class StoryScroll : MonoBehaviour
{
    public TMP_Text storyText;
    public GameObject pressAnyKeyText;

    public float scrollSpeed = 30f;
    public float fadeStartY = 300f;
    public float fadeEndY = 600f;
    public float endY = 700f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Start()
    {
        if (storyText == null)
        {
            Debug.LogError("❌ TMP_Text가 연결되지 않았습니다. Inspector에서 storyText를 연결하세요.");
            return;
        }

        rectTransform = storyText.GetComponent<RectTransform>();

        canvasGroup = storyText.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = storyText.gameObject.AddComponent<CanvasGroup>();

        if (pressAnyKeyText != null)
            pressAnyKeyText.SetActive(false); // 처음에는 꺼진 상태
    }

    void Update()
    {
        if (storyText == null || rectTransform == null || canvasGroup == null) return;

        rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);

        float y = rectTransform.anchoredPosition.y;
        if (y >= fadeStartY)
        {
            float t = Mathf.InverseLerp(fadeStartY, fadeEndY, y);
            canvasGroup.alpha = 1f - t;
        }

        if (y >= endY)
        {
            gameObject.SetActive(false); // 스토리 텍스트 자체는 비활성화
            if (pressAnyKeyText != null)
            {
                pressAnyKeyText.SetActive(true); // ✅ 여기서 나타남!
                Debug.Log("✅ 'Press Any Key' 텍스트 표시됨");
            }
        }
    }
}