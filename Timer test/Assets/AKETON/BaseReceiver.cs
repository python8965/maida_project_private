using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


[System.Serializable]
public enum ReceiverMode
{
    Raw,
    Transformed,
    Scaled,
    TransformAndScaled
}

public class BaseReceiver : MonoBehaviour
{
    private UdpClient m_Receiver;
    public int m_Port = 12345;
    public string m_ReceiveMessage;
    public Vector3[] baseCoord;

    public Action OnReceive;
    public Action OnFinishReceive;
    
    public ReceiverMode mode = ReceiverMode.TransformAndScaled;
    
    bool isFinished;
    
    private Vector3 floorTransform;
    public float receivedScale = 18.0f;
    
    void Awake()
    {
        baseCoord = new Vector3[Helpers.CoordVectorSize];
        Receive();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Application Quit");
        isFinished = true;
        baseCoord = new Vector3[Helpers.CoordVectorSize];
        
        OnFinishReceive?.Invoke();
    }
    
    
    public Vector3 TransformReceivedPosition(Vector3 V) //원래 있던 함수입니다.
    {
        var PointB = Helpers.PointB;
        
        var Position = (V - baseCoord[10]) / 50 + PointB;
        
        
        Vector3 pointA = (baseCoord[134] - baseCoord[135]) / 50 + PointB;
        Vector3 pointC = new Vector3(0, 0, 0);
        
        // 두 벡터 계산
        Vector3 vector1 = pointA - PointB;
        Vector3 vector2 = pointC - PointB;

        // 두 벡터 사이의 각도 계산
        float angle = Vector3.Angle(vector1, vector2);

        // 평면 위의 회전 축 계산 (pointA, pointB, pointC가 이루는 평면의 법선 벡터)
        Vector3 rotationAxis = Vector3.Cross(vector1, vector2).normalized;

        // 기준점에서 평면 위의 각도만큼 회전 이동
        Vector3 position = Position;
        Vector3 vector3 = Quaternion.AngleAxis(angle, rotationAxis) * (position - PointB);
        Position = PointB + vector3;
        Position = Quaternion.AngleAxis(angle * ((float)Math.PI / 180f), rotationAxis) * Position;
            
            
        return Position;
    }

    async void Receive()
    {
        
        isFinished = false;
        
        try
        {
            using var udpClient = new UdpClient(m_Port);
            //using var udpClient = new UdpClient("localhost", m_Port);
            while (!isFinished)
            {
                var receivedResult = await udpClient.ReceiveAsync();

                
                
                m_ReceiveMessage = Encoding.Default.GetString(receivedResult.Buffer).Trim();


                if (m_ReceiveMessage.Equals("End"))
                {
                    continue;
                }
                
                m_ReceiveMessage = m_ReceiveMessage.Replace("[", "").Replace("]", "");


                string[] str = m_ReceiveMessage.Split(',');
                

                for (int i = 0; i < Helpers.CoordVectorSize; i++)
                {
                    try
                    {
                        baseCoord[i] = new Vector3(float.Parse(str[i * 3]), float.Parse(str[i * 3 + 1]),
                            float.Parse(str[i * 3 + 2]));
                    }catch (Exception e)
                    {
                        Debug.Log(e);
                        Debug.Log("error index is"  + i);
                    }
                }
                
                var newBaseCoord = new Vector3[Helpers.CoordVectorSize];
                
                if (mode == ReceiverMode.Transformed || mode == ReceiverMode.TransformAndScaled)
                {

                    for (int i = 0; i < Helpers.CoordVectorSize; i++)
                    {
                        newBaseCoord[i] = TransformReceivedPosition(baseCoord[i]);
                    }

                    baseCoord = newBaseCoord;
                }
                
                if (mode == ReceiverMode.TransformAndScaled || mode == ReceiverMode.Scaled)
                {

                    var foot = baseCoord[13];
                    var resizedFoot = foot / receivedScale;
                    floorTransform = resizedFoot;

                    //divideFactor = thighDistance / boneDistance / 1.1f;

                    for (int i = 0; i < Helpers.CoordVectorSize; i++)
                    {
                        baseCoord[i] = baseCoord[i] / receivedScale - floorTransform;
                    }
                }

                OnReceive?.Invoke();
            }
        }
        catch (SocketException e)
        {
            Debug.Log(e.Message);
        }

    }

    
}