using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public int nextMove; // 행동지표를 결정할 변수

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    CapsuleCollider2D capsuleCollider;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();   
        capsuleCollider = GetComponent<CapsuleCollider2D>();    

        // Invoke() : 주어진 시간이 지난 뒤, 지정된 함수를 실행한다.
        Invoke(nameof(Think), 5);
    }

    void FixedUpdate()
    {
        // 이동하기
        rigid.velocity = new Vector2(nextMove,rigid.velocity.y);

        // 지형 체크
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.3f, rigid.position.y); // 다음에 이동할 위치를 구한다.

        Debug.DrawRay(frontVec, Vector3.down, new Color(1, 0, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(
            frontVec,
            Vector3.down, 1,
            LayerMask.GetMask("Platform")   // Ray에 맞을 오브젝트로 바닥을 설정
        );

        if(rayHit.collider == null) // 다음 지형이 없다면
        {
            Turn(); // 회전한다.
        }
    }

    // 재귀 함수
    void Think()
    {
        // 다음 이동 방향설정
        nextMove = Random.Range(-1, 2);
        
        // 이동 애니메이션
        anim.SetInteger("walkSpeed", nextMove);

        // 방향전환
        if (nextMove != 0)
            spriteRenderer.flipX = nextMove == 1;

        // 재귀
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke(nameof(Think), nextThinkTime);
    }

    void Turn()
    {
        nextMove *= -1; // 다음에 이동할 방향을 바꾼다.
        spriteRenderer.flipX = nextMove == 1;

        CancelInvoke();// 현재 작동 중인 모든 Invoke함수를 멈춘다.
        Invoke(nameof(Think), 5);
    }

    // 몬스터가 죽었을 때 처리
    public void OnDamaged()
    {
        // 투명도 지정
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // y축 뒤집기
        spriteRenderer.flipY = true;

        // 콜라이더 지정
        capsuleCollider.enabled = false;

        // 점프
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        // 5초뒤 사라짐
        Invoke(nameof(DeActive), 5);
    }

    void DeActive()
    {
        gameObject.SetActive(false);
    }
}
