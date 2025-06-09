using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCDialogueTyper : MonoBehaviour
{
    [Header("UI ����")]
    public GameObject balloonUI;
    public TextMeshProUGUI dialogueText;

    [Header("���")]
    [TextArea(3, 10)]
    public string dialogue = "�̰��� ���� �����̶��!\r\n���ο� �����\r\n�Բ��� �غ� �ƴ°�?";
    public float typingSpeed = 0.05f;

    [Header("��ǳ�� ��ġ ������")]
    public Vector2 offset = new Vector2(550, 40); // �� �ν����Ϳ��� ���� ����

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
            Debug.LogError("balloonUI�� �θ� Canvas�� �����ϴ�!");
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
