using UnityEngine;
using UnityEngine.UI;

public class UIObjectOverlapTrigger : MonoBehaviour
{
    public GameObject targetObject; // 체크할 게임 오브젝트
    public RectTransform targetUI; // 체크할 UI 요소
    public float requiredTime = 2.0f; // 필요한 시간
    public Button targetButton; // 트리거할 버튼

    public Color overlapColor = Color.red; // 겹치는 동안 사용할 색상
    private Color originalColor; // 원래 색상

    private Renderer objectRenderer; // 오브젝트의 렌더러
    private float timeStayed = 0.0f; // 머무른 시간 추적

    void Start()
    {
        objectRenderer = targetObject.GetComponent<Renderer>();
        originalColor = objectRenderer.material.color; // 원래 색상 저장
    }

    void Update()
    {
        if (IsOverlapping(targetUI, targetObject))
        {
            timeStayed += Time.deltaTime;
            objectRenderer.material.color = overlapColor; // 색상 변경

            if (timeStayed >= requiredTime)
            {
                targetButton.onClick.Invoke();
                timeStayed = 0.0f;
            }
        }
        else
        {
            timeStayed = 0.0f;
            objectRenderer.material.color = originalColor; // 색상 복원
        }
    }

    bool IsOverlapping(RectTransform uiElement, GameObject obj)
    {
        Vector3[] uiCorners = new Vector3[4];
        uiElement.GetWorldCorners(uiCorners);

        Vector3 objPosition = obj.transform.position;
        Vector2 objScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, objPosition);

        Rect uiRect = new Rect(uiCorners[0].x, uiCorners[0].y, uiCorners[2].x - uiCorners[0].x, uiCorners[2].y - uiCorners[0].y);
        return uiRect.Contains(objScreenPosition);
    }
}
