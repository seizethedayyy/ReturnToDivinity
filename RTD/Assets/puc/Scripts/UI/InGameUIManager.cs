using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance { get; private set; }

    [Header("UI 요소")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;

    [Header("메뉴 팝업 UI")]
    public GameObject menuUI;
    public GameObject optionUI;

    [Header("캐릭터 정보 팝업")]
    public GameObject characterInfoUI;

    [Header("체력 UI")]
    public Image hpBarFillImage;

    [Header("체력 숫자 UI")]
    public TextMeshProUGUI hpText;

    [Header("콤보 슬롯 Glow")]
    public GameObject combo1Glow;
    public GameObject combo2Glow;
    public GameObject combo3Glow;
    public GameObject combo4Glow;

    [Header("Fury UI")]
    public Image comboSlot1;
    public Image comboSlot2;
    public Image comboSlot3;
    public Image comboSlot4;
    public Image furySkillIcon;
    public GameObject furySkillLockOverlay;
    public Image furyGaugeFillImage;

    private Image[] comboSlots;
    private int comboLevelUnlocked = 1;
    private int currentComboStep = 0;

    private Coroutine hpFlashCoroutine;
    private Coroutine hpBarCoroutine;
    private Color originalHpColor = Color.green;

       

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindUIReferences();

        if (SelectedCharacterData.Instance != null)
        {
            var info = SelectedCharacterData.Instance.selectedCharacter;
            ApplyCharacterInfo(info);
        }

        if (hpBarFillImage != null)
            originalHpColor = hpBarFillImage.color;

        // ✅ Fury 게이지를 0으로 명확히 초기화
        UpdateFuryGauge(0f);
    }

    public void RebindUIReferences()
    {
        if (menuUI == null)
            menuUI = GameObject.Find("MenuUI");

        if (optionUI == null)
            optionUI = GameObject.Find("OptionUI");

        if (characterInfoUI == null)
            characterInfoUI = GameObject.Find("CharacterInfoUI");

        if (portraitImage == null)
            portraitImage = GameObject.Find("Portrait")?.GetComponent<Image>();

        if (nameText == null)
            nameText = GameObject.Find("NameText")?.GetComponent<TextMeshProUGUI>();

        if (levelText == null)
            levelText = GameObject.Find("LevelText")?.GetComponent<TextMeshProUGUI>();

        if (hpBarFillImage == null)
            hpBarFillImage = GameObject.Find("HpBarFill")?.GetComponent<Image>();

        if (hpText == null)
            hpText = GameObject.Find("HpText")?.GetComponent<TextMeshProUGUI>();

        if (comboSlot1 == null)
            comboSlot1 = GameObject.Find("ComboSlot1")?.GetComponent<Image>();
        if (comboSlot2 == null)
            comboSlot2 = GameObject.Find("ComboSlot2")?.GetComponent<Image>();
        if (comboSlot3 == null)
            comboSlot3 = GameObject.Find("ComboSlot3")?.GetComponent<Image>();
        if (comboSlot4 == null)
            comboSlot4 = GameObject.Find("ComboSlot4")?.GetComponent<Image>();
        if (furySkillIcon == null)
            furySkillIcon = GameObject.Find("FurySkillIcon")?.GetComponent<Image>();
        if (furyGaugeFillImage == null)
            furyGaugeFillImage = GameObject.Find("FuryGaugeFill")?.GetComponent<Image>();
    }

    public void ApplyCharacterInfo(CharacterInfo info)
    {
        if (portraitImage != null)
            portraitImage.sprite = info.portraitSprite;
        if (levelText != null)
            levelText.text = "Lv.1";
        if (nameText != null)
            nameText.text = info.characterName;
        if (furySkillIcon != null && info.furySkillIcon != null)
            furySkillIcon.sprite = info.furySkillIcon;
    }

    public void ApplyCharacterStats(CharacterStats stats)
    {
        if (furySkillIcon != null && stats.furySkillIcon != null)
            furySkillIcon.sprite = stats.furySkillIcon;
    }

    public void ShowMenuPopup()
    {
        if (menuUI != null)
            menuUI.SetActive(true);
        else
            Debug.LogWarning("[InGameUIManager] menuUI가 연결되지 않았습니다.");
    }

    public void ResumeGame()
    {
        if (menuUI != null)
            menuUI.SetActive(false);
    }

    public void OpenOptionPopup()
    {
        if (optionUI != null)
        {
            optionUI.SetActive(true);
            menuUI.SetActive(false);
        }
    }

    public void ShowCharacterInfoPopup()
    {
        if (characterInfoUI != null)
            characterInfoUI.SetActive(true);
        else
            Debug.LogWarning("[InGameUIManager] characterInfoUI가 연결되지 않았습니다.");
    }

    public void CloseCharacterInfoPopup()
    {
        if (characterInfoUI != null)
            characterInfoUI.SetActive(false);
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void UpdateHpUI(int currentHp, int maxHp)
    {
        if (hpBarFillImage == null)
        {
            Debug.LogWarning("[UI] hpBarFillImage가 null입니다. Fill 오브젝트 연결 필요.");
            return;
        }

        float target = Mathf.Clamp01((float)currentHp / maxHp);

        if (hpBarCoroutine != null)
            StopCoroutine(hpBarCoroutine);
        hpBarCoroutine = StartCoroutine(AnimateHpBar(target));

        if (hpText != null)
            hpText.text = $"{currentHp} / {maxHp}";

        if (hpFlashCoroutine != null)
            StopCoroutine(hpFlashCoroutine);
        hpFlashCoroutine = StartCoroutine(FlashHpBarFade());
    }

    private IEnumerator AnimateHpBar(float target)
    {
        float current = hpBarFillImage.fillAmount;

        while (Mathf.Abs(current - target) > 0.001f)
        {
            current = Mathf.Lerp(current, target, 0.2f);
            hpBarFillImage.fillAmount = current;
            yield return null;
        }

        hpBarFillImage.fillAmount = target;
    }

    private IEnumerator FlashHpBarFade()
    {
        Color fadedColor = originalHpColor;
        fadedColor.a = 0.3f;
        hpBarFillImage.color = fadedColor;

        yield return new WaitForSeconds(0.1f);

        hpBarFillImage.color = originalHpColor;
    }

    // 🔥 Fury 관련 메서드 추가
    public void UpdateComboSlot(int comboIndex)
    {
        GameObject[] glows = { combo1Glow, combo2Glow, combo3Glow, combo4Glow };

        for (int i = 0; i < glows.Length; i++)
        {
            if (glows[i] != null)
                glows[i].SetActive(i < comboIndex); // 현재 콤보까지만 점등
        }
    }

    public void ResetComboSlot()
    {
        GameObject[] glows = { combo1Glow, combo2Glow, combo3Glow, combo4Glow };

        foreach (var glow in glows)
        {
            if (glow != null)
                glow.SetActive(false);
        }
    }

    public void UpdateFuryGauge(float percent)
    {
        if (furyGaugeFillImage != null)
            furyGaugeFillImage.fillAmount = Mathf.Clamp01(percent);

        bool isReady = percent >= 1.0f;

        if (furySkillIcon != null)
            furySkillIcon.color = isReady ? Color.white : new Color(1f, 1f, 1f, 0.5f);

        if (furySkillLockOverlay != null)
            furySkillLockOverlay.SetActive(!isReady);
    }

    public void OnClickFurySkill()
    {
        if (furyGaugeFillImage == null || furyGaugeFillImage.fillAmount < 1f)
        {
            Debug.Log("[UI] Fury 게이지가 가득 차지 않아 스킬 사용 불가");
            return;
        }

        Debug.Log("[UI] Fury Skill Activated!");

        // 실제 스킬 발동 로직은 외부에서 호출
        UpdateFuryGauge(0f);
        UpdateComboSlot(0);
    }
}
