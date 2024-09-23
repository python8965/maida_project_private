using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RootMotion;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

[RequireComponent(typeof(FullBodyBipedIK))]
[RequireComponent(typeof(ScalableBoneReference))]
public class IKScript : MonoBehaviour
{
    public IReceiver receiver;
    public FullBodyBipedIK ik;
    public bool isStopReceving = false;
    public bool isMirroredReceving = false;
    //public FullBodyBipedIK ik;

    public Transform LeftUpperLeg;
    public Transform RightUpperLeg;
    public float factor = 18.0f; // 가져온 위치를 18.0f의 비율로 나누어서 ik 릭에 전달합니다. 나중에 계산 될 수 있을 겁니다.
    // Start is called before the first frame update

    public Transform rootBone;
    public bool debug;
    private ScalableBoneReference reference;

    public SerializableDictionary<string, Quaternion> baseRotations;
    
    [ContextMenu("AutoInit")]
    private void AutoInit()
    {
        string error = "";
        reference = GetComponent<ScalableBoneReference>();
        ik = GetComponent<FullBodyBipedIK>();
        
        if (ik.ReferencesError(ref error) && rootBone != null)
        {
            var bipedRef = new BipedReferences();
            BipedReferences.AutoDetectReferences(ref bipedRef, rootBone, BipedReferences.AutoDetectParams.Default);
            
            ik.SetReferences(bipedRef, null);
            
            
        }
        else
        {
            Debug.Log("root Bone is null");
            
        }



        if (ik.ReferencesError(ref error))
        {
            Debug.LogWarning("IK script is not initiated");
            return;
        }
        
        if (ik != null)
        {
            ik.solver.bodyEffector.positionWeight = 0.5f;
            ik.solver.leftHandEffector.positionWeight = 1.0f;
            ik.solver.leftHandEffector.rotationWeight = 1.0f;
            ik.solver.leftShoulderEffector.positionWeight = 1.0f;
            ik.solver.leftArmChain.bendConstraint.weight = 0.8f;

            ik.solver.rightHandEffector.positionWeight = 1.0f;
            ik.solver.rightHandEffector.rotationWeight = 1.0f;

            ik.solver.rightShoulderEffector.positionWeight = 1.0f;

            ik.solver.rightArmChain.bendConstraint.weight = 0.8f;

            ik.solver.leftFootEffector.positionWeight = 1.0f;
            ik.solver.leftFootEffector.rotationWeight = 1.0f;

            ik.solver.leftThighEffector.positionWeight = 1.0f;

            ik.solver.leftLegChain.bendConstraint.weight = 0.8f;

            ik.solver.rightFootEffector.positionWeight = 1.0f;
            ik.solver.rightFootEffector.rotationWeight = 1.0f;

            ik.solver.rightThighEffector.positionWeight = 1.0f;
            ik.solver.rightLegChain.bendConstraint.weight = 0.8f;
        }
        if (transform.Find("IKRig") == null)
        {
            var obj = new GameObject("IKRig");
            obj.transform.parent = transform;
            obj.AddComponent<Rig>();
        
            foreach (var dict in CSVReader.jointCsv)
            {
                string ikName = (string)dict["IKName"];
            
                var ikObject = new GameObject(ikName);
                ikObject.transform.parent = obj.transform;
            }
        }
        
        baseRotations.Clear();
        
        
        foreach (var dict in CSVReader.jointCsv)
        {
            string jointType = (string)dict["JointType"];
            string ikName = (string)dict["IKName"];
            string ikProperty = (string)dict["IKProperty"];

            if (jointType.Equals("Bind") || jointType.Equals("Position"))
            {
                Debug.Log(ikProperty);
                Debug.Log(ikName);

                var targetTransform = Helpers.FindIKRig(transform, ikName).transform;
                Helpers.SetValue(ik, ikProperty, targetTransform);
            }

            if (jointType.Equals("Rotation"))
            {
                Debug.Log(ikProperty);
                Debug.Log(ikName);
                
                var boneName = (string)dict["BoneName"];

                var refTransform = reference.GetReferenceByName(boneName);
                if (refTransform == null)
                {
                    Debug.LogError("Reference transform is null, maybe forget to init reference?");
                }
                
                baseRotations.Add(boneName, refTransform.rotation);


            }
        }

        
    }
    void Start()
    {
        reference = GetComponent<ScalableBoneReference>();
        ik = GetComponent<FullBodyBipedIK>();
    }
    // Update is called once per frame
    void Update()
    {
        CSVReader.isMirrored = isMirroredReceving;
        
        var received = receiver.GetCoord();
        
        if (isStopReceving)
        {
            return;
        }
        var foot = Helpers.GetReceivedPosition(received, 13);
        var resizedFoot = foot / factor;
        
        
        var lThigh = Helpers.GetReceivedPosition(received, 8);
        var rThigh = Helpers.GetReceivedPosition(received, 11);
        
        // Debug.Log(lThigh);
        // Debug.Log(rThigh);

        var thighDistance = Vector3.Distance(lThigh, rThigh);
        
        var lThighBone = LeftUpperLeg;
        var rThighBone = RightUpperLeg;
        
        var boneDistance = Vector3.Distance(lThighBone.position, rThighBone.position);

        
        // Debug.Log(thighDistance);
        // Debug.Log(boneDistance);
        
        
        factor = thighDistance / boneDistance / 1.5f;

        if (factor == 0.0f)
        {
            factor = 10.0f;
        }
        
        Vector3 ReceivedLocationToLocalLocation(Vector3 coord) // 수신받은 좌표를 월드 기준 ik릭 좌표로 변환합니다
        //수학을 좀 못해서 이상할 수 있습니다.
        {
            var ReceivedDeltaResized = coord / factor;

            var Result = ReceivedDeltaResized - resizedFoot;
            return Result;
        }
        
        { // 머리 부분을 따로 적용하는 코드입니다. 이것도 이미 있던 코드입니다.
            var IK = Helpers.FindIKRig(transform, "Head");

            var chin = Helpers.GetReceivedPosition(received, 32);
            var eye1 = Helpers.GetReceivedPosition(received, 41);
            var eye2 = Helpers.GetReceivedPosition(received, 50);
            var nose = Helpers.GetReceivedPosition(received, 54);
            
            var ranchor = Helpers.GetReceivedPosition(received, 25);
            var lanchor = Helpers.GetReceivedPosition(received, 39);
            
            // 머리의 위치 계산 (모든 스피어의 평균 위치)
            Vector3 headPosition = (eye1 + eye2 + nose + ranchor + lanchor) / 5.0f;
            Vector3 localHeadPosition = ReceivedLocationToLocalLocation(headPosition);
            IK.position = localHeadPosition;

            // 머리의 방향 계산 (여기서는 단순히 눈의 중간과 코를 기준으로 계산)
            
            
            
            Vector3 anchorCenter = (ranchor + lanchor) / 2.0f;
            Vector3 right = (lanchor- ranchor).normalized;
            Vector3 up = (anchorCenter - chin).normalized;
            //Vector3 up = Vector3.Cross((ear1 - ear2).normalized, forward).normalized;

            Vector3 front = Vector3.Cross(up, right);

            if (isMirroredReceving)
            {
                front = -front;
            }
            
            if (debug)
            {
                Debug.DrawRay(localHeadPosition, front * 0.1f, Color.blue);
                Debug.DrawRay(localHeadPosition, up * 0.1f, Color.green);
                Debug.DrawRay(localHeadPosition, right * 0.1f, Color.red);
            }
            
            // 머리의 회전 적용
            Quaternion headRotation = Quaternion.LookRotation(front, up);
            IK.rotation = headRotation;
        }
        
        { // 몸통 부분을 따로 처리하는 코드입니다. 
            var ls = Helpers.GetReceivedPosition(received, 2); // Left Shoulder
            var rs = Helpers.GetReceivedPosition(received, 5); // Right Shoulder
            var lt = Helpers.GetReceivedPosition(received, 8); // Left Thigh
            var rt = Helpers.GetReceivedPosition(received, 11); // Right Thigh

            var neck = Helpers.GetReceivedPosition(received, 1);
            
            var ms = (ls + rs) / 2.0f;
            

            ms = (ms + neck) / 2.0f;
            
            var mt = (lt + rt) / 2.0f;
            
            var body = ms * 0.3f + mt * 0.7f;
            var ikRig = Helpers.FindIKRig(transform, "Body");
            
            

            ikRig.position = ReceivedLocationToLocalLocation(body);
        }
        
        var csv = CSVReader.jointCsv;
        
        
        
        foreach (var dict in csv)
        {
            string jointType = (string)dict["JointType"];

            if (jointType.Equals("Bind"))
            {
                continue;
            }
            
            string ikName = (string)dict["IKName"];
            int jointID = (int)dict["JointID"];
            
            
            
            var ikRig = Helpers.FindIKRig(transform, ikName);

            var unsizedCoord = Helpers.GetReceivedPosition(received,jointID);
            var ikPosition = ReceivedLocationToLocalLocation(unsizedCoord);

            switch (jointType)
            {
                case "Position":
                {
                    int TargetID = (int)dict["TargetID"];

                    // if (TargetID != -1)
                    // {
                    //     int Adjust = (int)dict["Adjust"];
                    //     
                    //     var AdjustTarget = ReceivedLocationToLocalLocation(Helpers.GetReceivedPosition(received,TargetID));
                    //
                    //
                    //     var Ori = ikPosition * ((100 - Adjust) * 0.01f);
                    //     var Adj = AdjustTarget * (Adjust * 0.01f);
                    //
                    //     ikPosition = Ori + Adj;
                    // }
                    
                    ikRig.position = ikPosition;
                    break;
                }
                
                case "Rotation":
                {
                    int TargetID = (int)dict["TargetID"];
                    int HintID = (int)dict["HintID"];
                    int AdjustFlag = (int)dict["Adjust"];
                    var boneName = (string)dict["BoneName"];
                    var target = Helpers.GetReceivedPosition(received, TargetID);
                    var hint = Helpers.GetReceivedPosition(received, HintID);

                    var coord = unsizedCoord;

                    var vector = target - coord;
                    var project = Vector3.ProjectOnPlane(vector, Vector3.up);

                    
                    var roation = Quaternion.FromToRotation(Vector3.up, target - coord);

                    var rawhintvector = hint - coord;
                    var rawtargetvector = target - coord;
                    
                    var fix = Vector3.Project(rawhintvector, rawtargetvector);
                    
                    var targetvector = rawtargetvector.normalized;

                    var hintvector = (rawhintvector - fix).normalized;
                    
                    if (debug)
                    {

                        Debug.DrawRay(ikRig.position, targetvector * 0.4f, Color.red);
                        Debug.DrawRay(ikRig.position, hintvector * 0.4f, Color.green);

                        Debug.DrawRay(unsizedCoord, targetvector * 1.0f, Color.red);
                        Debug.DrawRay(unsizedCoord, hintvector * 1.0f, Color.green);
                    }
                    

                    var FrontRot = Quaternion.identity;
                    
                    reference.LeftFoot.rotation = roation;

                    var rot = baseRotations[boneName];
                    
                    
                    if (false)
                    {
                        var forwardvector = Vector3.Cross(targetvector, hintvector);
                        
                        if (AdjustFlag == 1) // flip
                        {
                            forwardvector = -forwardvector;
                        }
                        Debug.DrawRay(ikRig.position, forwardvector * 0.4f, Color.black);
                        FrontRot = Quaternion.LookRotation(forwardvector, targetvector);
                    }
                    else
                    {
                        var upvector = Vector3.Cross(targetvector, hintvector);
                        if (AdjustFlag == 1) // flip
                        {
                            upvector = -upvector;
                        }
                        
                        Debug.DrawRay(ikRig.position, upvector * 0.4f, Color.black);

                        var diffrot = Quaternion.LookRotation(targetvector, upvector);
                        FrontRot = diffrot * rot;
                        Debug.DrawRay(ikRig.position, FrontRot.eulerAngles.normalized , Color.white);
                    }
                    

                    ikRig.rotation = FrontRot;

                    break;
                }


                case "Grip":
                {
                    int TargetID = (int)dict["TargetID"];
                    int HintID = (int)dict["HintID"];
                    string boneName = (string)dict["IKProperty"];
                    var middle = ReceivedLocationToLocalLocation(Helpers.GetReceivedPosition(received,TargetID));
                    var last = ReceivedLocationToLocalLocation(Helpers.GetReceivedPosition(received,HintID));

                    var d1 = (middle - ikPosition).normalized;
                    var d2 = (last - middle).normalized;

                    var factor = Vector3.Cross(d1, d2);

                    if (factor.magnitude > 0.5f)
                    {
                        //Debug.Log("Grip");
                    }

                    
                    
                    var link = ikRig.gameObject.GetComponent<BoneLink>();

                    if (link == null)
                    {
                        break;
                    }

                    var poser = link.bone.GetComponent<HandPoser>();
                    if (poser == null)
                    {
                        Debug.Log("Poser not found");
                        break;
                    }
                    
                    //Debug.Log("Poser found");
                    
                    poser.weight = factor.magnitude;
                    
                    if (debug)
                    {

                        Debug.DrawRay(ikPosition, d1 * 2.0f, Color.red);
                        Debug.DrawRay(ikPosition, d2 * 2.0f, Color.green);
                    }
                    
                    break;
                }
            }
        }
        
        
        // 배경에 수신받은 좌표 기준으로 선을 그리는 디버깅 코드입니다. 
    }
}
