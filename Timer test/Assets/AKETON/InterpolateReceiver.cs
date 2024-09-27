using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using UnityEditor;


public class InterpolateReceiver : IReceiver
{
    private UdpClient m_Receiver;
    public int InterpolateSize = 5;
    public int frame = 0;
    
    private Queue<Vector3[]> queue;
    public List<double> distanceList;
    private Vector3[] coord;
    
    bool skipFrame = false;
    bool enableFilterByDistance = false;
    bool enableInterpolate = true;

    [DebugGUIGraph(min: -1, max: 1, r: 0, g: 1, b: 1, autoScale: true)]
    public double maxDeltaMove;

    void Awake()
    {
        if (BaseReceiver == null)
        {
            Debug.LogError("Base receiver is null");
        }
        
        BaseReceiver.OnReceive += ReceiveCallback;
        
        
        coord = new Vector3[Helpers.CoordVectorSize];
        queue = new Queue<Vector3[]>();
        
        distanceList = new List<double>();
    }

    protected override void OnFinishReceive()
    {
        coord = new Vector3[Helpers.CoordVectorSize];
        queue = new Queue<Vector3[]>();
        
        distanceList = new List<double>();
    }

    void ReceiveCallback()
    {
        if (BaseReceiver.m_ReceiveMessage.Equals("End"))
        {
            skipFrame = false;
            
            
            frame = 0;
            
            queue.Clear();
            distanceList = new List<double>();
            return;
        }
        
        var jointsCSV = CSVReader.jointCsv;

        
        
        var baseCoord = GetBaseCoord();

        var localDistanceArray = new List<double>();
        
        foreach (var joint in jointsCSV)
        {
            var jointType = (string)joint["JointType"];
            if (!jointType.Equals("Position"))
            {
                continue;
            }
            int jointID = (int)joint["JointID"];
            int targetID = (int)joint["TargetID"];

            var jointPos = baseCoord[jointID];
            var targetPos = baseCoord[targetID];

            var length = Vector3.Distance(jointPos, targetPos);
            
            localDistanceArray.Add(length);
        }

        if (frame >= InterpolateSize && enableFilterByDistance)
        {
            for (int i = 0; i < localDistanceArray.Count; i++)
            {
                var errorFactor = localDistanceArray[i] / distanceList[i];

                const double factorRatio = 1.5;

                if (errorFactor is > factorRatio or < 1 / factorRatio)
                {
                    Debug.Log(errorFactor + " Factor Out Of Range, Current Frame" + frame + "Is Ignored");
                    skipFrame = true;
                }
            }
        }

        if (!skipFrame && enableFilterByDistance)
        {
            if (distanceList.Count != localDistanceArray.Count)
            {
                Debug.Log("dist array size mismatch, initalizing");
                distanceList = localDistanceArray;
            }
            else
            {
                for (int i = 0; i < localDistanceArray.Count; i++)
                {
                    distanceList[i] = distanceList[i] * 0.8 + localDistanceArray[i] * 0.2;
                }
            }
        }
        else if(! enableFilterByDistance)
        {
            distanceList = new List<double>();
        }

        if (skipFrame)
        {
            skipFrame = false;
            frame++;
            return;
        }
        

        queue.Enqueue(baseCoord.Clone() as Vector3[]);
        if (queue.Count > InterpolateSize)
        {
            queue.Dequeue();
        }

        var arrayQueue = queue.ToArray();
        maxDeltaMove = 0.0;
        for (int i = 0; i < Helpers.CoordVectorSize; i++) // 점마다 반복
        {
            int count = 1;

            Vector3 current = arrayQueue[0][i];
            
            if (frame >= InterpolateSize && enableInterpolate)
            {
                Vector3[] values = new Vector3[InterpolateSize-1];
                for (int j = 1; j <  InterpolateSize - 1; j++)
                {
                    var value = arrayQueue[j][i];
                
                    count += 1;
                    values[j - 1] = value;
                }
                Vector3 currentDelta = values[0] - current;
                
                Vector3 sum = current;

                for (int j = 0; j < count - 1 ; j++)
                {
                    sum += values[j];
                }
            
                var median = sum / count;

                var result = median ;

                coord[i] = result;
            
                if (currentDelta.magnitude > maxDeltaMove)
                {
                    maxDeltaMove = currentDelta.magnitude;
                }
            } else if (!enableInterpolate)
            {
                coord[i] = current;
            }
        }
        
        frame++;
    }

    public override Vector3[] GetCoord()
    {
        return coord;
    }
}
