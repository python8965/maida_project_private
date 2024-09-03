using UnityEngine;

public class HeadMovement : MonoBehaviour
{
    public Transform[] spheres; // 눈, 코, 귀의 위치를 나타내는 스피어들
    public Transform head; // 머리를 나타내는 타원형 스피어

    void Update()
    {
        if (spheres.Length != 5)
        {
            Debug.LogError("스피어의 수가 5개가 아닙니다.");
            return;
        }

        // 각 스피어의 위치 가져오기
        Vector3 eye1 = spheres[0].position;
        Vector3 eye2 = spheres[1].position;
        Vector3 nose = spheres[2].position;
        Vector3 ear1 = spheres[3].position;
        Vector3 ear2 = spheres[4].position;

        // 머리의 위치 계산 (모든 스피어의 평균 위치)
        Vector3 headPosition = (eye1 + eye2 + nose + ear1 + ear2) / 5.0f;
        head.position = headPosition;

        // 머리의 방향 계산 (여기서는 단순히 눈의 중간과 코를 기준으로 계산)
        Vector3 eyeCenter = (eye1 + eye2) / 2.0f;
        Vector3 forward = (nose - eyeCenter).normalized;
        Vector3 up = Vector3.Cross((ear1 - ear2).normalized, forward).normalized;

        // 머리의 회전 적용
        Quaternion headRotation = Quaternion.LookRotation(forward, up);
        head.rotation = headRotation;
    }
}