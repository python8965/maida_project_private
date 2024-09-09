using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : MonoBehaviour
{
    public int cubeNum;
    public Vector3[] coord2;
    public Vector3 pointB = new Vector3(0, -20, 0);
    public Vector3 pointC = new Vector3(0, 0, 0);

    // Start is called before the first frame update
    void Start()
    {
        coord2 = GameObject.Find("Receiver").GetComponent<IReceiver>().GetCoord();
    }

    // Update is called once per frame
    void Update()
    {

        
        
        // 구체 위치 계산
        transform.position = (coord2[cubeNum] - coord2[10]) / 50 +Helpers.PointB; 

        
        // new Vector3(
        //     coord2[cubeNum * 3] / 50 - coord2[30] / 50,
        //     coord2[cubeNum * 3 + 1] / 50 - coord2[31] / 50 - 20,
        //     coord2[cubeNum * 3 + 2] / 50 - coord2[32] / 50
        // );
        
        
        // (A, B, C) 점 계산
        Vector3 pointA = coord2[134] / 50 - coord2[135] / 50 + Helpers.PointB;

        // 두 벡터 계산
        Vector3 vector1 = pointA - pointB;
        Vector3 vector2 = pointC - pointB;

        // 두 벡터 사이의 각도 계산
        float angle = Vector3.Angle(vector1, vector2);

        // 평면 위의 회전 축 계산 (pointA, pointB, pointC가 이루는 평면의 법선 벡터)
        Vector3 rotationAxis = Vector3.Cross(vector1, vector2).normalized;

        // 기준점에서 평면 위의 각도만큼 회전 이동
        transform.RotateAround(pointB, rotationAxis, angle);
    }
}
