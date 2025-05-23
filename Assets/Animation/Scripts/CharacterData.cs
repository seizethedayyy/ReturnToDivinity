using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    [SerializeField] private int characterID;
    [SerializeField] private string characterName;
    [SerializeField] private float baseMaxHp;
    [SerializeField] private float baseAttackPower;
    [SerializeField] private float baseDefensePower;
    [SerializeField] private float baseAttackSpeed;
    [SerializeField] private float baseMoveSpeed;
    [SerializeField] private float baseHpRegen;
    [SerializeField] private float[] comboDelays;
    [SerializeField] private GameObject[] skills;
    [SerializeField] private Animator attackEffect;
    [SerializeField] private Animator playerAnim;
    [SerializeField] private GameObject namePowerPrefab;
    [SerializeField] private Vector2 colliderOffset;
    [SerializeField] private Vector2 colliderSize;

    public int CharacterID  => characterID;
    public string CharacterName  => characterName;
    public float BaseMaxHp  => baseMaxHp;
    public float BaseAttackPower  => baseAttackPower;
    public float BaseDefensePower  => baseDefensePower;
    public float BaseAttackSpeed  => baseAttackSpeed;
    public float BaseMoveSpeed  => baseMoveSpeed;
    public float BaseHpRegen  => baseHpRegen;
    public int FullComboCount => comboDelays.Length;
    public float[] ComboDelays => comboDelays;
    public GameObject[] Skills => skills;
    public Animator AttackEffect => attackEffect;
    public Animator PlayerAnim => playerAnim;
    public GameObject NamePowerPrefab => namePowerPrefab;
    public Vector2 ColliderOffset => colliderOffset;
    public Vector2 ColliderSize => colliderSize;
}
