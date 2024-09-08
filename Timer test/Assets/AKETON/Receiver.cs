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
    
    protected float[] GetBaseCoord()
    {
        return BaseReceiver.baseCoord;
    }
    public abstract float[] GetCoord();
}

public class Receiver : IReceiver
{
    public override float[] GetCoord()
    {
        var BaseCoord = GetBaseCoord();
        
        if (BaseCoord == null)
        {
            Debug.LogError("BaseCoord Is Null");
        }
        
        return BaseCoord;
    }
}
