using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;
    public Color hoverColor = Color.red;
    private Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalColor = image.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.color = originalColor;
    }
}