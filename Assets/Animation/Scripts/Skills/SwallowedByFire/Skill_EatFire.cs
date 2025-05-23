using UnityEngine;

public class Skill_EatFire : SkillBase
{
    const float BUFF_AMOUNT = 1.3f;
    const float BUFF_DURATION = 2f;

    protected override void Activate(Player player)
    {
        var buff = new StatBuff(StatType.AttackPower, BUFF_AMOUNT, BUFF_DURATION);
        player.ApplyBuff(buff);
    }
}
