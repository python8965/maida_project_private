using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        InitReceiver();
        baseCoord = new Vector3[Helpers.CoordVectorSize];
    }

    void OnApplicationQuit()
    {
        CloseReceiver();
    }

    void InitReceiver()
    {
        try
        {
            if (m_Receiver == null)
            {
                m_Receiver = new UdpClient(m_Port);
                m_Receiver.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            }
        }
        catch (SocketException e)
        {
            Debug.Log(e.Message);
        }
    }

    void ReceiveCallback(IAsyncResult ar)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, m_Port);
        byte[] received = m_Receiver.EndReceive(ar, ref ipEndPoint);
        m_Receiver.BeginReceive(new AsyncCallback(ReceiveCallback), null);

        m_ReceiveMessage = Encoding.Default.GetString(received).Trim();
        m_ReceiveMessage = m_ReceiveMessage.Replace("[", "").Replace("]", "");
        
        
        string[] str = m_ReceiveMessage.Split(',');

        for (int i = 0; i < Helpers.CoordVectorSize; i++) {
            baseCoord[i] = new Vector3(float.Parse(str[i * 3]), float.Parse(str[i * 3 + 1]), float.Parse(str[i * 3 + 2]));
        }

        OnReceive?.Invoke();
    }

    void CloseReceiver()
    {
        if (m_Receiver != null)
        {
            m_Receiver.Close();
            m_Receiver = null;
        }
    }
    
    
}