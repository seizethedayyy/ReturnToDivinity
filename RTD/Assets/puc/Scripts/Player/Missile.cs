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

    private Transform shooter; // 발사한 주체 (플레이어)

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

    public void Init(Vector2 dir, int dmg, Transform shooterTransform)
    {
        direction = dir.normalized;
        damage = dmg;
        shooter = shooterTransform;

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
            if (collision.CompareTag("Enemy") && collision.TryGetComponent(out EnemyBase enemy))
            {
                if (shooter == null)
                {
                    Debug.LogWarning("[Missile] shooter가 설정되지 않았습니다.");
                }

                enemy.OnHit(damage, shooter);  // ✅ 수정된 부분

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
