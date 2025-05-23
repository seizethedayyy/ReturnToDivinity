using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text descriptionText;

    [Header("캐릭터 설명")]
    [TextArea] public string[] characterDescriptions;

    [Header("스킬 설명 (Q/E/R 순)")]
    [TextArea] public string[] skillQDescriptions;
    [TextArea] public string[] skillEDescriptions;
    [TextArea] public string[] skillRDescriptions;

    [Header("인스펙터에서 설정할 데이터")]
    public string[] characterIds;              // ✅ 각 캐릭터의 고유 ID
    public string[] characterNames;
    public Sprite[] characterPortraits;

    [Header("패시브 스킬 아이콘")]
    public Sprite[] passiveSkillIcons;

    [Header("패시브 스킬 정보")]
    public string[] passiveSkillNames;
    [TextArea] public string[] passiveSkillDescriptions;
    public int[] passiveSkillLevels;

    [Header("스킬 아이콘")]
    public Sprite[] skillRIcons;
    public Sprite[] skillEIcons;
    public Sprite[] skillQIcons;

    [Header("효과음")]
    public AudioClip selectSfx;
    public AudioClip confirmSfx;

    private int selectedIndex = -1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("TitleScene");
        }
    }

    public void SelectCharacter(int index)
    {
        selectedIndex = index;

        if (AudioManager.Instance != null && selectSfx != null)
        {
            AudioManager.Instance.PlaySfx(selectSfx);
        }

        if (index >= 0 && index < characterDescriptions.Length)
        {
            descriptionText.text = characterDescriptions[index];
        }
    }

    public void ConfirmSelection()
    {
        if (selectedIndex == -1)
        {
            if (descriptionText != null)
                descriptionText.text = "<color=#FFCC00>캐릭터를 선택하세요!</color>";
            return;
        }

        StartCoroutine(ConfirmSelectionDelayed());
    }

    private IEnumerator ConfirmSelectionDelayed()
    {
        if (AudioManager.Instance != null && confirmSfx != null)
            AudioManager.Instance.PlaySfx(confirmSfx);

        yield return new WaitForSeconds(0.3f);

        // ✅ 기존 Player 제거
        GameObject existingPlayer = GameObject.FindWithTag("Player");
        if (existingPlayer != null)
        {
            Destroy(existingPlayer);
            Debug.Log("[선택] 기존 씬 Player 오브젝트 제거 완료");
        }

        // ✅ 선택된 캐릭터 ID로 프리팹 로드
        string id = characterIds[selectedIndex];
        GameObject selectedPlayablePrefab = Resources.Load<GameObject>($"Characters/{id}");

        if (selectedPlayablePrefab == null)
        {
            Debug.LogError($"[선택 실패] Resources/Characters/{id}.prefab 파일을 찾을 수 없습니다. ID를 확인하세요.");
            yield break;
        }

        // ✅ 프리팹 인스턴스화 및 유지
        GameObject persistentPrefab = Instantiate(selectedPlayablePrefab);
        DontDestroyOnLoad(persistentPrefab);

        string prefabName = selectedPlayablePrefab.name;

        // ✅ 캐릭터 이름 커스터마이징
        string autoName = characterNames[selectedIndex];
        switch (prefabName)
        {
            case "Player1": autoName = "불에 삼켜진 자"; break;
            case "Player2": autoName = "하늘에서 온 자"; break;
            case "Player3": autoName = "다가설 수 없는 자"; break;
        }

        // ✅ 선택 캐릭터 정보 저장
        SelectedCharacterData.Instance.SelectCharacter(
            id, autoName, characterPortraits[selectedIndex], passiveSkillIcons[selectedIndex],
            skillRIcons[selectedIndex], skillEIcons[selectedIndex], skillQIcons[selectedIndex],
            skillQDescriptions[selectedIndex], skillEDescriptions[selectedIndex], skillRDescriptions[selectedIndex],
            passiveSkillNames[selectedIndex], passiveSkillDescriptions[selectedIndex], passiveSkillLevels[selectedIndex],
            persistentPrefab,
            prefabName
        );

        // ✅ 다음 씬으로 이동
        SceneManager.LoadScene("VillageScene");
    }
}