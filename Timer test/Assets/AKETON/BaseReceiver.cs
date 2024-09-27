using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class BaseReceiver : MonoBehaviour
{
    private UdpClient m_Receiver;
    public int m_Port = 12345;
    public string m_ReceiveMessage;
    public Vector3[] baseCoord;

    public Action OnReceive;
    public Action OnFinishReceive;
    
    bool isFinished;


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

                OnReceive?.Invoke();
            }
        }
        catch (SocketException e)
        {
            Debug.Log(e.Message);
        }

    }

    
}