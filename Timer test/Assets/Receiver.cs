using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;


public abstract class IReceiver : MonoBehaviour 
{ 
    public abstract float[] GetCoord();
} 

public class Receiver : IReceiver
{
    private UdpClient m_Receiver;
    public int m_Port = 12345;
    public string m_ReceiveMessage;
    public float[] coord;

    void Awake()
    {
        InitReceiver();
        coord = new float[408];
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

        for (int i = 0; i < 408; i++) {
            coord[i] = float.Parse(str[i]);
        }
    }

    void CloseReceiver()
    {
        if (m_Receiver != null)
        {
            m_Receiver.Close();
            m_Receiver = null;
        }
    }

    public override float[] GetCoord()
    {
        return coord;
    }
}
