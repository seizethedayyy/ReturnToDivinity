using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class CharacterInfoUI : MonoBehaviour
{
    [Header("탭 패널들")]
    public GameObject panelBasicInfo;
    public GameObject panelSkills;
    public GameObject panelAwakenedSkills;

    [Header("기본 선택될 버튼")]
    public HoverTextColor defaultTabButton;

    [Header("캐릭터 기본 정보 UI")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;

    [Header("캐릭터 정보창 내 스킬 아이콘")]
    public Image skillQIconInInfoPanel;
    public Image skillEIconInInfoPanel;
    public Image skillRIconInInfoPanel;

    [Header("스킬 설명 영역")]
    public TextMeshProUGUI skillDescriptionText;

    [Header("패시브 스킬 정보")]
    public TextMeshProUGUI passiveSkillNameText;
    public TextMeshProUGUI passiveSkillDescriptionText;
    public TextMeshProUGUI passiveSkillLevelText;
    public Image passiveSkillIconImage;

    private void OnEnable()
    {
        ShowBasicInfo();
        ApplyCharacterInfo();
        StartCoroutine(DelayedSelectDefaultTab());
    }

    private IEnumerator DelayedSelectDefaultTab()
    {
        yield return null;
        if (defaultTabButton != null && SelectionManager.Instance != null)
        {
            SelectionManager.Instance.ForceSelectDefault(defaultTabButton);
        }
    }

    private void ApplyCharacterInfo()
    {
        var info = SelectedCharacterData.Instance?.selectedCharacter;
        if (info == null)
        {
            Debug.LogWarning("[CharacterInfoUI] 선택된 캐릭터 정보가 없습니다.");
            return;
        }

        if (portraitImage != null) portraitImage.sprite = info.portraitSprite;
        if (nameText != null) nameText.text = info.characterName;
        if (levelText != null) levelText.text = "Lv.1";

        if (passiveSkillNameText != null) passiveSkillNameText.text = info.passiveSkillName;
        if (passiveSkillDescriptionText != null) passiveSkillDescriptionText.text = info.passiveSkillDescription;
        if (passiveSkillLevelText != null) passiveSkillLevelText.text = $"Lv. {info.passiveSkillLevel}";

        if (passiveSkillIconImage != null) passiveSkillIconImage.sprite = info.passiveSkillIcon;

        Debug.Log($"[CharacterInfoUI] 패시브 스킬: {info.passiveSkillName} / Lv.{info.passiveSkillLevel}");
    }

    public void ShowBasicInfo()
    {
        panelBasicInfo.SetActive(true);
        panelSkills.SetActive(false);
        panelAwakenedSkills.SetActive(false);
    }

    public void ShowSkills()
    {
        Debug.Log("[CharacterInfoUI] ShowSkills() 호출됨");

        panelBasicInfo.SetActive(false);
        panelSkills.SetActive(true);
        panelAwakenedSkills.SetActive(false);

        var info = SelectedCharacterData.Instance?.selectedCharacter;
        if (info == null) return;

        if (skillQIconInInfoPanel != null) skillQIconInInfoPanel.sprite = info.skillQIcon;
        if (skillEIconInInfoPanel != null) skillEIconInInfoPanel.sprite = info.skillEIcon;
        if (skillRIconInInfoPanel != null) skillRIconInInfoPanel.sprite = info.skillRIcon;

        if (skillDescriptionText != null) skillDescriptionText.text = info.skillQDescription;
    }

    public void ShowAwakenedSkills()
    {
        panelBasicInfo.SetActive(false);
        panelSkills.SetActive(false);
        panelAwakenedSkills.SetActive(true);
    }

    public void OnClickSkillQ()
    {
        var info = SelectedCharacterData.Instance?.selectedCharacter;
        if (info != null && skillDescriptionText != null)
            skillDescriptionText.text = info.skillQDescription;
    }

    public void OnClickSkillE()
    {
        var info = SelectedCharacterData.Instance?.selectedCharacter;
        if (info != null && skillDescriptionText != null)
            skillDescriptionText.text = info.skillEDescription;
    }

    public void OnClickSkillR()
    {
        var info = SelectedCharacterData.Instance?.selectedCharacter;
        if (info != null && skillDescriptionText != null)
            skillDescriptionText.text = info.skillRDescription;
    }
}