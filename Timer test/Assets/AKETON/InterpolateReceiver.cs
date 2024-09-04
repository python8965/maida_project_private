using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;


public class InterpolateReceiver : IReceiver
{
    private UdpClient m_Receiver;
    public int m_Port = 12345;
    public string m_ReceiveMessage;
    public int InterpolateSize = 5;
    
    public float[] coord;
    private List<float[]> queue;
    
    private bool isStablized = false;

    void Awake()
    {
        InitReceiver();
        coord = new float[408];
        queue = new List<float[]>();

        for (int i = 0; i < 10; i++)
        {
            queue.Add(new float[408]);
        }
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

        if (m_ReceiveMessage.Equals("End"))
        {
            isStablized = false;
            return;
        }
        
        m_ReceiveMessage = m_ReceiveMessage.Replace("[", "").Replace("]", "");


        string[] str = m_ReceiveMessage.Split(',');

        float[] localcoord = new float[408];
        for (int i = 0; i < 408; i++)
        {
            localcoord[i] = float.Parse(str[i]);
        }


        for (int i =  InterpolateSize-1; i >= 0; i--)
        {
            queue[i + 1] = queue[i];
        }
        
        queue[0] = localcoord.Clone() as float[];


        for (int i = 0; i < 408 / 3; i++)
        {
            int count = 1;
            
            Vector3 current = new Vector3(queue[0][i * 3], queue[0][i * 3 + 1], queue[0][i * 3 + 2]);
            Vector3[] values = new Vector3[InterpolateSize-1];
            Vector3[] delta = new Vector3[InterpolateSize-1];
            
            for (int j = 1; j <  InterpolateSize - 1  ; j++)
            {
                
                var value = new Vector3(queue[j][i * 3], queue[j+1][i * 3 + 1], queue[j+2][i * 3 + 2]);
                
                count += 1;
                values[j - 1] = value;
            }
            
            Vector3 currentDelta = values[0] - current;

            if (i == 10)
            {
                
            }
            //Debug.Log(currentDelta.magnitude);
            if (currentDelta.magnitude > 100.0f && isStablized)
            { // revert
                queue[0][i * 3] = queue[1][i * 3];
                queue[0][i * 3 + 1] = queue[1][i * 3 + 1];
                queue[0][i * 3 + 2] = queue[1][i * 3 + 2];
            }

            Vector3 sum = current;

            for (int j = 0; j < count - 1 ; j++)
            {
                sum += values[j];
            }
            
            var median = sum / count;

            var result = median ;
            
            //Debug.Log(count);

            coord[i * 3] = result.x;
            coord[i * 3+1] = result.y;
            coord[i * 3+2] = result.z;
            
        }

        isStablized = true;
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
