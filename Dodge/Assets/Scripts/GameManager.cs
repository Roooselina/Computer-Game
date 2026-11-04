using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 생존 시간(점수) 측정, 게임오버, 최고 기록 저장 및 표시
public class GameManager : MonoBehaviour
{
    public const string HighScoreKey = "BestTime"; // 최고 기록 저장 키 (PlayerPrefs)

    public GameObject gameoverText; // 게임오버 시 활성화할 텍스트 오브젝트
    public Text timeText; // 생존 시간 표시 텍스트
    public Text recordText; // 최고 기록 표시 텍스트

    // 최고 점수 UI (선택)
    public Text scoreText; // 게임오버 화면의 현재 점수 텍스트
    public Text highScoreText; // 플레이 중 화면의 최고 점수 텍스트
    public string scoreLabel = "SCORE: ";
    public string highScoreLabel = "HIGH SCORE: ";

    private float surviveTime; // 생존 시간
    private bool isGameover; // 게임오버 상태
    private float bestTime; // 저장된 최고 기록

    public float CurrentScore { get { return surviveTime; } } // 현재 점수 (생존 시간)
    public float HighScore { get { return Mathf.Max(bestTime, surviveTime); } } // 현재 판까지 반영한 최고 점수
    public bool IsGameover { get { return isGameover; } }

    // 다른 씬(메인 메뉴 등)에서도 저장된 최고 점수를 가져올 수 있도록 제공
    public static float GetSavedHighScore()
    {
        float best = PlayerPrefs.GetFloat(HighScoreKey, 0f);
        List<float> records = RecordStore.Load();
        return records.Count > 0 ? Mathf.Max(best, records[0]) : best;
    }

    void Start()
    {
        // 일시정지 상태에서 넘어온 경우를 대비해 시간 배율 초기화
        Time.timeScale = 1f;

        surviveTime = 0;
        isGameover = false;
        bestTime = GetSavedHighScore();
        UpdateHighScoreHUD();
    }

    void Update()
    {
        if (!isGameover)
        {
            surviveTime += Time.deltaTime;
            if (timeText != null) timeText.text = "Time: " + (int)surviveTime;
            // 화면의 최고 점수만 갱신 (저장은 게임오버 시)
            UpdateHighScoreHUD();
        }
        else
        {
            // 게임오버 상태에서 R 키로 현재 씬 재시작
            if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    // 게임오버 처리: 기록 저장 및 결과 표시
    public void EndGame()
    {
        if (isGameover) return;

        isGameover = true;
        if (gameoverText != null) gameoverText.SetActive(true);

        // 최고 기록 갱신 시 저장 (게임을 다시 실행해도 유지)
        bool isNewRecord = surviveTime > bestTime;
        if (isNewRecord)
        {
            bestTime = surviveTime;
            PlayerPrefs.SetFloat(HighScoreKey, bestTime);
            PlayerPrefs.Save();
        }

        // Top 3 기록에 등록 (순위 밖이면 -1)
        int rank = RecordStore.Submit(surviveTime);

        // 게임오버 화면에 현재 점수와 최고 점수를 구분해서 표시
        if (scoreText != null) scoreText.text = scoreLabel + (int)surviveTime + (rank >= 0 ? "  (TOP " + (rank + 1) + ")" : "");
        if (recordText != null)
        {
            recordText.text = highScoreLabel + (int)bestTime + (isNewRecord ? "  NEW!" : "");
        }
        UpdateHighScoreHUD();
    }

    private void UpdateHighScoreHUD()
    {
        if (highScoreText != null) highScoreText.text = highScoreLabel + (int)HighScore;
    }
}
