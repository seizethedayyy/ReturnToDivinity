using UnityEngine;

[System.Serializable]
public class CharacterInfo
{
    public string characterId;
    public string characterName;
    public Sprite portraitSprite;

    public Sprite furySkillIcon;
    public string furySkillName;
    public string furySkillDescription;

    public string prefabName;
}

public class SelectedCharacterData : MonoBehaviour
{
    public static SelectedCharacterData Instance { get; private set; }

    public CharacterInfo selectedCharacter;
    public GameObject selectedCharacterPrefab;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectCharacter(
        string id, string name, Sprite portrait,
        Sprite furyIcon, string furyName, string furyDesc,
        GameObject prefab, string prefabName
    )
    {
        selectedCharacter = new CharacterInfo
        {
            characterId = id,
            characterName = name,
            portraitSprite = portrait,
            furySkillIcon = furyIcon,
            furySkillName = furyName,
            furySkillDescription = furyDesc,
            prefabName = prefabName
        };

        selectedCharacterPrefab = prefab;
    }
}
