using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEditor;


public class InterpolateReceiver : IReceiver
{
    private UdpClient m_Receiver;
    public int InterpolateSize = 5;
    public int frame = 0;
    
    private  Queue<Vector3[]> queue;
    private Vector3[] coord;
    
    private bool isStablized = false;

    [DebugGUIGraph(min: -1, max: 1, r: 0, g: 1, b: 1, autoScale: true)]
    public double maxDeltaMove;

    void Awake()
    {
        BaseReceiver.OnReceive += ReceiveCallback;
        
        
        coord = new Vector3[Helpers.CoordVectorSize];
        queue = new Queue<Vector3[]>();
    }

    void ReceiveCallback()
    {
        if (BaseReceiver.m_ReceiveMessage.Equals("End"))
        {
            isStablized = false;
            frame = 0;
            return;
        }

        
        
        var baseCoord = GetBaseCoord();

        queue.Enqueue(baseCoord.Clone() as Vector3[]);
        if (queue.Count > InterpolateSize)
        {
            queue.Dequeue();
        }

        var arrayQueue = queue.ToArray();
        
        maxDeltaMove = 0.0;
        for (int i = 0; i < Helpers.CoordVectorSize; i++)
        {
            int count = 1;

            Vector3 current = arrayQueue[0][i];
            
            
            
            Vector3[] values = new Vector3[InterpolateSize-1];
            //Vector3[] delta = new Vector3[InterpolateSize-1];
            
            for (int j = 1; j <  InterpolateSize - 1  ; j++)
            {
                
                var value = arrayQueue[j][i];
                
                count += 1;
                values[j - 1] = value;
            }
            
            Vector3 currentDelta = values[0] - current;
            
            
             if (currentDelta.magnitude > 100.0f && isStablized)
             { // revert
                 Debug.Log("magnitude is " + currentDelta.magnitude + " in " + i + "th vector" + frame + "frame");
                 
                 arrayQueue[0][i] = arrayQueue[1][i];
             }

             if (currentDelta.magnitude > maxDeltaMove)
             {
                 maxDeltaMove = currentDelta.magnitude;
             }

            Vector3 sum = current;

            for (int j = 0; j < count - 1 ; j++)
            {
                sum += values[j];
            }
            
            var median = sum / count;

            var result = median ;

            coord[i] = result;

        }
        
        

        isStablized = true;

        frame++;
    }

    public override Vector3[] GetCoord()
    {
        return coord;
    }
}
