using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimerController : MonoBehaviour
{
    public Button startButton; // 타이머를 시작할 버튼
    public Text timerText; // 타이머를 표시할 텍스트
    public Text messageText; // 메시지를 표시할 텍스트
    public float timerDuration = 60f; // 타이머 지속 시간 (초 단위)
    public Canvas canvas; // UI 캔버스

    private float timer;
    private bool isTimerRunning = false;
    private int buttonClickCount = 0; // 버튼 클릭 횟수

    void Start()
    {
        // 버튼 클릭 이벤트 리스너 추가
        startButton.onClick.AddListener(OnButtonClick);
        messageText.gameObject.SetActive(false); // 초기 메시지 텍스트 숨기기
        timerText.gameObject.SetActive(false); // 초기 타이머 텍스트 숨기기
        UpdateTimerText(); // 초기 타이머 텍스트 설정
    }

    void Update()
    {
        if (isTimerRunning)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                isTimerRunning = false;
                TimerEnded();
            }
            UpdateTimerText();
        }
    }

    void OnButtonClick()
    {
        buttonClickCount++;
        if (buttonClickCount % 2 == 0) // 짝수번째 클릭일 때만 타이머 시작
        {
            StartTimer();
        }
        else // 홀수번째 클릭일 때 타이머 중지 및 초기화
        {
            StopTimer();
        }
    }

    void StartTimer()
    {
        timer = timerDuration;
        isTimerRunning = true;
        messageText.gameObject.SetActive(false); // 메시지 텍스트 숨기기
        timerText.gameObject.SetActive(true); // 타이머 텍스트 보이기
        UpdateTimerText();
    }

    void StopTimer()
    {
        isTimerRunning = false;
        timerText.gameObject.SetActive(false); // 타이머 텍스트 숨기기
        messageText.gameObject.SetActive(false); // 메시지 텍스트 숨기기
    }

    void UpdateTimerText()
    {
        if (timer <= 0 && !isTimerRunning)
        {
            timerText.gameObject.SetActive(false); // 타이머가 00:00일 때 숨기기
        }
        else
        {
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void TimerEnded()
    {
        messageText.text = "Breaktime over";
        messageText.color = Color.red; // 메시지 텍스트 색상을 붉은 색으로 변경
        messageText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(false); // 타이머 텍스트 숨기기
    }
}
