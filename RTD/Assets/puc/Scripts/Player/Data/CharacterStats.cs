using UnityEngine;

public enum CharacterType
{
    Knight,
    Castle
}

[CreateAssetMenu(fileName = "CharacterStats", menuName = "Game/Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("기본 스탯")]
    public string characterName;
    public CharacterType characterType;
    
    public int maxHp;
    public float recovery;
    public float attackDamage;
    public float attackSpeed;
    public float attackRange;
    public float moveSpeed;
    public float defense;

    [Header("전투 설정")]
    public float hitStunTime = 0.4f; // 피격 시 경직 시간 (초)

    [Header("Fury Skill")]
    public Sprite furySkillIcon;

    [Header("Fury 시스템")]
    public float furyGainPerHit;       // Knight 전용
    public float furyMax = 100f;

    [Header("Combo 설정")]
    public float comboCooldown;
    public float[] comboMultipliers = { 1.2f, 1.3f, 1.4f, 1.5f }; // 콤보별 보정 계수

    [Header("Castle 전용")]
    public bool useHitGauge;
    public float hitGaugePerHit;
}