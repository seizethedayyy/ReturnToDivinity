using System;
using UnityEngine;

public class Skill_Swallow : SkillBase
{
    const float DAMAGE_PERCENT = 0.05f;
    const float BUFF_AMOUNT = 1.5f;
    const float BUFF_DURATION = 5f;
    [SerializeField] private GameObject fireBoomPrefab;
    protected override void Activate(Player player)
    {
        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
        {
            var buff = new StatBuff(stat, BUFF_AMOUNT, BUFF_DURATION);
            player.ApplyBuff(buff);
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) { return; }

        // collision.GetComponent<Enemy>().Damage(DAMAGE_PERCENT * player.CurrentAttackPower)
    }

}
