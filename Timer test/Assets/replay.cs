using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CameraRecorder : MonoBehaviour
{
    public Camera mainCamera; // 메인 카메라
    public RenderTexture screenCapture; // 캡처할 RenderTexture
    public RawImage displayImage; // 캡처된 화면을 표시할 RawImage
    public Button recordButton; // 녹화를 시작하고 중단할 버튼
    public Button togglePlaybackButton; // 재생 및 일시정지를 위한 버튼
    public Button resetButton; // 녹화본을 지우고 처음으로 돌아가는 버튼
    public Slider playBar; // 플레이 바
    public float captureInterval = 0.1f; // 화면을 캡처할 간격 (초 단위)
    public int maxFrames = 300; // 최대 프레임 수 (녹화 시간 제한)

    public GameObject imageA; // Play 상태에서 활성화할 이미지 A
    public GameObject imageB; // Pause 상태에서 활성화할 이미지 B

    private Texture2D[] frames; // 캡처된 프레임을 저장할 배열
    private int frameCount = 0; // 현재 저장된 프레임 수
    private bool isRecording = false; // 녹화 상태
    private bool isReplaying = false; // 리플레이 상태
    private bool isPaused = false; // 일시정지 상태
    private float timer = 0f; // 캡처 간격 타이머
    private Coroutine replayCoroutine; // 리플레이 코루틴
    private int currentFrame = 0; // 현재 재생 프레임 인덱스
    private Text recordButtonText; // 녹화 버튼의 텍스트 컴포넌트
    private Text togglePlaybackButtonText; // 재생/일시정지 버튼의 텍스트 컴포넌트
    private Color originalColor; // 버튼의 원래 색상

    void Start()
    {
        // 프레임 배열 초기화
        frames = new Texture2D[maxFrames];

        // 버튼 클릭 이벤트 리스너 추가
        recordButton.onClick.AddListener(ToggleRecording);
        togglePlaybackButton.onClick.AddListener(TogglePlayback);
        resetButton.onClick.AddListener(ResetRecording);
        playBar.onValueChanged.AddListener(OnPlayBarValueChanged);
        displayImage.gameObject.SetActive(false);

        // 카메라의 TargetTexture 설정
        if (mainCamera != null)
        {
            mainCamera.targetTexture = screenCapture;
        }

        // 버튼의 텍스트 컴포넌트 찾기
        recordButtonText = recordButton.GetComponentInChildren<Text>();
        togglePlaybackButtonText = togglePlaybackButton.GetComponentInChildren<Text>();

        // 초기 텍스트 설정
        recordButtonText.text = "Start Set";
        togglePlaybackButtonText.text = "Play";

        // 버튼의 원래 색상 저장
        originalColor = recordButton.GetComponent<Image>().color;

        // 초기 이미지 상태 설정
        imageA.SetActive(false);
        imageB.SetActive(true);

        // 슬라이더 항상 활성화
        playBar.gameObject.SetActive(true);
    }

    void Update()
    {
        if (isRecording)
        {
            timer += Time.deltaTime;

            if (timer >= captureInterval)
            {
                CaptureFrame();
                timer = 0f;
            }
        }
    }

    void CaptureFrame()
    {
        // RenderTexture에서 화면 캡처
        RenderTexture.active = screenCapture;
        if (frames[frameCount] == null)
        {
            frames[frameCount] = new Texture2D(screenCapture.width, screenCapture.height, TextureFormat.RGBA32, false);
        }
        frames[frameCount].ReadPixels(new Rect(0, 0, screenCapture.width, screenCapture.height), 0, 0);
        frames[frameCount].Apply();
        RenderTexture.active = null;

        frameCount++;
        if (frameCount >= maxFrames)
        {
            frameCount = maxFrames - 1; // 프레임 수 제한
        }
    }

    void ToggleRecording()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    void StartRecording()
    {
        isRecording = true;
        frameCount = 0; // 프레임 수 초기화
        displayImage.gameObject.SetActive(false); // 리플레이 화면을 숨깁니다
        timer = 0f; // 타이머 초기화

        // 텍스트 및 버튼 색상 업데이트
        recordButtonText.text = "Stop Set";
        recordButton.GetComponent<Image>().color = Color.red; // 빨간색으로 변경
    }

    void StopRecording()
    {
        isRecording = false;

        // 텍스트 및 버튼 색상 업데이트
        recordButtonText.text = "Start Set";
        recordButton.GetComponent<Image>().color = originalColor; // 원래 색상으로 변경
    }

    void TogglePlayback()
    {
        if (isReplaying)
        {
            if (isPaused)
            {
                ResumePlayback();
            }
            else
            {
                PausePlayback();
            }
        }
        else
        {
            StartReplay();
        }
    }

    void StartReplay()
    {
        if (replayCoroutine != null)
        {
            StopCoroutine(replayCoroutine);
        }
        isRecording = false;
        isReplaying = true;
        isPaused = false;
        displayImage.gameObject.SetActive(true);
        playBar.maxValue = frameCount - 1;
        playBar.value = 0;
        mainCamera.enabled = false; // 메인 카메라 비활성화
        replayCoroutine = StartCoroutine(PlayBackFrames());
        togglePlaybackButtonText.text = "Pause";
        
        // 이미지 A 활성화, 이미지 B 비활성화
        imageA.SetActive(true);
        imageB.SetActive(false);
    }

    void PausePlayback()
    {
        isPaused = true;
        togglePlaybackButtonText.text = "Play";

        // 이미지 A 비활성화, 이미지 B 활성화
        imageA.SetActive(false);
        imageB.SetActive(true);
    }

    void ResumePlayback()
    {
        isPaused = false;
        togglePlaybackButtonText.text = "Pause";

        // 이미지 A 활성화, 이미지 B 비활성화
        imageA.SetActive(true);
        imageB.SetActive(false);
    }

    IEnumerator PlayBackFrames()
    {
        while (isReplaying)
        {
            if (!isPaused)
            {
                displayImage.texture = frames[currentFrame];
                playBar.value = currentFrame;
                currentFrame++;
                if (currentFrame >= frameCount)
                {
                    currentFrame = frameCount - 1; // 마지막 프레임에서 멈추기
                    isReplaying = false; // 재생 상태 종료
                    togglePlaybackButtonText.text = "Play";

                    // 이미지 A 활성화, 이미지 B 비활성화
                    imageA.SetActive(true);
                    imageB.SetActive(false);
                }
                else
                {
                    yield return new WaitForSeconds(captureInterval);
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    void OnPlayBarValueChanged(float value)
    {
        if (!isReplaying) return;
        currentFrame = Mathf.RoundToInt(value);
        if (currentFrame >= 0 && currentFrame < frameCount)
        {
            displayImage.texture = frames[currentFrame];
        }
    }

    void ResetRecording()
    {
        // 녹화 중지 및 초기화
        isRecording = false;
        isReplaying = false;
        isPaused = false;

        // 저장된 프레임 삭제
        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i] != null)
            {
                Destroy(frames[i]);
                frames[i] = null;
            }
        }

        frameCount = 0; // 프레임 수 초기화

        // UI 초기화
        displayImage.gameObject.SetActive(false);
        playBar.value = 0;
        currentFrame = 0; // 현재 프레임을 초기화

        // 메인 카메라 활성화
        mainCamera.enabled = true;

        // 버튼 텍스트 초기화
        recordButtonText.text = "Start Set";
        togglePlaybackButtonText.text = "Play";

        // 버튼 색상 초기화
        recordButton.GetComponent<Image>().color = originalColor;

        // 초기 이미지 상태 설정
        imageA.SetActive(false);
        imageB.SetActive(true);

        // 리플레이 코루틴 중지
        if (replayCoroutine != null)
        {
            StopCoroutine(replayCoroutine);
        }
    }
}
