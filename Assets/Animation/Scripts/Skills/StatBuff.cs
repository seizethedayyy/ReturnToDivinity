using UnityEngine;

public class StatBuff
{
    public StatType TargetStat;
    public float MultipleAmount;
    public float Duration;

    public StatBuff(StatType stat, float amount, float duration)
    {
        TargetStat = stat;
        MultipleAmount = amount;
        Duration = duration;
    }
}
