using System.Collections;
using UnityEngine;

public class NamePower_Fire : NamePowerBase
{
    const float BUFF_AMOUNT_AS= 1.10f;
    const float BUFF_AMOUNT_MS = 1.10f;
    const float BUFF_DURATION = 0.5f;
    const float TICK_DELAY = 0.5f;
    const int TICK_COUNT = 5;
    const float TICK_DAMAGE_PERCENT = 0.02f;
    [SerializeField] private float rayRadius;
    private LayerMask enemyLayer;

    private Collider2D coll;

    private bool isNearbyEnemy;

    protected override void Awake()
    {
        base.Awake();
        coll = GetComponent<Collider2D>();
        StartCoroutine(CheckEnemy());
    }

    IEnumerator CheckEnemy()
    {
        while (true)
        {
            isNearbyEnemy = Physics2D.OverlapCircle(player.transform.position, rayRadius, enemyLayer);

            if (isNearbyEnemy)
            {
                var buff1 = new StatBuff(StatType.AttackSpeed, BUFF_AMOUNT_AS, BUFF_DURATION);
                player.ApplyBuff(buff1);

                var buff2 = new StatBuff(StatType.MoveSpeed, BUFF_AMOUNT_MS, BUFF_DURATION);
                player.ApplyBuff(buff2);
            }

            yield return new WaitForSeconds(BUFF_DURATION);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rayRadius);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) { return; }

        StartCoroutine("BurnDamage", collision);
    }

    IEnumerator BurnDamage(Collider2D enemy)
    {
        for (int i = 0; i < TICK_COUNT; ++i)
        {
            // enemy.gameObject.GetComponent<Enemy>().Damage(player.CurrentAttackPower * TICK_DAMAGE_PERCENT);
            yield return new WaitForSeconds(TICK_DELAY);
        }
    }

    public void Active()
    {
        StartCoroutine(ActiveCollider());
    }

    IEnumerator ActiveCollider()
    {
        coll.enabled = true;
        yield return new WaitForSeconds(0.1f);
        coll.enabled = false;
    }
}
