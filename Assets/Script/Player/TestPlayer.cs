using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class TestPlayer : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    [Header("이동 설정")]
    public float moveSpeed = 5f;

    [Header("체력 설정")]
    public int maxHp = 100;
    private int currentHp;

    [Header("공격력")]
    public int attackPower = 10;

    private Vector2 moveInput;
    private bool isAttacking = false;
    private bool isHit = false;

    [Header("공격 판정")]
    public Transform attackCheck;
    public float attackRange = 1.0f;
    public LayerMask enemyLayer;

    public float knockbackForce = 5f;
    public float hitFlashDuration = 0.1f;
    private Coroutine flashCoroutine;
    private Color originalColor;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHp = maxHp;
        originalColor = spriteRenderer.color;
        if (originalColor.a < 1f) originalColor.a = 1f;
    }

    private void Start()
    {
        StartCoroutine(InitializeHpUI());
    }

    private IEnumerator InitializeHpUI()
    {
        yield return new WaitUntil(() =>
            InGameUIManager.Instance != null &&
            InGameUIManager.Instance.hpText != null &&
            InGameUIManager.Instance.hpBarFillImage != null
        );

        InGameUIManager.Instance.UpdateHpUI(currentHp, maxHp);
    }

    private void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        bool isMoving = moveInput != Vector2.zero;
        animator.SetBool("Move", isMoving);

        if (!isAttacking)
        {
            if (moveInput.x != 0)
                spriteRenderer.flipX = moveInput.x < 0;
        }

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.CompareTag("NPC"))
                return;

            StartAttack();
        }
    }

    private void FixedUpdate()
    {
        if (!isAttacking)
        {
            Vector2 moveDelta = moveInput.normalized * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + moveDelta);
        }
    }

    private void StartAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        animator.SetBool("IsAttacking", true);
        AudioManager.Instance?.PlaySfx("testSfx");
    }

    public void DoAttack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackCheck.position, attackRange, enemyLayer);
        foreach (Collider2D col in enemies)
        {
            if (col.CompareTag("Enemy") && col.TryGetComponent(out EnemyController enemy))
            {
                enemy.OnHit(attackPower);
                Debug.Log($"[공격] Enemy {enemy.name} → {attackPower} 데미지");
            }
            else
            {
                Debug.LogWarning($"[공격 실패] 대상 없음 또는 EnemyController 미포함: {col.name}");
            }
        }
    }

    public void EndAttack()
    {
        Debug.Log("[EndAttack] 호출됨");
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        InGameUIManager.Instance?.UpdateHpUI(currentHp, maxHp);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("[Player] 사망 처리");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isHit) return;

        if (collision.collider.CompareTag("Enemy"))
        {
            if (isAttacking) return; // 공격 중일 때 넉백 제거

            isHit = true;

            TakeDamage(1);

            Vector2 hitDir = (transform.position - collision.transform.position).normalized;
            rb.AddForce(hitDir * knockbackForce, ForceMode2D.Impulse);

            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashWhiteWithAlpha());

            Invoke(nameof(ResetHit), 0.5f);
        }
    }

    private IEnumerator FlashWhiteWithAlpha()
    {
        Color fadedWhite = new Color(1f, 1f, 1f, 0.3f);
        spriteRenderer.color = fadedWhite;

        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }

    private void ResetHit()
    {
        isHit = false;
    }

    public int GetCurrentHp() => currentHp;
    public int GetMaxHp() => maxHp;

    private void OnDrawGizmosSelected()
    {
        if (attackCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackCheck.position, attackRange);
    }
}