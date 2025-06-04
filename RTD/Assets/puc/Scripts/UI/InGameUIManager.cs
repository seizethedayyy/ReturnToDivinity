using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 인게임 UI를 관리합니다. 캐릭터 정보, HP/Exp/Fury 게이지, 콤보 UI, 메뉴 팝업, GameOver 패널 등을 포함합니다.
/// </summary>
public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance { get; private set; }

    [Header("UI 요소")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("메뉴 팝업 UI")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("HP UI")]
    [SerializeField] public Image hpBarFillImage;
    [SerializeField] public TextMeshProUGUI hpText;

    [Header("Exp UI")]
    [SerializeField] private Image expBarFillImage;
    [SerializeField] private TextMeshProUGUI expText;

    [Header("Gold UI")]
    [SerializeField] private TextMeshProUGUI goldText;

    [Header("Fury UI")]
    [SerializeField] private Image furyGaugeFillImage;

    [Header("Fury Skill UI")]
    [SerializeField] private Image furySkillIconImage;

    [Header("콤보 슬롯 아이콘 (잠금/해금)")]
    [SerializeField] private Image comboSlot1;
    [SerializeField] private Image comboSlot2;
    [SerializeField] private Image comboSlot3;
    [SerializeField] private Image comboSlot4;

    [Header("콤보 슬롯 Glow Objects (애니메이션)")]
    [SerializeField] private GameObject combo1Glow;
    [SerializeField] private GameObject combo2Glow;
    [SerializeField] private GameObject combo3Glow;
    [SerializeField] private GameObject combo4Glow;

    private Color originalHpColor;

    public void OpenMenu()
    {
        if (menuPanel != null)
            menuPanel.SetActive(true);
        Time.timeScale = 0f;  // 일시정지
    }

    public void CloseMenu()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);
        Time.timeScale = 1f;  // 재개
    }

    public void OnClickResume()
    {
        CloseMenu();
    }
      

    public void OnClickOption()
    {
        if (optionPanel != null)
            optionPanel.SetActive(true);
    }

    public void OnClickGoToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    public void OnClickQuitGame()
    {
        Application.Quit();
    }

    public void OnClickSaveGame()
    {
        Debug.Log("저장 기능은 추후 구현 예정입니다.");
    }

    private void Awake()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (hpBarFillImage != null)
        {
            originalHpColor = hpBarFillImage.color;
            hpBarFillImage.fillAmount = 1f;
        }
        if (hpText != null) hpText.text = "0 / 0";

        if (expBarFillImage != null) expBarFillImage.fillAmount = 0f;
        if (expText != null) expText.text = "0 / 0";

        if (furyGaugeFillImage != null) furyGaugeFillImage.fillAmount = 0f;
        if (furySkillIconImage != null) furySkillIconImage.enabled = false;

        if (portraitImage != null) portraitImage.sprite = null;
        if (nameText != null) nameText.text = "";
        if (levelText != null) levelText.text = "Lv.0";
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindUIReferences();

        
        if (gameOverPanel != null)
        {
            CanvasGroup cg = gameOverPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                gameOverPanel.SetActive(false);
            }
        }

        if (hpBarFillImage != null)
            originalHpColor = hpBarFillImage.color;

        UpdateFuryGauge(0f);

        if (expBarFillImage != null)
            expBarFillImage.fillAmount = 0f;
        if (expText != null)
            expText.text = "0 / 0";

        if (portraitImage != null) portraitImage.sprite = null;
        if (nameText != null) nameText.text = "";
        if (levelText != null) levelText.text = "";

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && playerObj.TryGetComponent<PlayerController>(out var playerCtrl))
        {
            var selectedData = SelectedCharacterData.Instance;

            if (selectedData != null && selectedData.selectedCharacter != null)
            {
                if (portraitImage != null)
                    portraitImage.sprite = selectedData.selectedCharacter.portraitSprite;

                if (nameText != null)
                    nameText.text = selectedData.selectedCharacter.characterName;

                if (furySkillIconImage != null)
                {
                    furySkillIconImage.sprite = selectedData.selectedCharacter.furySkillIcon;
                    furySkillIconImage.enabled = true;
                }
            }
            else
            {
                Debug.LogWarning("[InGameUIManager] CharacterSelectManager에서 올바른 초상화 배열을 찾을 수 없습니다.");
            }

            UpdateLevelText(playerCtrl.currentLevel);
            UpdateHpUI(playerCtrl.currentHp, playerCtrl.Data.maxHp);
            UpdateExpUI(playerCtrl.currentExp, playerCtrl.Data.exp);
            UpdateFuryGauge(playerCtrl.currentFury / playerCtrl.Data.furyMax);

            UpdateComboLockUI(
                playerCtrl.currentLevel,
                playerCtrl.combo2UnlockLevel,
                playerCtrl.combo3UnlockLevel,
                playerCtrl.combo4UnlockLevel
            );
        }
    }

    public void RebindUIReferences() { }

    public void UpdateComboLockUI(int playerLevel, int unlock2, int unlock3, int unlock4)
    {
        if (comboSlot1 != null)
        {
            comboSlot1.gameObject.SetActive(true);
            comboSlot1.color = Color.white;
        }
        if (combo1Glow != null) combo1Glow.SetActive(false);

        if (comboSlot2 != null)
        {
            if (playerLevel >= unlock2)
            {
                comboSlot2.color = Color.white;
                comboSlot2.gameObject.SetActive(true);
            }
            else
            {
                comboSlot2.color = Color.gray;
                comboSlot2.gameObject.SetActive(false);
            }
            if (combo2Glow != null) combo2Glow.SetActive(false);
        }

        if (comboSlot3 != null)
        {
            if (playerLevel >= unlock3)
            {
                comboSlot3.color = Color.white;
                comboSlot3.gameObject.SetActive(true);
            }
            else
            {
                comboSlot3.color = Color.gray;
                comboSlot3.gameObject.SetActive(false);
            }
            if (combo3Glow != null) combo3Glow.SetActive(false);
        }

        if (comboSlot4 != null)
        {
            if (playerLevel >= unlock4)
            {
                comboSlot4.color = Color.white;
                comboSlot4.gameObject.SetActive(true);
            }
            else
            {
                comboSlot4.color = Color.gray;
                comboSlot4.gameObject.SetActive(false);
            }
            if (combo4Glow != null) combo4Glow.SetActive(false);
        }
    }

    public void UpdateComboSlot(int comboIndex)
    {
        GameObject[] glows = { combo1Glow, combo2Glow, combo3Glow, combo4Glow };
        for (int i = 0; i < glows.Length; i++)
        {
            if (glows[i] != null)
                glows[i].SetActive(i < comboIndex);
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

    public void UpdateCharacterInfo(Sprite portrait, string name, int level)
    {
        if (portraitImage != null) portraitImage.sprite = portrait;
        if (nameText != null) nameText.text = name;
        if (levelText != null) levelText.text = $"Lv.{level}";
    }

    public void UpdateHpUI(int currentHp, int maxHp)
    {
        if (hpText != null)
            hpText.text = $"{currentHp} / {maxHp}";
        if (hpBarFillImage != null && maxHp > 0)
        {
            float percent = (float)currentHp / maxHp;
            hpBarFillImage.fillAmount = percent;
            hpBarFillImage.color = Color.Lerp(Color.red, originalHpColor, percent);
        }
    }

    public void UpdateExpUI(int currentExp, int requiredExp)
    {
        if (expBarFillImage != null && requiredExp > 0)
        {
            float percent = Mathf.Clamp01((float)currentExp / requiredExp);
            expBarFillImage.fillAmount = percent;
        }
        if (expText != null)
            expText.text = $"{currentExp} / {requiredExp}";
    }

    public void UpdateGoldUI(int amount)
    {
        if (goldText != null)
            goldText.text = $"{amount} G";
    }

    public void UpdateFuryGauge(float percent)
    {
        if (furyGaugeFillImage != null)
            furyGaugeFillImage.fillAmount = Mathf.Clamp01(percent);

        if (furySkillIconImage != null)
        {
            // Fury가 Max일 때 원래 색상으로, 아닐 땐 회색으로
            furySkillIconImage.color = (percent >= 1f) ? Color.white : Color.gray;
        }
    }

    public void UpdateLevelText(int level)
    {
        if (levelText != null)
            levelText.text = $"Lv.{level}";
    }

    public void OpenOption()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);  // 메뉴 패널 먼저 닫기

        if (optionPanel != null)
            optionPanel.SetActive(true); // 옵션 패널 열기
    }

    public void CloseOption()
    {
        if (optionPanel != null)
            optionPanel.SetActive(false);
    }

    

    public void ShowGameOver(float delay)
    {
        StartCoroutine(GameOverCoroutine(delay));
    }

    private IEnumerator GameOverCoroutine(float delay)
    {
        if (gameOverPanel != null)
        {
            CanvasGroup cg = gameOverPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                gameOverPanel.SetActive(true);

                // fade 시간 측정
                float fadeDuration = 1f;
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    cg.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                cg.alpha = 1f;
            }

            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene("TitleScene");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
