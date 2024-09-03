using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DistanceChecker : MonoBehaviour
{
    public Transform sphere1; // 첫 번째 스피어
    public Transform sphere2; // 두 번째 스피어
    public Transform referenceSphere1; // 최대 거리 임계값을 설정할 첫 번째 참조 스피어
    public Transform referenceSphere2; // 최대 거리 임계값을 설정할 두 번째 참조 스피어
    public Transform cylinder1; // 첫 번째 실린더
    public Transform cylinder2; // 두 번째 실린더
    public Text statusText; // 상태 메시지를 표시할 Text
    public Color normalColor = Color.white; // 정상 상태 색상
    public Color alertColor = Color.red; // 경고 상태 색상

    private Renderer cylinder1Renderer; // 첫 번째 실린더의 렌더러
    private Renderer cylinder2Renderer; // 두 번째 실린더의 렌더러

    void Start()
    {
        // 렌더러 컴포넌트를 가져옵니다.
        cylinder1Renderer = cylinder1.GetComponent<Renderer>();
        cylinder2Renderer = cylinder2.GetComponent<Renderer>();

        // 초기 색상을 설정합니다.
        SetCylindersColor(normalColor);
    }

    void Update()
    {
        // 두 스피어 사이의 거리를 계산
        float distance = Vector3.Distance(sphere1.position, sphere2.position);

        // 최대 거리 임계값을 참조 스피어 사이의 거리로 설정
        float referenceDistance = Vector3.Distance(referenceSphere1.position, referenceSphere2.position);
        float maxDistanceThreshold = referenceDistance;
        float minDistanceThreshold = referenceDistance * 0.5f;

        // 거리가 maxDistanceThreshold보다 큰 경우
        if (distance > maxDistanceThreshold)
        {
            statusText.text = "WIDE";
            SetCylindersColor(alertColor);
        }
        // 거리가 minDistanceThreshold보다 작은 경우
        else if (distance < minDistanceThreshold)
        {
            statusText.text = "NARROW";
            SetCylindersColor(alertColor);
        }
        else
        {
            statusText.text = "";
            SetCylindersColor(normalColor);
        }
    }

    void SetCylindersColor(Color color)
    {
        cylinder1Renderer.material.color = color;
        cylinder2Renderer.material.color = color;
    }
}
