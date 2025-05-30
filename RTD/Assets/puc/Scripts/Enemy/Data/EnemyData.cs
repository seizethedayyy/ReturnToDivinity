using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class EnemyData
{
    public string id;
    public string name;
    public int level;
    public float maxHp;
    public float moveSpeed;
    public float detectionRange;
    public float attackRadius;
    public float attackDamage;
    public float chaseStopRange;
    public float stopDistance;
    public float angleThreshold;
    public float attackCooldown;
    public int exp;
    public int gold;
}

[System.Serializable]
public class EnemyDataRoot
{
    public List<EnemyData> data;
}