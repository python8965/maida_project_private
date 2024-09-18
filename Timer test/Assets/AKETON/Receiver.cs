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

    private void Awake()
    {
        BaseReceiver.OnFinishReceive += OnFinishReceive;
    }

    protected Vector3[] GetBaseCoord()
    {
        return BaseReceiver.baseCoord;
    }

    protected abstract void OnFinishReceive();
    private void OnDestroy()
    {
        BaseReceiver.OnFinishReceive -= OnFinishReceive;
    }

    public abstract Vector3[] GetCoord();
}

public class Receiver : IReceiver
{
    protected override void OnFinishReceive()
    {
        
    }

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
