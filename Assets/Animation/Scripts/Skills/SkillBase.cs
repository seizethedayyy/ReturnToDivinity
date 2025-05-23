using UnityEngine;

public abstract class SkillBase : MonoBehaviour
{
    protected Player player;
    protected float cooldown;
    protected float lastUseTime;
    void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    
    public virtual void UseSkill(Player player)
    {
        if (Time.time < cooldown + lastUseTime) { return; }

        lastUseTime = Time.time;

        Activate(player);
    }

    protected abstract void Activate(Player player);
}
