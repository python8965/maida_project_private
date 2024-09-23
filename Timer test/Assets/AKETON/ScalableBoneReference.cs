using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable InconsistentNaming

public class ScalableBoneReference : MonoBehaviour
{
    public Transform root;
    
    public Transform Head;
    public Transform Body;
    
    public Transform LeftHand;
    public Transform LeftForeArm;
    public Transform LeftArm;
    public Transform LeftShoulder;
    
    public Transform RightHand;
    public Transform RightForeArm;
    public Transform RightArm;
    public Transform RightShoulder;
    
    public Transform LeftUpLeg;
    public Transform LeftLeg;
    public Transform LeftFoot;
    
    public Transform RightUpLeg;
    public Transform RightLeg;
    public Transform RightFoot;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Transform GetReferenceByName(string boneName)
    {
        return (Transform)GetType().GetField(boneName).GetValue(this);
    }

    public void SetReferenceByName(string boneName, Transform bone)
    {
        GetType().GetField(boneName).SetValue(this, bone);
    }

    
    [ContextMenu("AutoUpdateReferences")]
    void AutoUpdateReferences()
    {
        if (root == null)
        {
            return;
        }
        
        List<string> boneNames = new List<string> { "Head" , "Body" , "LeftHand", "LeftForeArm", "LeftArm",
            "LeftShoulder", "RightHand", "RightForeArm" ,"RightArm", "RightShoulder", "LeftUpLeg","LeftLeg", "LeftFoot","RightUpLeg","RightLeg","RightFoot"};

        foreach (Transform child in root)
        {
            foreach (var boneName in boneNames)
            {
                if (child.name.Contains(boneName))
                {
                    Debug.Log("Find boneName : " + boneName);
                    
                    SetReferenceByName(boneName, child);
                    
                    //GetType().GetField(boneName).SetValue(this, child);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
