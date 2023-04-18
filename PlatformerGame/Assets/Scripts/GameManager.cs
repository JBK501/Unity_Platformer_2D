using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove player;
    public GameObject[] stages;

    public Image[] UIhealth;
    public Text UIPoint;
    public Text UIStage;
    public GameObject UIRestartBtn;

    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }

    public void NextStage()
    {
        // 스테이지를 변경한다.
        if(stageIndex < stages.Length - 1)
        {
            stages[stageIndex].SetActive(false);
            stageIndex++;
            stages[stageIndex].SetActive(true);
            PlayerReposition();

            UIStage.text = "STAGE " + (stageIndex + 1);
        }
        else // 게임 클리어 시
        {
            // Player Control Rock
            Time.timeScale = 0;

            // 결과 표시
            Debug.Log("게임 클리어.");

            // 재시작 버튼
            Text btnText = UIRestartBtn.GetComponentInChildren<Text>();
            btnText.text = "Clear!";
            UIRestartBtn.SetActive(true);
        }
        
        // 점수를 계산한다.
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown()
    {
        if (health > 1)
        {
            health--;
            UIhealth[health].color = new Color(1, 1, 1, 0.2f);
        }
        else
        {
            // 모든 체력 UI OFF처리
            UIhealth[0].color = new Color(1, 0, 0, 0.4f);

            // 플레이어 사망 처리
            player.OnDie();

            // 결과 UI표시
            Debug.Log("죽었습니다.");

            // 재도전 버튼 UI표시
            UIRestartBtn.SetActive(true);
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
        if(stageIndex == 0)
        {
            player.transform.position = new Vector3(-3, 2, -1);
        }
        else
            player.transform.position = new Vector3(0, 0, -1);

        player.VelocityZero();
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
