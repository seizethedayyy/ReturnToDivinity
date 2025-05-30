using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class HoverTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text targetText;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color selectedColor = Color.green;

    private bool isSelected = false;

    private void Start()
    {
        if (targetText != null)
            targetText.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected && targetText != null)
            targetText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected && targetText != null)
            targetText.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.SelectButton(this);
            ForceSelectColor();
        }
    }

    public void Deselect()
    {
        isSelected = false;
        if (targetText != null)
            targetText.color = normalColor;
    }

    public void ForceSelectColor()
    {
        isSelected = true;
        if (targetText != null)
            targetText.color = selectedColor;
    }
}