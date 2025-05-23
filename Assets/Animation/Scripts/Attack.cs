using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private Player player;
    private Animator anim;
    private Collider2D coll;

    private HashSet<GameObject> alreadyHittedTargets = new(); // 제일 빠르다네요

    void Awake()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
        player = GetComponentInParent<Player>();
        coll.enabled = false;
    }

    void EnableCollider()
    {
        coll.enabled = true;
    }

    void DisableCollider()
    {
        coll.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) { return; }

        // collision.gameObject.GetComponent<Enemy>().Damage(player.CurrentAttackPower);
    }

}



