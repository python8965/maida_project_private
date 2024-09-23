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
    public Transform startBone;
    public Transform endBone;
    [ReadOnly]
    public float originalRaito;
}

[RequireComponent(typeof(IKScript))]
[RequireComponent(typeof(ScalableBoneReference))]
public class CustomizeScript : MonoBehaviour
{
    [FormerlySerializedAs("bones")] public CustomizeBones[] customizeBones;

    
    
    public double[] scale;

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
            }
        };
        
        scale = new double[customizeBones.Length];

        for (int i = 0; i < customizeBones.Length; i++)
        {
            scale[i] = 1.0;
        }

        for (int i = 0; i < customizeBones.Length; i++)
        {
            customizeBones[i].startBone = (Transform)reference.GetType().GetField(customizeBones[i].name).GetValue(reference);
            customizeBones[i].endBone = (Transform)reference.GetType().GetField(customizeBones[i].endBoneName).GetValue(reference);
        }
    }

    private void Awake()
    {
        ik = GetComponent<IKScript>();
        reference = GetComponent<ScalableBoneReference>();
    }

    
    [ContextMenu("SetOriginalRaito")]
    void SetOriginalRaito()
    {
        reference = GetComponent<ScalableBoneReference>();
        
        var identityLength = Vector3.Distance(reference.LeftUpLeg.position, reference.RightUpLeg.position); ; 
        
        for (var index = 0; index < customizeBones.Length; index++)
        {
            var b = customizeBones[index];

            var boneLength = Vector3.Distance(b.startBone.position, b.endBone.position);    
            
            Debug.Log("SetOriginalRaito "+boneLength +" / "+identityLength + " = " + boneLength/identityLength);
            
            customizeBones[index].originalRaito = boneLength / identityLength;
                              
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    [ContextMenu("Calibrate")]
    void Calibrate()
    {
        var coord = ik.receiver.GetCoord();

        var ThighA = Helpers.GetReceivedPosition(coord, 8);
        
        var ThighB = Helpers.GetReceivedPosition(coord, 11);
        
        var identityLength = Vector3.Distance(ThighA, ThighB);
        
        foreach (var bone in CSVReader.jointCsv)
        {
            var jointType = (string)bone["JointType"];
            
            if (jointType.Equals("Position"))
            {
                var boneName = (string)bone["BoneName"];
                var boneId = customizeBones.ToList().FindIndex(bones => bones.name.Equals(boneName));
                if (boneId == -1)
                {
                    Debug.Log(boneName + " is not exist. continue.");
                    continue;
                }
                
                var JointID = (int)bone["JointID"];
                var TargetID = (int)bone["TargetID"];

                var JointPos = Helpers.GetReceivedPosition(coord, JointID);
                var TargetPos = Helpers.GetReceivedPosition(coord, TargetID);

                var Length =  Vector3.Distance(JointPos, TargetPos);

                var Raito = Length / identityLength;
                
                scale[boneId] = customizeBones[boneId].originalRaito / Raito;
                Debug.Log($"Calibration receivedRatio : {Raito} result {scale[boneId]}");
                
                
                //
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < customizeBones.Length; i++)
        {
            var bone = customizeBones[i];
            
            var scalev = (float)scale[i];
            var scaleav = (scalev + 1.0f) / 2.0f;
            
            bone.startBone.localScale = new Vector3(scaleav, scalev,scaleav);

            // for (int j = 0; j < bone.bones.Length; j++)
            // {
            //     Debug.Log(bone.bones[j].name);
            //     bone.bones[j].localScale = new Vector3( scaleav, scalev,scaleav);
            // }
        }
    }
}
