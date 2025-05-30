using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonColorController : MonoBehaviour, IPointerClickHandler
{
    private Button button;
    private ColorBlock originalColors;
    public Color selectedColor = Color.green;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalColors = button.colors;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ButtonSelectionManager.Instance.OnButtonClicked(this);
    }

    public void Select()
    {
        ColorBlock colors = button.colors;
        colors.normalColor = selectedColor;
        colors.highlightedColor = selectedColor;
        colors.pressedColor = selectedColor;
        colors.selectedColor = selectedColor;
        button.colors = colors;
    }

    public void Deselect()
    {
        button.colors = originalColors;
    }
}