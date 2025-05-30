using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                player.TakeDamage(damage);
                Debug.Log("[HitBox] 플레이어에게 데미지 적용 완료");
            }
        }
    }
}