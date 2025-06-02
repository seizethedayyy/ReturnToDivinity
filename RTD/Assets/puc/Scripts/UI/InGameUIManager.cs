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
    [SerializeField] private Image portraitImage;             // 캐릭터 초상화 Image (Inspector에 연결)
    [SerializeField] private TextMeshProUGUI nameText;        // 캐릭터 이름 Text (Inspector에 연결)
    [SerializeField] private TextMeshProUGUI levelText;       // 레벨 표시 Text (Inspector에 연결)

    [Header("메뉴 팝업 UI")]
    [SerializeField] private GameObject settingsPanel;        // 설정 패널 (Inspector에 연결)
    [SerializeField] private GameObject inventoryPanel;       // 인벤토리 패널 (Inspector에 연결)
    [SerializeField] private GameObject gameOverPanel;        // GameOver 패널 (Inspector에 연결)

    [Header("HP UI")]
    [SerializeField] public Image hpBarFillImage;            // HP 바 Image (Filled 타입) (Inspector에 연결)
    [SerializeField] public TextMeshProUGUI hpText;          // HP 텍스트 (Inspector에 연결)

    [Header("Exp UI")]
    [SerializeField] private Image expBarFillImage;           // Exp 바 Image (Filled 타입) (Inspector에 연결)
    [SerializeField] private TextMeshProUGUI expText;         // Exp 텍스트 (Inspector에 연결)

    [Header("Fury UI")]
    [SerializeField] private Image furyGaugeFillImage;        // Fury 게이지 Image (Filled 타입) (Inspector에 연결)

    private Color originalHpColor;

    private void Awake()
    {
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

        // ─── 기본 UI 초기화 ────────────────────────
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
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

        if (portraitImage != null) portraitImage.sprite = null;
        if (nameText != null) nameText.text = "";
        if (levelText != null) levelText.text = "Lv.0";
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindUIReferences();

        // ─── 씬 로드 시 UI 초기화 ─────────────────
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
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

        // 캐릭터 정보(UI) 재초기화
        if (portraitImage != null) portraitImage.sprite = null;
        if (nameText != null) nameText.text = "";
        if (levelText != null) levelText.text = "";

        // ─── PlayerController를 찾아 UI 초기값 설정 ──────────────────────
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && playerObj.TryGetComponent<PlayerController>(out var playerCtrl))
        {
            // Unity 6 에서는 제네릭 FindObjectOfType 대신 비제네릭을 사용해야 합니다.
            var selectedData = SelectedCharacterData.Instance;

            int charIndex = (int)playerCtrl.CurrentCharacterType;

            // 1) 초상화 ◀ CharacterSelectManager.characterPortraits 사용
            if (selectedData != null && selectedData.selectedCharacter != null)
            {
                // 1) 초상화 설정
                if (portraitImage != null)
                    portraitImage.sprite = selectedData.selectedCharacter.portraitSprite;

                // 2) 이름 설정
                if (nameText != null)
                    nameText.text = selectedData.selectedCharacter.characterName;
            }
            else
            {
                Debug.LogWarning("[InGameUIManager] CharacterSelectManager에서 올바른 초상화 배열을 찾을 수 없습니다.");
            }

            

            // 3) 레벨, HP, Exp, Fury 초기화
            UpdateLevelText(playerCtrl.currentLevel);
            UpdateHpUI(playerCtrl.currentHp, playerCtrl.Data.maxHp);
            UpdateExpUI(playerCtrl.currentExp, playerCtrl.Data.exp);
            UpdateFuryGauge(playerCtrl.currentFury / playerCtrl.Data.furyMax);

            // 4) 콤보 잠금/해제 상태 초기화
            UpdateComboLockUI(
                playerCtrl.currentLevel,
                playerCtrl.combo2UnlockLevel,
                playerCtrl.combo3UnlockLevel,
                playerCtrl.combo4UnlockLevel
            );
            Debug.Log($"[콤보UI] OnSceneLoaded → currentLevel={playerCtrl.currentLevel}, unlock2={playerCtrl.combo2UnlockLevel}, unlock3={playerCtrl.combo3UnlockLevel}, unlock4={playerCtrl.combo4UnlockLevel}");
        }
    }

    public void RebindUIReferences()
    {
        // ─── Inspector에서 연결되지 않았을 때 자동 바인딩 예시 ────────────
        // if (portraitImage == null)
        //     portraitImage = GameObject.Find("PortraitImage").GetComponent<Image>();
        // if (nameText == null)
        //     nameText = GameObject.Find("NameText").GetComponent<TextMeshProUGUI>();
        // if (hpBarFillImage == null)
        //     hpBarFillImage = GameObject.Find("HpBar/Fill").GetComponent<Image>();
        // ...
        // 필요 시 콤보 슬롯과 Glow도 찾아서 연결할 수 있습니다.
    }

    // ==================================================
    // ⑩ 콤보 관련 필드 (★추가★)
    // ==================================================
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

    // ==================================================
    // ⑪ 콤보 잠금/해제 UI 업데이트 (★추가★)
    // ==================================================
    /// <summary>
    /// playerLevel에 따라 콤보 슬롯 아이콘을 켜거나 끕니다.
    /// - combo1은 항상 활성화
    /// - combo2~combo4는 (playerLevel >= 해당 unlockLevel)일 때만 활성화
    /// </summary>
    public void UpdateComboLockUI(int playerLevel, int unlock2, int unlock3, int unlock4)
    {
        // Combo1 아이콘은 무조건 활성화
        if (comboSlot1 != null)
        {
            comboSlot1.gameObject.SetActive(true);
            comboSlot1.color = Color.white;
        }
        if (combo1Glow != null) combo1Glow.SetActive(false);

        // Combo2 잠금/해제
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

        // Combo3 잠금/해제
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

        // Combo4 잠금/해제
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

        Debug.Log($"[콤보UI] UpdateComboLockUI → playerLevel={playerLevel}, unlock2={unlock2}, unlock3={unlock3}, unlock4={unlock4}");
    }

    // ==================================================
    // ⑫ 콤보 Glow 업데이트 (★추가★)
    // ==================================================
    /// <summary>
    /// comboIndex 단계까지 Glow를 켭니다.
    /// comboIndex=1 → combo1Glow만 켜짐
    /// comboIndex=2 → combo1Glow, combo2Glow 켜짐
    /// </summary>
    public void UpdateComboSlot(int comboIndex)
    {
        GameObject[] glows = { combo1Glow, combo2Glow, combo3Glow, combo4Glow };
        for (int i = 0; i < glows.Length; i++)
        {
            if (glows[i] != null)
                glows[i].SetActive(i < comboIndex);
        }
    }

    // ==================================================
    // ⑬ 콤보 초기화 시 Glow 모두 끕니다 (★추가★)
    // ==================================================
    public void ResetComboSlot()
    {
        GameObject[] glows = { combo1Glow, combo2Glow, combo3Glow, combo4Glow };
        foreach (var glow in glows)
        {
            if (glow != null)
                glow.SetActive(false);
        }
    }

    // ==================================================
    // ⑭ 캐릭터 정보 UI 업데이트 (Portrait, Name, Level)
    // ==================================================
    public void UpdateCharacterInfo(Sprite portrait, string name, int level)
    {
        if (portraitImage != null) portraitImage.sprite = portrait;
        if (nameText != null) nameText.text = name;
        if (levelText != null) levelText.text = $"Lv.{level}";
    }

    // ==================================================
    // ⑮ HP UI 업데이트
    // ==================================================
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

    // ==================================================
    // ⑯ Exp UI 업데이트
    // ==================================================
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

    // ==================================================
    // ⑰ Fury 게이지 업데이트
    // ==================================================
    public void UpdateFuryGauge(float percent)
    {
        if (furyGaugeFillImage != null)
            furyGaugeFillImage.fillAmount = Mathf.Clamp01(percent);
    }

    // ==================================================
    // ⑱ Level UI 업데이트
    // ==================================================
    public void UpdateLevelText(int level)
    {
        if (levelText != null)
            levelText.text = $"Lv.{level}";
    }

    // ==================================================
    // ⑲ 메뉴 팝업 열기/닫기 (원본 기능 그대로)
    // ==================================================
    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OpenInventory()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(true);
    }

    public void CloseInventory()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    // ==================================================
    // ⑳ GameOver 패널 표시 (원본 기능 그대로)
    // ==================================================
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

                // 기존 페이드 인 처리
                while (cg.alpha < 1f)
                {
                    cg.alpha += Time.deltaTime;
                    yield return null;
                }

                cg.alpha = 1f;
            }
            else
            {
                Debug.LogWarning("[GameOver] gameOverPanel에 CanvasGroup이 없습니다.");
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
