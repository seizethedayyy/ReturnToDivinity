using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float lifetime = 1f;
    public Vector3 moveOffset = new Vector3(0, 1.5f, 0);
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetText(string value)
    {
        if (text != null)
            text.text = value;
    }

    private void Start()
    {
        transform.localPosition += moveOffset;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
    }
}