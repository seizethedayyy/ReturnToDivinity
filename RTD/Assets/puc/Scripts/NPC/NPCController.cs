using UnityEngine;
using TMPro;

public class NPCController : MonoBehaviour
{
    [Header("�̵� �ӵ�")]
    public float moveSpeed = 2f;

    [Header("�÷��̾�� ���ߴ� �Ÿ�")]
    [SerializeField] private float stopDistance = 1.5f;

    [Header("����")]
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

                // �ȱ� ���� (0.5�� ����)
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
                        audioSource.PlayOneShot(signSound); // ��� ���
                    }
                }
            }

            // NPC ���� ����
            if (transform.position.x < player.position.x)
                transform.localScale = new Vector3(1, 1, 1);
            else
                transform.localScale = new Vector3(-1, 1, 1);
        }

        // ��ȭ ������ ���� �����̽��ٷ�
        if (Input.GetKeyDown(KeyCode.Space) && !dialogueEnded)
        {
            if (talkPanel.activeSelf == false)
            {
                // ��ȭâ ����
                talkPanel.SetActive(true);

                // Sign ��Ȱ��ȭ
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
            audioSource.PlayOneShot(dialogueSound); // ��� ���
        }

        switch (clickCount)
        {
            case 0:
                text.text = "�� �� ���ô��� ��� �����̽��ϴ�.\n���� �� ������ ������ ī�̷��Դϴ�.";
                break;
            case 1:
                text.text = "�̰��� ���� �ູ �Ʒ� ��ư��� ������ ��������.\n������ ���� ������ �̻��� ���� �Ͼ�� �־��.";
                break;
            case 2:
                text.text = "�ֹε��� �ϳ��Ѿ� ������� �ֽ��ϴ�.\n���������� ��ݵ� ���� ���� ���� �ܰ��̾����.";
                break;
            case 3:
                text.text = "�����ٸ� ���縦 ��Ź����� �ɱ��?";
                text.rectTransform.anchoredPosition = new Vector2(text.rectTransform.anchoredPosition.x, 0f);
                break;
            case 4:
                // ��ȭ ������ NPC ��Ȱ��ȭ
                talkPanel.SetActive(false);
                dialogueEnded = true;
                gameObject.SetActive(false);
                return;
        }

        clickCount++;
    }
}
