using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove player;
    public GameObject[] stages;

    public void NextStage()
    {
        // 스테이지를 변경한다.
        if(stageIndex < stages.Length - 1)
        {
            stages[stageIndex].SetActive(false);
            stageIndex++;
            stages[stageIndex].SetActive(true);
            PlayerReposition();
        }
        else
        {
            Time.timeScale = 0;

            Debug.Log("게임 클리어.");
        }
        
        // 점수를 계산한다.
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown()
    {
        if (health > 1)
            health--;
        else
        {
            // 플레이어 사망 처리
            player.OnDie();

            // 결과 UI표시
            Debug.Log("죽었습니다.");

            // 재도전 버튼 UI표시
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if(health > 1)  // 체력이 남을 시
            {
                // 플레이어 위치 복구
                PlayerReposition();
            }

            // 체력 깎기
            HealthDown();
        }
    }

    void PlayerReposition()
    {
        player.transform.position = new Vector3(0, 0, -1);
        player.VelocityZero();
    }
}
