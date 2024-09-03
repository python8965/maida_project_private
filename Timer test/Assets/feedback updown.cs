using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GaugeController : MonoBehaviour
{
    public Slider gaugeSlider; // 게이지를 표시할 Slider
    public Text statusText; // 상태 메시지를 표시할 Text
    public GameObject upImage; // UP 상태일 때 표시할 이미지
    public GameObject downImage; // DOWN 상태일 때 표시할 이미지
    public GameObject pauseImage; // PAUSE 상태일 때 표시할 이미지
    public float pauseTime = 1f; // 상태 변경 시 PAUSE 메시지를 표시할 시간

    private Coroutine stateCoroutine; // 현재 상태를 처리하는 코루틴
    private string currentState = ""; // 현재 상태 메시지
    private string targetState = ""; // 변경할 목표 상태
    private bool isChangingState = false; // 상태 변경 중인지 여부

    void Update()
    {
        // 게이지가 다 찼을 때
        if (gaugeSlider.value >= gaugeSlider.maxValue)
        {
            if (currentState != "Up")
            {
                ChangeState("Up");
            }
        }
        // 게이지가 20% 미만일 때
        else if (gaugeSlider.value <= gaugeSlider.maxValue * 0.2f)
        {
            if (currentState != "Down")
            {
                ChangeState("Down");
            }
        }
    }

    void ChangeState(string newState)
    {
        if (!isChangingState)
        {
            targetState = newState;
            if (stateCoroutine != null)
            {
                StopCoroutine(stateCoroutine);
            }
            stateCoroutine = StartCoroutine(ChangeStateCoroutine(newState));
        }
    }

    IEnumerator ChangeStateCoroutine(string newState)
    {
        isChangingState = true;

        if (!string.IsNullOrEmpty(newState))
        {
            // 현재 상태를 PAUSE로 설정
            statusText.text = "PAUSE";
            ShowOnlyImage(pauseImage);
            yield return new WaitForSeconds(pauseTime);
        }

        // PAUSE 상태 후 새로운 상태로 전환
        statusText.text = newState;
        currentState = newState;

        // 새로운 상태에 따른 이미지 설정
        if (newState == "Up")
        {
            ShowOnlyImage(upImage);
        }
        else if (newState == "Down")
        {
            ShowOnlyImage(downImage);
        }

        isChangingState = false;
    }

    void ShowOnlyImage(GameObject activeImage)
    {
        // 모든 이미지를 비활성화
        upImage.SetActive(false);
        downImage.SetActive(false);
        pauseImage.SetActive(false);

        // 활성화할 이미지만 활성화
        if (activeImage != null)
        {
            activeImage.SetActive(true);
        }
    }
}
