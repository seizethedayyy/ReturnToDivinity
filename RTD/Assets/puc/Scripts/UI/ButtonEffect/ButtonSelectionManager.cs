using UnityEngine;
using TMPro;

public class ButtonSelectionManager : MonoBehaviour
{
    public static ButtonSelectionManager Instance { get; private set; }

    private ButtonColorController selectedButton;

    [Header("Confirm 텍스트 초기화 대상")]
    public HoverTextColor confirmHover;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void OnButtonClicked(ButtonColorController button)
    {
        if (selectedButton != null && selectedButton != button)
        {
            selectedButton.Deselect();
        }

        selectedButton = button;
        selectedButton.Select();

        // ✅ Confirm 버튼 텍스트 초기화 및 Hover 상태 복원
        if (confirmHover != null)
        {
            confirmHover.Deselect(); // normalColor로 복원 + isSelected 해제 → Hover 재작동
            Debug.Log("[ButtonSelectionManager] Confirm 버튼 Hover 상태 초기화 완료");
        }
        else
        {
            Debug.LogWarning("[ButtonSelectionManager] confirmHover가 연결되지 않았습니다.");
        }
    }

    public void DeselectAll()
    {
        if (selectedButton != null)
        {
            selectedButton.Deselect();
            selectedButton = null;
        }
    }
}