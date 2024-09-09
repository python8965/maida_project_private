using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;



public abstract class IReceiver: MonoBehaviour
{
    public BaseReceiver BaseReceiver;

    
    protected Vector3[] GetBaseCoord()
    {
        return BaseReceiver.baseCoord;
    }
    public abstract Vector3[] GetCoord();
}

public class Receiver : IReceiver
{
    public override Vector3[] GetCoord()
    {
        var BaseCoord = GetBaseCoord();
        
        if (BaseCoord == null)
        {
            Debug.LogError("BaseCoord Is Null");
        }
        
        return BaseCoord;
    }
}
