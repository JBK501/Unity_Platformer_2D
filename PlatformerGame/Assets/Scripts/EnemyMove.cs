using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    public int nextMove;


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        Think();

        Invoke("Think", 5);
    }

    void FixedUpdate()
    {
        // 이동하기
        rigid.velocity = new Vector2(nextMove,rigid.velocity.y);

        // 지형 체크
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove,rigid.position.y);
    }

    void Think()
    {
        nextMove = Random.Range(-1, 2);

        Invoke("Think", 5);
    }
}
