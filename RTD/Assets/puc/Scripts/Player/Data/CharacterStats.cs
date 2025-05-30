using UnityEngine;

public enum CharacterType
{
    Knight,
    Castle
}

[CreateAssetMenu(fileName = "CharacterStats", menuName = "Game/Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("�⺻ ����")]
    public string characterName;
    public CharacterType characterType;
    
    public int maxHp;
    public float recovery;
    public float attackDamage;
    public float attackSpeed;
    public float attackRange;
    public float moveSpeed;
    public float defense;

    [Header("���� ����")]
    public float hitStunTime = 0.4f; // �ǰ� �� ���� �ð� (��)

    [Header("Fury Skill")]
    public Sprite furySkillIcon;

    [Header("Fury �ý���")]
    public float furyGainPerHit;       // Knight ����
    public float furyMax = 100f;

    [Header("Combo ����")]
    public float comboCooldown;
    public float[] comboMultipliers = { 1.2f, 1.3f, 1.4f, 1.5f }; // �޺��� ���� ���

    [Header("Castle ����")]
    public bool useHitGauge;
    public float hitGaugePerHit;
}