using UnityEngine;
using UnityEngine.UI;

public class Jointangleshoulderpress : MonoBehaviour
{
    public Transform sphereA; 
    public Transform sphereB; 
    public Transform sphereC; 
    public Slider angleSlider; 
    public Text countText; 
    public Button toggleButton; 
    public float maxAngle = 180f; 

    private bool isCounting = false; 
    private int count = 0; 
    private bool hasReachedMax = false; 

    void Start()
    {
        angleSlider.maxValue = maxAngle;
        toggleButton.onClick.AddListener(ToggleCounting);
        UpdateCountText();
        UpdateButtonText();
    }

    void Update()
    {
        if (isCounting)
        {
            Vector3 vectorAB = sphereB.position - sphereA.position;
            Vector3 vectorBC = sphereC.position - sphereB.position;
            float angle = Vector3.Angle(vectorAB, vectorBC);
            angleSlider.value = Mathf.Clamp(angle, 0, maxAngle);
            if (angleSlider.value >= maxAngle)
            {
                if (!hasReachedMax)
                {
                    count++;
                    UpdateCountText();
                    hasReachedMax = true; 
                }
            }
            else
            {
                hasReachedMax = false; 
            }
        }
    }

    void ToggleCounting()
    {
        isCounting = !isCounting;
        if (!isCounting)
        {
            angleSlider.value = 0; // 작동을 멈추면 게이지 값을 0으로 설정
        }
        if (isCounting)
        {
            count = 0; // 카운트를 0으로 초기화
            UpdateCountText();
        }
        UpdateButtonText();
    }

    void UpdateCountText()
    {
        countText.text = " " + count;
    }

    void UpdateButtonText()
    {
        toggleButton.GetComponentInChildren<Text>().text = isCounting ? "Stop" : "Start";
    }
}
