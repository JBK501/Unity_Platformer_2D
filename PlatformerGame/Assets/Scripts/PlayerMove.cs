using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;
    public float maxSpeed;
    public float jumpPower;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    CapsuleCollider2D capsuleCollider;
    AudioSource audioSource;


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    

    // 단발적인 키 입력일 때 Update사용
    // 지속적인 키 입력일 때 FixedUpdate사용(이때 단발적인 키 입력 발생 시 씹히는 현상이 발생할 수 있음) 초당 프레임이 더 낮기 때문
    void Update()
    {
        // 점프
        // 점프키를 눌렀고, 점프하고 있는 상태가 아닐 때(연속 점프 막음)
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping")) 
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse); // 점프한다.
            anim.SetBool("isJumping", true);

            PlaySound("JUMP");
        }

        // 방향키에서 손을 뗄 시 속도를 줄인다.
        if(Input.GetButtonUp("Horizontal"))
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);

        // 방향 전환(왼쪽 방향키를 누르면 뒤집고 오른쪽 방향키를 누르면 원상태로 복귀)
        if (Input.GetButton("Horizontal"))  
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;    

        // 이동 애니메이션 처리
        if (Mathf.Abs(rigid.velocity.x) < 0.3f)   // 멈췄으면
            anim.SetBool("isWalking", false);   // 정지상태로 전환한다.
        else // 이동 중이면
            anim.SetBool("isWalking", true);    // 걷기상태로 전환한다.
    }
    
    void FixedUpdate()
    {
        // 속도 증가
        float h = Input.GetAxisRaw("Horizontal"); // (-1, 0, 1) 방향을 받아온다.
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse); // 해당 방향으로 업데이트 할 때 마다 이동한다.

        // 최대속도 지정
        if (rigid.velocity.x > maxSpeed)    
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if(rigid.velocity.x < maxSpeed*(-1))
            rigid.velocity = new Vector2(maxSpeed*(-1), rigid.velocity.y);

        // 점프 해제
        // {RayCast : 오브젝트 검색을 위해 Ray를 쏘는 방식}

        // DrawRay : 에디터 상에서만 Ray를 그려주는 함수
        
        // RaycastHit : Ray에 닿은 오브젝트
        // RaycastHit변수의 콜라이더로 닿은 오브젝트 확인 가능
        // RaycastHit은 관통이 안된다. 즉 콜라이더를 한 번 맞으면 끝임(플레이어에도 콜라이더가 있어서 문제가 됨).

        if(rigid.velocity.y < 0)    // 속도가 마이너스 일 때 Ray를 쏜다.
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(1, 0, 0));

            RaycastHit2D rayHit = Physics2D.Raycast(
                rigid.position,
                Vector3.down, 1,
                LayerMask.GetMask("Platform")   // Ray에 맞을 오브젝트로 바닥을 설정
            );

            if (rayHit.collider != null) // Ray가 바닥에 맞았으면
            {
                if (rayHit.distance < 0.5f) // Ray와 바닥과의 거리가 0.5미만일 때 
                    anim.SetBool("isJumping", false);   // 점프를 헤제한다.
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy") // 적과 충돌할 때
        {
            // 적 위에 있을 때
            if(rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
            }
            else
                OnDamaged(collision.transform.position);
        }
        else if(collision.gameObject.tag == "Spike")
        {
            OnDamaged(collision.transform.position);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Item")
        {
            // 점수
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if (isBronze)
                gameManager.stagePoint += 50;
            else if (isSilver)
                gameManager.stagePoint += 100;
            else if (isGold)
                gameManager.stagePoint += 300;


            // 아이템 비활성화
            collision.gameObject.SetActive(false);

            PlaySound("ITEM");
        }
        else if(collision.gameObject.tag == "Finish")
        {
            // 다음 스테이지
            gameManager.NextStage();

            PlaySound("FINISH");
        }
    }

    void OnAttack(Transform enemy)
    {
        // 점수
        gameManager.stagePoint += 100;

        // 적 밟을 시 반발력 적용
        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

        // 사운드
        PlaySound("ATTACK");

        // 적 사망
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }

    // 플레이어가 데미지를 입었을 때 처리
    void OnDamaged(Vector2 targetPos)
    {
        // 체력 깎기
        gameManager.HealthDown();

        // 반대 방향으로 튕긴다.
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);

        // 투명도 설정
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // 레이어 변경(무적모드 설정)
        gameObject.layer = 11;

        // 애니메이션
        anim.SetTrigger("doDamaged"); 
        
        // 사운드
        PlaySound("DAMAGED");

        // 3초후 무적모드 해제
        Invoke(nameof(OffDamaged), 3);
    }

    // 무적모드 해제
    void OffDamaged()
    {
        gameObject.layer = 10;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void OnDie()
    {
        // 투명도 지정
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // y축 뒤집기
        spriteRenderer.flipY = true;

        // 콜라이더 지정
        capsuleCollider.enabled = false;

        // 점프
        rigid.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);

        PlaySound("DIE");
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }

    void PlaySound(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
            case "DIE":
                audioSource.clip = audioDie;
                break;
            case "FINISH":
                audioSource.clip = audioFinish;
                break;
        }

        audioSource.Play();
    }
}
