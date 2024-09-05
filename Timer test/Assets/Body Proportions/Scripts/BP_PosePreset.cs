using ScriptableDictionaries;
using System;
using UnityEngine;
namespace OnlyNew.BodyProportions
{
    public class BP_PosePreset : ScriptableDictionary<string, BoneTransformData>
    {
    }

    [Serializable]
    public struct BoneTransformData
    {
        public BoneTransformData(Vector3 pos, Quaternion rot, Vector3 scl)
        {
            this.relativeLocalPosition = pos;
            this.relativeLocalRotation = rot;
            this.localScale = scl;
        }
        public Vector3 relativeLocalPosition;
        public Quaternion relativeLocalRotation;
        public Vector3 localScale;
    }
}