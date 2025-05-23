using System.Collections.Generic;
using UnityEngine;

public class Skill_Judgement : SkillBase
{
    const float BEAM_CREATE_DELAY = 3f;
    const float DAMAGE_PERCENT = 2.3f;
    private HashSet<GameObject> alreadyHittedTargets = new(); // 제일 빠르다네요
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private GameObject sparkPrefab;
    protected override void Activate(Player player)
    {
        Invoke("CreateBeam", BEAM_CREATE_DELAY);
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

    void CreateBeam()
    {
        Instantiate(beamPrefab, null);
    }
}
