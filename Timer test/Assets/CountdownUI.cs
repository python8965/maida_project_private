using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    public float startTime = 60f; // 타이머 시작 시간 (초 단위)
    private float timeRemaining;
    private bool timerIsRunning = false;
    public Text timerText; // UI 텍스트 컴포넌트
    public Button startButton; // 시작 버튼

    void Start()
    {
        timeRemaining = startTime;
        UpdateTimerText();

        // 버튼 클릭 이벤트 리스너 추가
        startButton.onClick.AddListener(ToggleTimer);
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerText();
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                // 타이머가 끝났을 때 할 작업 추가
            }
        }
    }

    void ToggleTimer()
    {
        if (timerIsRunning)
        {
            // 타이머가 실행 중일 때 버튼을 누르면 타이머를 리셋
            ResetTimer();
        }
        else
        {
            // 타이머가 실행 중이 아닐 때 버튼을 누르면 타이머를 시작
            StartTimer();
        }
    }

    void StartTimer()
    {
        timerIsRunning = true;
    }

    void ResetTimer()
    {
        timeRemaining = startTime;
        UpdateTimerText();
        timerIsRunning = false;
    }

    void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
