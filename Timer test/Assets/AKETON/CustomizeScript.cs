using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OnlyNew.BodyProportions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct CustomizeBones
{
    [FormerlySerializedAs("startBoneName")] public string name;
    public string endBoneName;
    public Transform ikStartBone;
    public Transform ikEndBone;
    
    public Transform bone;
    
    [ReadOnly]
    public float originalLength;
}

[RequireComponent(typeof(IKScript))]
[RequireComponent(typeof(ScalableBoneReference))]
public class CustomizeScript : MonoBehaviour
{
    public CustomizeBones[] customizeBones;

    
    
    public double[] scale;

    public bool CalibrateEveryFrame;

    private bool isChanged;
    
    private IKScript ik;
    private ScalableBoneReference reference;
    private void Reset()
    {
        ik = GetComponent<IKScript>();
        reference = GetComponent<ScalableBoneReference>();
        
        customizeBones = new[]
        {
            // new CustomizeBones
            // {
            //     startBoneName = "Spine",
            // },
            new CustomizeBones
            {
                name = "LeftArm",
                endBoneName = "LeftForeArm",
            },
            new CustomizeBones
            {
                name = "RightArm",
                endBoneName = "RightForeArm",
            },
            new CustomizeBones
            {
                name = "LeftForeArm",
                endBoneName = "LeftHand",
            },
            new CustomizeBones
            {
                name = "RightForeArm",
                endBoneName = "RightHand",
            },
            new CustomizeBones
            {
                name = "LeftUpLeg",
                endBoneName = "LeftLeg",
            },
            new CustomizeBones
            {
                name = "RightUpLeg",
                endBoneName = "RightLeg",
            },
            new CustomizeBones
            {
                name = "LeftLeg",
                endBoneName = "LeftFoot",
            },
            new CustomizeBones
            {
                name = "RightLeg",
                endBoneName = "RightFoot",
            },
            
            
        };
        
        scale = new double[customizeBones.Length];

        for (int i = 0; i < customizeBones.Length; i++)
        {
            scale[i] = 1.0;
        }

        for (int i = 0; i < customizeBones.Length; i++)
        {
            customizeBones[i].bone = (Transform)reference.GetType().GetField(customizeBones[i].name).GetValue(reference);
            //customizeBones[i].endBone = (Transform)reference.GetType().GetField(customizeBones[i].endBoneName).GetValue(reference);
            
            customizeBones[i].ikStartBone = Helpers.FindIKRig(transform, customizeBones[i].name);
            customizeBones[i].ikEndBone = Helpers.FindIKRig(transform, customizeBones[i].endBoneName);
        }
    }

    private void Awake()
    {
        ik = GetComponent<IKScript>();
        reference = GetComponent<ScalableBoneReference>();
    }

    
    [ContextMenu("SetOriginalLength")]
    void SetOriginalLength()
    {
        reference = GetComponent<ScalableBoneReference>();
        
        for (var index = 0; index < customizeBones.Length; index++)
        {
            var b = customizeBones[index];

            var boneLength = Vector3.Distance(b.ikStartBone.position, b.ikEndBone.position);    
            
            Debug.Log("SetOriginalRaito "+boneLength );
            
            customizeBones[index].originalLength = boneLength;
                              
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    [ContextMenu("Calibrate")]
    void Calibrate()
    {
        reference = GetComponent<ScalableBoneReference>();
        
        for (var index = 0; index < customizeBones.Length; index++)
        {
            var b = customizeBones[index];

            var boneLength = Vector3.Distance(b.ikStartBone.position, b.ikEndBone.position);    
                         
            scale[index] = boneLength / customizeBones[index].originalLength ;
            Debug.Log($"{b.name} receivedLength : {boneLength} original: {customizeBones[index].originalLength} result {scale[index]}");
        }
        //
        // foreach (var bone in CSVReader.jointCsv)
        // {
        //     var jointType = (string)bone["JointType"];
        //     
        //     if (jointType.Equals("Position"))
        //     {
        //         var boneName = (string)bone["IKName"];
        //         var boneId = customizeBones.ToList().FindIndex(bones => bones.name.Equals(boneName));
        //         if (boneId == -1)
        //         {
        //             Debug.Log(boneName + " is not exist. continue.");
        //             continue;
        //         }
        //         
        //         var boneLength = Vector3.Distance(b.startBone.position, b.endBone.position);    
        //
        //         
        //         var JointPos = Helpers.GetReceivedPosition(coord, JointID, ik.divideFactor);
        //         var TargetPos = Helpers.GetReceivedPosition(coord, TargetID, ik.divideFactor);
        //         
        //         Debug.DrawLine(JointPos, TargetPos, Color.yellow);
        //
        //         var Length =  Vector3.Distance(JointPos, TargetPos);
        //
        //
        //         scale[boneId] = Length / customizeBones[boneId].originalLength ;
        //         Debug.Log($"Calibration bone name {boneName} receivedLength : {Length} , {customizeBones[boneId].originalLength} result {scale[boneId]}");
        //         
        //         
        //         //
        //     }
        // }
    }

    // Update is called once per frame
    void Update()
    {
        if (CalibrateEveryFrame)
        {
            Calibrate();
        }
        
        for (int i = 0; i < customizeBones.Length; i++)
        {
            var bone = customizeBones[i];
            
            var scalev = (float)scale[i];
            var scaleav = (scalev + 3.0f) / 4.0f;
            
            bone.bone.localScale = new Vector3(scaleav, scalev,scaleav);

            // for (int j = 0; j < bone.bones.Length; j++)
            // {
            //     Debug.Log(bone.bones[j].name);
            //     bone.bones[j].localScale = new Vector3( scaleav, scalev,scaleav);
            // }
        }
    }
}
