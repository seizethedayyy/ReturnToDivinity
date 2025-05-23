using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    const float DEFENSE_CONSTANT = 120f; // 수가 커질수록 방어력 감소
    private Vector2 inputVec;
    private Animator anim;
    private Animator attackAnim;
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    [SerializeField] private CharacterData characterData;

    private SkillBase[] skills;
    private NamePowerBase namePower;

    
    private bool isMujeok;
    [SerializeField] private float dodgeCooldown;
    private float lastDodgeTime;

    private bool InputComboAlready;
    private float comboLimitTime;

    private bool canMove;

    #region Stats
    private float currentHp;
    private float currentMaxHp;
    private float currentAttackPower;
    private float currentDefensePower;
    private float currentAttackSpeed;
    private float currentMoveSpeed;
    private float currentHpRegen;

    public float CurrentHp => currentHp;
    public float CurrentMaxHp => currentMaxHp;
    public float CurrentAttackPower => GetBuffedStat(StatType.AttackPower, currentAttackPower);
    public float CurrentDefensePower => GetBuffedStat(StatType.DefensePower, currentDefensePower);
    public float CurrentAttackSpeed => GetBuffedStat(StatType.AttackSpeed, currentAttackSpeed);
    public float CurrentMoveSpeed => GetBuffedStat(StatType.MoveSpeed, currentMoveSpeed);
    public float CurrentHpRegen => GetBuffedStat(StatType.HpRegen, currentHpRegen);

    private Dictionary<StatType, List<float>> activeBuff = new();

    #endregion



    public Animator Anim => anim;
    public CharacterData CharacterData => characterData;

    #region Awake Settings
    void Awake()
    {
        GetNecessaryComponents();
        SetDefaultValue();
        // getcharacterdata
        // 애니메이터 설정 - 캐릭터 데이터에서
        SetCollider(); // 캐릭터데이터받고나서
        SetPlayerStat(); // 캐릭터데이터받고나서
        SetSkills(); // 캐릭터데이터 받고나서
        SetAttackAnimator(); // 캐릭터데이터 받고나서
        SetNamePower(); // 캐릭터데이터 받고나서
    }
    
    void GetNecessaryComponents()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
    }

    void SetDefaultValue()
    {
        InputComboAlready = false;
        canMove = true;
    }

    void SetAnimator()
    {
        anim.runtimeAnimatorController = CharacterData.PlayerAnim.runtimeAnimatorController;
    }

    void SetCollider()
    {
        if (coll != null)
        {
            coll.offset = characterData.ColliderOffset;
            coll.size = characterData.ColliderSize;
        }
    }

    void SetPlayerStat()
    {
        currentHp = characterData.BaseMaxHp;
        currentMaxHp = characterData.BaseMaxHp;
        currentAttackPower = characterData.BaseAttackPower;
        currentDefensePower = characterData.BaseDefensePower;
        currentAttackSpeed = characterData.BaseAttackSpeed;
        currentMoveSpeed = characterData.BaseMoveSpeed;
        currentHpRegen = characterData.BaseHpRegen;
    }

    void SetSkills()
    {
        skills = new SkillBase[3];

        for (int i = 0; i < characterData.Skills.Length; i++)
        {
            var skill = Instantiate(characterData.Skills[i], transform);
            skills[i] = skill.GetComponent<SkillBase>();
        }
    }

    void SetAttackAnimator()
    {
        attackAnim = transform.Find("PlayerAttack").GetComponent<Animator>();
        attackAnim.runtimeAnimatorController = CharacterData.AttackEffect.runtimeAnimatorController;
    }

    void SetNamePower()
    {
        GameObject namePowerObj = Instantiate(characterData.NamePowerPrefab, transform);
        namePower = namePowerObj.GetComponent<NamePowerBase>();
    }
    #endregion


    void FixedUpdate()
    {
        Moving();
    }

    void Update()
    {
        CheckComboWindow();
    }

    void Moving()
    {
        if (!canMove) { return; }
        Vector2 nextVec = inputVec * currentMoveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + nextVec);
    }

    void LateUpdate()
    {
        anim.SetBool("Move", inputVec != Vector2.zero);
        Flip();
    }

    void Flip()
    {
        if (!canMove) { return; }
        if ((inputVec.x < 0 && Mathf.RoundToInt(transform.rotation.y) == 0) || (inputVec.x > 0 && Mathf.RoundToInt(transform.rotation.y) != 0))
        {
            transform.Rotate(0, 180, 0);
        }
    }

    void OnMove(InputValue inputValue)
    {
        inputVec = inputValue.Get<Vector2>();
    }

    #region Attack
    void OnAttack(InputValue inputValue)
    {
        anim.SetTrigger("Attack");
        attackAnim.SetTrigger("On");

        if (!InputComboAlready)
        {
            int comboCount = anim.GetInteger("ComboCount");
            anim.SetInteger("ComboCount", comboCount >= characterData.FullComboCount-1 ? 0 : comboCount+1);
            attackAnim.SetInteger("Combo", comboCount >= characterData.FullComboCount-1 ? 0 : comboCount+1);
            InputComboAlready = true;
        }
    }

    void OpenComboWindow()
    {
        InputComboAlready = false;
        comboLimitTime = Time.time + characterData.ComboDelays[anim.GetInteger("ComboCount")];
    }

    void CheckComboWindow()
    {
        if (Time.time > comboLimitTime)
        {
            anim.SetInteger("ComboCount", 0);
            attackAnim.SetInteger("Combo", 0);
            CanMove();
        }
    }
    #endregion

    void StopMove()
    {
        canMove = false;
    }

    void CanMove()
    {
        canMove = true;
    }

    #region Dodge
    void OnDodge(InputValue inputValue)
    {
        if (Time.time < lastDodgeTime + dodgeCooldown) { return; }
        anim.SetTrigger("Dodge");
    }

    void StartDodge()
    {
        isMujeok = true;
    }

    void EndDodge()
    {
        isMujeok = false;
        lastDodgeTime = Time.time;
    }
    #endregion

    #region Skills
    void OnSkill1(InputValue inputValue)
    {
        anim.SetTrigger("Skill1");
        skills[0]?.UseSkill(this);
    }
    void OnSkill2(InputValue inputValue)
    {
        anim.SetTrigger("Skill2");
        skills[1]?.UseSkill(this);
    }
    void OnSkill3(InputValue inputValue)
    {
        anim.SetTrigger("Skill3");
        skills[2]?.UseSkill(this);
    }
    #endregion

    public void Damage(float damageAmount)
    {
        float defensing = CurrentDefensePower / (CurrentDefensePower + DEFENSE_CONSTANT);
        currentHp -= damageAmount * (1f - defensing); // 이벤트 매니저로 처리

        if (CurrentHp < 0) // 추후 이벤트 매니저로 이전
        {
            Die();
        }
    }

    void Die()
    {
        anim.SetTrigger("Death");
        isMujeok = true;
        StopMove();
    }


    #region Buff Mechanism
    public float GetBuffedStat(StatType statType, float baseValue)
    {
        float totalAmount = 1f;
        if (activeBuff.TryGetValue(statType, out var buffs))
        {
            foreach (float buffAmount in buffs)
            {
                totalAmount *= buffAmount;
            }
        }
        return baseValue * totalAmount;
    }

    public void ApplyBuff(StatBuff buff)
    {
        StartCoroutine(ControlBuff(buff));
    }

    private IEnumerator ControlBuff(StatBuff buff)
    {
        if (!activeBuff.ContainsKey(buff.TargetStat))
        {
            activeBuff[buff.TargetStat] = new List<float>();
        }

        activeBuff[buff.TargetStat].Add(buff.MultipleAmount);
        yield return new WaitForSeconds(buff.Duration);
        activeBuff[buff.TargetStat].Remove(buff.MultipleAmount);
    }

    #endregion
}
