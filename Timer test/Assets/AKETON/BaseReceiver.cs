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


    void Awake()
    {
        baseCoord = new Vector3[Helpers.CoordVectorSize];
        Receive();
    }

    async void Receive()
    {
        try
        {
            using var udpClient = new UdpClient(m_Port);
            
            while (true)
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
                    baseCoord[i] = new Vector3(float.Parse(str[i * 3]), float.Parse(str[i * 3 + 1]),
                        float.Parse(str[i * 3 + 2]));
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