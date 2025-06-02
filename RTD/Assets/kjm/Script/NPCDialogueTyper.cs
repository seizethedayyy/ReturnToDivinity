using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCDialogueTyper : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject balloonUI;
    public TextMeshProUGUI dialogueText;

    [Header("대사")]
    [TextArea(3, 10)]
    public string dialogue = "이곳은 무기 상점이라네!\r\n새로운 무기와\r\n함께할 준비가 됐는가?";
    public float typingSpeed = 0.05f;

    [Header("말풍선 위치 오프셋")]
    public Vector2 offset = new Vector2(550, 40); // ← 인스펙터에서 조정 가능

    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Canvas canvas;

    private void Start()
    {
        balloonUI.SetActive(false);
        dialogueText.text = "";

        canvas = balloonUI.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("balloonUI의 부모에 Canvas가 없습니다!");
        }
    }

    private void OnMouseDown()
    {
        if (!isTyping && !balloonUI.activeSelf)
        {
            balloonUI.SetActive(true);

            RectTransform balloonRect = balloonUI.GetComponent<RectTransform>();
            Vector3 worldPos = transform.position + new Vector3(0, 0.5f, 0);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            RectTransform canvasRect = balloonRect.parent as RectTransform;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                null,
                out localPoint);

            balloonRect.anchoredPosition = localPoint + offset;

            typingCoroutine = StartCoroutine(TypeDialogue());
        }
    }

    private IEnumerator TypeDialogue()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in dialogue)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void OnBalloonClick()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = dialogue;
            isTyping = false;
        }
        else
        {
            balloonUI.SetActive(false);
            dialogueText.text = "";
        }
    }
}
