using System.Collections;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public float speed = 10f;
    private Vector2 direction;
    private int damage;
    private Rigidbody2D rb;

    public LayerMask enemyLayer;

    private Transform fireOrigin; // 최초 발사 위치 기준용 (optional)
    private bool initialized = false;

    private void OnEnable()
    {
        if (!initialized)
        {
            rb = GetComponent<Rigidbody2D>();
            transform.SetParent(null); // 부모에서 분리하여 캐릭터 움직임 영향 제거
            initialized = true;
        }

        StartCoroutine(AutoDisable(2f));
    }

    public void Init(Vector2 dir, int dmg)
    {
        direction = dir.normalized;
        damage = dmg;

        // 발사 시점에 위치 고정 + 부모 분리
        transform.SetParent(null);
    }

    private void Update()
    {
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            if (collision.CompareTag("Enemy") && collision.TryGetComponent(out EnemyController enemy))
            {
                enemy.OnHit(damage);

                // PlayerController로부터 Fury 획득 함수 호출 시도
                TryNotifyFuryGain();

                gameObject.SetActive(false);
            }
        }
    }

    private void TryNotifyFuryGain()
    {
        // 화면 내의 PlayerController 탐색 (비효율이지만 Missile 내 독립 처리 방식)
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.GainFury(); // GainFury가 public이어야 함
        }
    }

    private IEnumerator AutoDisable(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
