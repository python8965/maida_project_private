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
    public int InterpolateSize = 5;
    public int frame = 0;
    
    private List<float[]> queue;
    private float[] coord;
    
    private bool isStablized = false;

    void Awake()
    {
        BaseReceiver.OnReceive += ReceiveCallback;
        
        
        coord = new float[408];
        queue = new List<float[]>();

        for (int i = 0; i < 10; i++)
        {
            queue.Add(new float[408]);
        }
    }

    void ReceiveCallback()
    {
        if (BaseReceiver.m_ReceiveMessage.Equals("End"))
        {
            isStablized = false;
            frame = 0;
            return;
        }

        coord = GetBaseCoord();

        for (int i =  InterpolateSize-1; i >= 0; i--)
        {
            queue[i + 1] = queue[i];
        }
        
        queue[0] = coord.Clone() as float[];


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
            
            Debug.Log(count);

            coord[i * 3] = result.x;
            coord[i * 3+1] = result.y;
            coord[i * 3+2] = result.z;
            
        }

        isStablized = true;

        frame++;
    }

    public override float[] GetCoord()
    {
        return coord;
    }
}
