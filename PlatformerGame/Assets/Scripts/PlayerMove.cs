using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed;
    public float jumpPower;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
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
        }

        // 방향키에서 손을 뗄 시 속도를 줄인다.
        if(Input.GetButtonUp("Horizontal"))
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);

        // 방향 전환
        if (Input.GetButtonDown("Horizontal"))  // 방향키를 입력할 때
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;    // 왼쪽 방향키 입력시 방향을 전환한다.

        // 이동 애니메이션 처리
        if (Mathf.Abs(rigid.velocity.x) < 0.3)   // 멈췄으면
            anim.SetBool("isWalking", false);   // 정지상태로 전환한다.
        else // 이동 중이면
            anim.SetBool("isWalking", true);    // 걷기상태로 전환한다.
    }
    
    void FixedUpdate()
    {
        // 속도 증가
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

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
        // RaycastHit는 관통이 안된다. 즉 콜라이더를 한 번 맞으면 끝임.

        if(rigid.velocity.y < 0)    // 속도가 마이너스 일 때 오브젝트를 Ray를 쏜다.
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
}
