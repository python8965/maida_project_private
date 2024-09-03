using UnityEngine;
using UnityEngine.UI;

public class Gauge : MonoBehaviour
{
    public Slider gaugeSlider; // UI 슬라이더 컴포넌트
    public float fillTime = 10f; // 게이지가 채워지는 데 걸리는 시간 (초 단위)
    private float elapsedTime = 0f; // 경과 시간
    private bool isFilling = true; // 게이지가 채워지고 있는지 여부
    private bool isPaused = false; // 게이지가 멈춰 있는지 여부
    public Button controlButton; // 제어 버튼

    void Start()
    {
        gaugeSlider.value = 0f; // 게이지 초기화
        StartFilling(); // 게이지 채우기 시작

        // 버튼 클릭 이벤트 리스너 추가
        controlButton.onClick.AddListener(TogglePause);
    }

    void Update()
    {
        if (isPaused)
        {
            return;
        }

        if (isFilling)
        {
            elapsedTime += Time.deltaTime;
            gaugeSlider.value = Mathf.Clamp01(elapsedTime / fillTime);

            if (elapsedTime >= fillTime)
            {
                StartDraining();
            }
        }
        else
        {
            elapsedTime -= Time.deltaTime;
            gaugeSlider.value = Mathf.Clamp01(elapsedTime / fillTime);

            if (elapsedTime <= 0)
            {
                StartFilling();
            }
        }
    }

    void StartFilling()
    {
        elapsedTime = 0f;
        isFilling = true;
    }

    void StartDraining()
    {
        elapsedTime = fillTime;
        isFilling = false;
    }

    void TogglePause()
    {
        isPaused = !isPaused;
    }
}
