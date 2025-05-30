using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    private HoverTextColor selectedButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                DeselectCurrent();
            }
        }
    }

    public void SelectButton(HoverTextColor button)
    {
        if (selectedButton != null && selectedButton != button)
        {
            selectedButton.Deselect();
        }

        selectedButton = button;
    }

    public void DeselectCurrent()
    {
        if (selectedButton != null)
        {
            selectedButton.Deselect();
            selectedButton = null;
        }
    }

    public void ForceSelectDefault(HoverTextColor button)
    {
        selectedButton = button;
        button.ForceSelectColor();
    }
}