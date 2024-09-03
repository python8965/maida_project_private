using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(FullBodyBipedIK))]

public class IKScript : MonoBehaviour
{
    private Animator animator;
    Receiver receiver;
    public FullBodyBipedIK ik;

    private List<Dictionary<string, object>> JointsCSV;
    //public FullBodyBipedIK ik;
    
    
    [FormerlySerializedAs("Factor")] public float factor = 18.0f; // 가져온 위치를 18.0f의 비율로 나누어서 ik 릭에 전달합니다. 나중에 계산 될 수 있을 겁니다.
    [FormerlySerializedAs("Height")] public float height = 0.5f; // 키 조정?
    // Start is called before the first frame update
    private void OnValidate()
    {
        
    }

    void Start()
    {
        // ik.solver.bodyEffector.positionWeight = 0.5f;
        //
        // ik.solver.leftHandEffector.positionWeight = 1.0f;
        // ik.solver.leftHandEffector.rotationWeight = 1.0f;
        //
        // ik.solver.leftShoulderEffector.positionWeight = 1.0f;
        // ik.solver.leftArmChain.bendConstraint.weight = 0.8f;
        //
        // ik.solver.rightHandEffector.positionWeight = 1.0f;
        // ik.solver.rightHandEffector.rotationWeight = 1.0f;
        //
        // ik.solver.leftShoulderEffector.positionWeight = 1.0f;
        //
        // ik.solver.rightArmChain.bendConstraint.weight = 0.8f;
        //
        // ik.solver.leftFootEffector.positionWeight = 1.0f;
        // ik.solver.leftFootEffector.rotationWeight = 1.0f;
        //
        // ik.solver.leftThighEffector.positionWeight = 1.0f;
        //
        // ik.solver.leftLegChain.bendConstraint.weight = 0.8f;
        //
        // ik.solver.rightFootEffector.positionWeight = 1.0f;
        // ik.solver.rightFootEffector.rotationWeight = 1.0f;
        //
        // ik.solver.rightThighEffector.positionWeight = 1.0f;
        // ik.solver.rightLegChain.bendConstraint.weight = 0.8f;
        
        // foreach (var dict in JointsCSV)
        // {
        //     string jointType = (string)dict["JointType"];
        //     string ikName = (string)dict["IKName"];
        //     string ikProperty = (string)dict["IKProperty"];
        //     int jointID = (int)dict["JointID"];
        //
        //     PropertyInfo ParseProperty(Type obj, string property)
        //     {
        //         string[] parts = property.Split('.');
        //
        //         Type type = null;
        //
        //         for (int i = 0; i < parts.Length - 1; i++)
        //         {
        //             var part = parts[i];
        //             
        //             if (i == 0)
        //             {
        //                 type = obj.GetProperty(part)!.GetType();
        //             }
        //             else
        //             {
        //                 type = type?.GetProperty(part)!.GetType();
        //             }
        //         }
        //         
        //         return type?.GetProperty(parts[^1]);
        //     }
        //
        //     if (jointType is "Simple" or "Bind")
        //     {
        //         var Property = ParseProperty(ik.GetType(), ikProperty);
        //         if (Property != null)
        //         {
        //             Property.SetValue(Property, Helpers.FindIKRig(transform, ikName).transform);
        //         }
        //     }
        // }
        
        receiver = gameObject.AddComponent<Receiver>();
        JointsCSV = CSVReader.Read("joints");
        var aFinger = transform.Find("Body");
        animator = GetComponent<Animator>();
        
        
        
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 ReceivedLocationToLocalLocation(Vector3 coord) // 수신받은 좌표를 월드 기준 ik릭 좌표로 변환합니다
        //수학을 좀 못해서 이상할 수 있습니다.
        {
            var ReceivedDelta = coord - Vector3.zero; 
            var ReceivedDeltaResized = ReceivedDelta / factor;

            var BoneFloor = transform.position + Vector3.up * height;
            var Result = BoneFloor + ReceivedDeltaResized;
            return Result;
        }
        
        { // 머리 부분을 따로 적용하는 코드입니다. 이것도 이미 있던 코드입니다.
            var IK = Helpers.FindIKRig(transform, "Head");
            
            var eye1 = Helpers.GetReceivedPosition(receiver.coord, 41); // Left Shoulder
            var eye2 = Helpers.GetReceivedPosition(receiver.coord, 40); // Right Shoulder
            var nose = Helpers.GetReceivedPosition(receiver.coord, 54); // Left Thigh
            var ear1 = Helpers.GetReceivedPosition(receiver.coord, 24); // Right Thigh
            var ear2 = Helpers.GetReceivedPosition(receiver.coord, 39); // Right Thigh 
            
            // 머리의 위치 계산 (모든 스피어의 평균 위치)
            Vector3 headPosition = (eye1 + eye2 + nose + ear1 + ear2) / 5.0f;
            IK.position = ReceivedLocationToLocalLocation(headPosition);

            // 머리의 방향 계산 (여기서는 단순히 눈의 중간과 코를 기준으로 계산)
            Vector3 eyeCenter = (eye1 + eye2) / 2.0f;
            Vector3 forward = (nose - eyeCenter).normalized;
            Vector3 up = Vector3.Cross((ear1 - ear2).normalized, forward).normalized;

            // 머리의 회전 적용
            Quaternion headRotation = Quaternion.LookRotation(forward, up);
            IK.rotation = headRotation;
        }
        
        { // 몸통 부분을 따로 처리하는 코드입니다. 
            var ls = Helpers.GetReceivedPosition(receiver.coord, 2); // Left Shoulder
            var rs = Helpers.GetReceivedPosition(receiver.coord, 5); // Right Shoulder
            var lt = Helpers.GetReceivedPosition(receiver.coord, 8); // Left Thigh
            var rt = Helpers.GetReceivedPosition(receiver.coord, 11); // Right Thigh

            var neck = Helpers.GetReceivedPosition(receiver.coord, 1);
            
            var ms = (ls + rs) / 2.0f;
            

            ms = (ms + neck) / 2.0f;
            
            var mt = (lt + rt) / 2.0f;
            
            var body = ms * 0.3f + mt * 0.7f;
            var ikRig = Helpers.FindIKRig(transform, "Body");
            
            

            ikRig.position = ReceivedLocationToLocalLocation(body);
        }

        
        
        foreach (var dict in JointsCSV)
        {
            string jointType = (string)dict["JointType"];

            if (!(jointType == "Simple" && jointType == "Rotation"))
            {
                continue;
            }
            
            string ikName = (string)dict["IKName"];
            int jointID = (int)dict["JointID"];
            
            
            
            var ikRig = Helpers.RecursiveFindChild(transform, ikName);

            var unsizedCoord = Helpers.GetReceivedPosition(receiver.coord, jointID);
            var ikPosition = ReceivedLocationToLocalLocation(unsizedCoord);

            switch (jointType)
            {
                case "Simple":
                {
                    ikRig.position = ikPosition;
                    break;
                }
                
                case "Rotation":
                {
                    int TargetID = (int)dict["RotationTargetID"];
                    int HintID = (int)dict["RotationHintID"];

                    var target = Helpers.GetReceivedPosition(receiver.coord, TargetID);
                    var hint = Helpers.GetReceivedPosition(receiver.coord, HintID);

                    var coord = unsizedCoord;

                    var vector = target - coord;
                    var project = Vector3.ProjectOnPlane(vector, Vector3.up);

                    
                    var roation = Quaternion.FromToRotation(Vector3.up, target - coord);

                    var rawhintvector = hint - coord;
                    var rawtargetvector = target - coord;
                    
                    var fix = Vector3.Project(rawhintvector, rawtargetvector);
                    
                    var targetvector = rawtargetvector.normalized;

                    var hintvector = (rawhintvector - fix).normalized;
                    
                    Debug.DrawRay(ikRig.position, targetvector * 0.2f, Color.red);
                    Debug.DrawRay(ikRig.position, hintvector * 0.2f, Color.green);

                    var FrontRot = Quaternion.LookRotation(hintvector, targetvector);
                    
                    
                    ikRig.rotation = FrontRot;

                    break;
                }


                case "Grip":
                {
                    
                    break;
                }
            }
        }
        
        
        // 배경에 수신받은 좌표 기준으로 선을 그리는 디버깅 코드입니다. 
        var dicts = CSVReader.Read("bones");
        foreach (var dict in dicts)
        {
            string boneName = (string)dict["BoneName"];
            int firstIndex = (int)dict["FirstBoneID"];
            int lastIndex = (int)dict["LastBoneID"];
            
            
            
            if (boneName == "")
            {
                continue;
            }

            if (firstIndex > lastIndex)
            {
                Debug.LogError($"FirstIndex {firstIndex} is greater than LastIndex {lastIndex}");
            }
            
            var findbone = animator.avatar.humanDescription.human.ToList()
                .Find(bone => bone.humanName == boneName);
            var boneIndex = animator.avatar.humanDescription.skeleton.ToList().FindIndex(p => p.name == findbone.boneName);
            
            //Debug.Log(findbone.boneName);
            if (boneIndex != -1)
            {
                Vector3 startPoint = Helpers.GetReceivedPosition(receiver.coord, firstIndex);
                Vector3 endPoint = Helpers.GetReceivedPosition(receiver.coord, lastIndex);
                
                
                Debug.DrawLine(startPoint, endPoint, Color.green);
            }
            
            
        }
    }
}
