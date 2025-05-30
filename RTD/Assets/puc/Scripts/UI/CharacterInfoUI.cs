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

    [Header("퓨리 스킬 UI")]
    public Image furySkillIconImage;
    public TextMeshProUGUI furySkillNameText;
    public TextMeshProUGUI furySkillDescriptionText;
    
    public Sprite furySkillIcon;
    public string furySkillName;
    public string furySkillDescription;

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
        if (info == null) return;

        portraitImage.sprite = info.portraitSprite;
        nameText.text = info.characterName;
        levelText.text = "Lv.1";

        furySkillIconImage.sprite = info.furySkillIcon;
        furySkillNameText.text = info.furySkillName;
        furySkillDescriptionText.text = info.furySkillDescription;
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

        ApplyCharacterInfo(); // 스킬 정보는 한 곳에서 처리
    }

    public void ShowAwakenedSkills()
    {
        panelBasicInfo.SetActive(false);
        panelSkills.SetActive(false);
        panelAwakenedSkills.SetActive(true);
    }
}
