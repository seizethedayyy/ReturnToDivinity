using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CursorBlinkOnClick : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotspot = Vector2.zero;
    [SerializeField] private float blinkDuration = 0.1f;

    private static CursorBlinkOnClick instance;
    private bool isBlinking = false;

    private void Awake()
    {
        // �̱���ó�� �ߺ� ���� + ����
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        Cursor.visible = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isBlinking)
        {
            StartCoroutine(BlinkCursor());
        }
    }

    private IEnumerator BlinkCursor()
    {
        isBlinking = true;
        Cursor.visible = false;
        yield return new WaitForSeconds(blinkDuration);
        Cursor.visible = true;
        isBlinking = false;
    }
}
