using UnityEngine;
using TMPro;

public class NPCController : MonoBehaviour
{
    [Header("이동 속도")]
    public float moveSpeed = 2f;

    [Header("플레이어와 멈추는 거리")]
    [SerializeField] private float stopDistance = 1.5f;

    [Header("사운드")]
    public AudioClip signSound;
    public AudioClip dialogueSound;
    public AudioClip walkSound;

    private AudioSource audioSource;
    private GameObject signObject;
    private Transform player;
    private Animator animator;

    private bool hasMovedToStartPosition = false;
    private bool signVisible = true;
    private bool dialogueEnded = false;

    public GameObject talkPanel;
    public TextMeshProUGUI text;

    private int clickCount = 0;
    private float walkSoundTimer = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsWalk", true);
        }

        foreach (Transform child in transform)
        {
            if (child.CompareTag("Sign"))
            {
                signObject = child.gameObject;
                signObject.SetActive(true);
                break;
            }
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (!hasMovedToStartPosition)
        {
            float randX = Random.Range(0f, 8.4f);
            float randY = Random.Range(-4.3f, 4.3f);
            transform.position = new Vector3(randX, randY, 0f);
            hasMovedToStartPosition = true;
        }

        if (player != null && !dialogueEnded)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer > stopDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                if (animator != null) animator.SetBool("IsWalk", true);

                // 걷기 사운드 (0.5초 간격)
                walkSoundTimer += Time.deltaTime;
                if (walkSoundTimer >= 0.5f && walkSound != null && audioSource != null)
                {
                    walkSoundTimer = 0f;
                    audioSource.PlayOneShot(walkSound);
                }

                if (signObject != null && signVisible && signObject.activeSelf)
                {
                    signObject.SetActive(false);
                }
            }
            else
            {
                if (animator != null) animator.SetBool("IsWalk", false);

                if (signObject != null && signVisible && !signObject.activeSelf)
                {
                    signObject.SetActive(true);
                    if (signSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(signSound); // 즉시 재생
                    }
                }
            }

            // NPC 방향 조정
            if (transform.position.x < player.position.x)
                transform.localScale = new Vector3(1, 1, 1);
            else
                transform.localScale = new Vector3(-1, 1, 1);
        }

        // 대화 진행은 오직 스페이스바로
        if (Input.GetKeyDown(KeyCode.Space) && !dialogueEnded)
        {
            if (talkPanel.activeSelf == false)
            {
                // 대화창 생성
                talkPanel.SetActive(true);

                // Sign 비활성화
                if (signObject != null)
                {
                    signObject.SetActive(false);
                    signVisible = false;
                }
            }

            AdvanceDialogue();
        }
    }


    private void AdvanceDialogue()
    {
        if (dialogueEnded) return;

        if (dialogueSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dialogueSound); // 즉시 재생
        }

        switch (clickCount)
        {
            case 0:
                text.text = "먼 길 오시느라 고생 많으셨습니다.\n저는 이 마을의 관리인 카이렐입니다.";
                break;
            case 1:
                text.text = "이곳은 신의 축복 아래 살아가는 조용한 마을이죠.\n하지만 요즘 마을에 이상한 일이 일어나고 있어요.";
                break;
            case 2:
                text.text = "주민들이 하나둘씩 사라지고 있습니다.\n마지막으로 목격된 곳은 마을 동쪽 외곽이었어요.";
                break;
            case 3:
                text.text = "괜찮다면 조사를 부탁드려도 될까요?";
                text.rectTransform.anchoredPosition = new Vector2(text.rectTransform.anchoredPosition.x, 0f);
                break;
            case 4:
                // 대화 끝내고 NPC 비활성화
                talkPanel.SetActive(false);
                dialogueEnded = true;
                gameObject.SetActive(false);
                return;
        }

        clickCount++;
    }
}
