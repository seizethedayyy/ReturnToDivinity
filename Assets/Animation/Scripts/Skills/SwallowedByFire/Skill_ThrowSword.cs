using System.Collections.Generic;
using UnityEngine;

public class Skill_ThrowSword : SkillBase
{
    const float DAMAGE_PERCENT = 2.3f;
    const float BUFF_AMOUNT = 1.03f;
    const float BUFF_DURATION = 3f;

    private HashSet<GameObject> alreadyHittedTargets = new(); // 제일 빠르다네요
    [SerializeField] private GameObject chainFirePrefab;

    protected override void Activate(Player player)
    {
        var buff = new StatBuff(StatType.MoveSpeed, BUFF_AMOUNT, BUFF_DURATION);
        player.ApplyBuff(buff);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) { return; }

        DamageToTarget(collision.gameObject);
    }

    void DamageToTarget(GameObject target)
    {
        if (alreadyHittedTargets.Contains(target)) { return; }

        // var enemy = target.GetComponent<Enemy>();
        // enemy?.Damage(DAMAGE_PERCENT * player.CurrentAttackPower);

        alreadyHittedTargets.Add(target);
    }
}
