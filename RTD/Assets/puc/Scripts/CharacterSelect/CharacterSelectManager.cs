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

    [Header("인스펙터에서 설정할 데이터")]
    public string[] characterIds;
    public string[] characterNames;
    public Sprite[] characterPortraits;

    [Header("퓨리 스킬 정보")]
    public Sprite[] furySkillIcons;
    public string[] furySkillNames;
    [TextArea] public string[] furySkillDescriptions;

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

        if (selectedIndex == 2)
        {
            if (descriptionText != null)
                descriptionText.text = "<color=#FF3300>이 캐릭터는 아직 사용할 수 없습니다.</color>";
            return;
        }

        StartCoroutine(ConfirmSelectionDelayed());
    }

    private IEnumerator ConfirmSelectionDelayed()
    {
        if (AudioManager.Instance != null && confirmSfx != null)
            AudioManager.Instance.PlaySfx(confirmSfx);

        yield return new WaitForSeconds(0.3f);

        GameObject existingPlayer = GameObject.FindWithTag("Player");
        if (existingPlayer != null)
        {
            Destroy(existingPlayer);
        }

        string id = characterIds[selectedIndex];
        GameObject selectedPlayablePrefab = Resources.Load<GameObject>($"Characters/{id}");

        if (selectedPlayablePrefab == null)
        {
            Debug.LogError($"[선택 실패] Resources/Characters/{id}.prefab 파일을 찾을 수 없습니다.");
            yield break;
        }

        GameObject persistentPrefab = Instantiate(selectedPlayablePrefab);
        DontDestroyOnLoad(persistentPrefab);
        string prefabName = selectedPlayablePrefab.name;

        string autoName = characterNames[selectedIndex];
        switch (prefabName)
        {
            case "Player1": autoName = "불에 삼켜진 자"; break;
            case "Player2": autoName = "하늘에서 온 자"; break;
            case "Player3": autoName = "다가설 수 없는 자"; break;
        }

        SelectedCharacterData.Instance.SelectCharacter(
            id, autoName, characterPortraits[selectedIndex],
            furySkillIcons[selectedIndex], furySkillNames[selectedIndex], furySkillDescriptions[selectedIndex],
            persistentPrefab, prefabName
        );

        SceneManager.LoadScene("VillageScene");
    }
}
