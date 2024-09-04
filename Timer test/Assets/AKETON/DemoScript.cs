using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

public class DemoScript : MonoBehaviour //본을 직접 건드리는 방식입니다. 이건 쓰지 않을 것 같습니다. 
{
    // SkinnedMeshRenderer skinnedMeshRenderer;
    // protected Animator animator;
    // Receiver receiver;
    // // Start is called before the first frame update
    // void Start()
    // {
    //     receiver = gameObject.AddComponent<Receiver>();
    //     
    //     
    //     // 테스트용으로 추가됨 - 제거
    //     var aFinger = transform.Find("Body");
    //     skinnedMeshRenderer =  aFinger.gameObject.GetComponent<SkinnedMeshRenderer>();
    //     animator = GetComponent<Animator>();
    //     if (skinnedMeshRenderer == null)
    //     {
    //         Debug.LogError("SkinnedMeshRenderer is null");
    //     }
    // }
    //
    // // Update is called once per frame
    // void OnAnimatorIK()
    // {
    //
    //     {
    //         var p1 = Helpers.GetReceivedPosition(received,2);
    //         var p2 = Helpers.GetReceivedPosition(received,5);
    //         
    //         var rotation = Quaternion.FromToRotation(Vector3.up, p2 - p1); 
    //         //Quaternion.LookRotation(p2-p1, Vector3.up);
    //         var findbone = animator.avatar.humanDescription.human.ToList()
    //             .Find(bone => bone.humanName == "Spine");
    //         var skeleton = animator.avatar.humanDescription.skeleton.ToList().Find(p => p.name == findbone.boneName);
    //         var Bone = Helpers.RecursiveFindChild(transform, findbone.boneName);
    //
    //         var eular = rotation.eulerAngles;
    //
    //         var d = eular.y;
    //         
    //         Bone.rotation = skeleton.rotation * Quaternion.Euler(0.0f, d, 0.0f);
    //         //Bone.rotation = skeleton.rotation;
    //         
    //         Debug.DrawLine(Bone.position, rotation * Vector3.forward, Color.green);
    //     }
    //     var csvdata = CSVReader.Read("bones");
    //     foreach (var dict in csvdata)
    //     {
    //         string BoneName = (string)dict["BoneName"];
    //         int FirstIndex = (int)dict["FirstBoneID"];
    //         int LastIndex = (int)dict["LastBoneID"];
    //
    //         
    //         
    //         if (BoneName == "")
    //         {
    //             continue;
    //         }
    //
    //         if (FirstIndex > LastIndex)
    //         {
    //             Debug.LogError($"FirstIndex {FirstIndex} is greater than LastIndex {LastIndex}");
    //         }
    //         
    //         var findbone = animator.avatar.humanDescription.human.ToList()
    //             .Find(bone => bone.humanName == BoneName);
    //         var boneIndex = animator.avatar.humanDescription.skeleton.ToList().FindIndex(p => p.name == findbone.boneName);
    //         
    //         //Debug.Log(findbone.boneName);
    //         if (boneIndex != -1)
    //         {
    //             Vector3 startPoint = Helpers.GetReceivedPosition(FirstIndex);
    //             Vector3 endPoint = Helpers.GetReceivedPosition(LastIndex);
    //             var localScale = new Vector3(1, (endPoint - startPoint).magnitude / 2, 1);
    //
    //             var TposeRotation = animator.avatar.humanDescription.skeleton[boneIndex].rotation;
    //             
    //             var rotation = Quaternion.FromToRotation(Vector3.up, endPoint - startPoint);
    //
    //             rotation = rotation * Quaternion.Euler(0.0f, 90.0f, 0.0f);
    //             
    //             
    //             
    //             
    //             
    //             var Bone = Helpers.RecursiveFindChild(transform, findbone.boneName);
    //             Debug.DrawLine(startPoint, endPoint, Color.green);
    //             //Debug.DrawLine(Bone.position, Bone.position + endPoint - startPoint, Color.green);
    //            //Bone.position = startPoint;
    //
    //             Vector3 eular = rotation.eulerAngles;
    //             rotation = Quaternion.Euler(eular);
    //             //Bone.rotation = rotation;
    //             
    //             animator.SetBoneLocalRotation(System.Enum.Parse<HumanBodyBones>(BoneName, true), rotation);
    //             //animator.avatar.humanDescription.skeleton[boneIndex].scale = ;
    //         }
    //         
    //         
    //     }
    //     
    //     // if (animator.avatar.isHuman)
    //     // {
    //     //     Debug.Log(animator.avatar.name + " is Human");
    //     //
    //     //     foreach (var h in animator.avatar.humanDescription.human)
    //     //     {
    //     //         Debug.Log(h.boneName);
    //     //         Debug.Log(h.humanName);
    //     //
    //     //         if (h.humanName)
    //     //         {
    //     //             
    //     //         }
    //     //     }
    //     //     Debug.Log(animator.avatar.humanDescription.skeleton);
    //     // }
    //
    //     
    // }
}
