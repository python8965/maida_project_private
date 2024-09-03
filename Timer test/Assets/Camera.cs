using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 10.0f; // 회전 속도
    public float distance = 50.0f; // 중심점으로부터의 거리

    // 첫 번째 세트 버튼
    public Button resetButton1;
    public Button rightViewButton1;
    public Button leftViewButton1;
    public Button oppositeViewButton1;

    private Vector2 lastTouchPosition;
    private bool isTouching;
    private bool isDragging;

    void Start()
    {
        // 초기 위치 설정
        transform.position = new Vector3(0.0f, 0.0f, -distance);
        transform.LookAt(Vector3.zero); // 초기 방향 설정

        // 첫 번째 세트 버튼 클릭 이벤트 등록
        if (resetButton1 != null)
        {
            resetButton1.onClick.AddListener(ResetCameraPosition);
        }
        if (rightViewButton1 != null)
        {
            rightViewButton1.onClick.AddListener(ViewFromRight);
        }
        if (leftViewButton1 != null)
        {
            leftViewButton1.onClick.AddListener(ViewFromLeft);
        }
        if (oppositeViewButton1 != null)
        {
            oppositeViewButton1.onClick.AddListener(ViewFromOpposite);
        }
    }

    void Update()
    {
        HandleTouchInput();
        HandleMouseInput();
        HandleKeyboardInput();

        // 거리 유지
        Vector3 direction = (transform.position - Vector3.zero).normalized;
        transform.position = direction * distance;
        transform.LookAt(Vector3.zero); // 중심점 향하게 설정
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
                isTouching = true;
            }
            else if (touch.phase == TouchPhase.Moved && isTouching)
            {
                Vector2 deltaPosition = touch.deltaPosition;
                float rotationY = -deltaPosition.x * rotationSpeed * Time.deltaTime;

                // 좌우 회전
                transform.RotateAround(Vector3.zero, Vector3.up, rotationY);

                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isTouching = false;
            }
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastTouchPosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 deltaPosition = currentMousePosition - lastTouchPosition;
            float rotationY = deltaPosition.x * rotationSpeed * Time.deltaTime;

            // 좌우 회전
            transform.RotateAround(Vector3.zero, Vector3.up, rotationY);

            lastTouchPosition = currentMousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void HandleKeyboardInput()
    {
        // 좌우 회전
        if (Input.GetKey(KeyCode.A))
        {
            transform.RotateAround(Vector3.zero, Vector3.up, rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.RotateAround(Vector3.zero, Vector3.up, -rotationSpeed * Time.deltaTime);
        }
    }

    public void ResetCameraPosition()
    {
        transform.position = new Vector3(0.0f, 0.0f, -distance);
        transform.LookAt(Vector3.zero); // 초기 방향 설정
    }

    public void ViewFromRight()
    {
        transform.position = new Vector3(distance, 0.0f, 0.0f);
        transform.LookAt(Vector3.zero);
    }

    public void ViewFromLeft()
    {
        transform.position = new Vector3(-distance, 0.0f, 0.0f);
        transform.LookAt(Vector3.zero);
    }

    public void ViewFromOpposite()
    {
        transform.position = new Vector3(0.0f, 0.0f, distance);
        transform.LookAt(Vector3.zero);
    }
}
