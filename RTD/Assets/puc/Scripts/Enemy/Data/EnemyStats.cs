using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Game/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Ω∫≈»")]
    public float maxHp = 30f;
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRadius = 2.0f;
    public float attackDamage = 10f;
    public float chaseStopRange = 7f;
    public float stopDistance = 1.5f;
    public float angleThreshold = 180f;
    public float attackCooldown = 1.5f;
}