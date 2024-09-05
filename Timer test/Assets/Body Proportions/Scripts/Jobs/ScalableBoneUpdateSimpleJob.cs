#if !UNITY_WEBGL
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace OnlyNew.BodyProportions
{
    [Unity.Burst.BurstCompile]
    public struct ScalableBoneUpdateSimpleJob : IJobParallelForTransform
    {
        [ReadOnly]
        public NativeArray<bool> hasParent;
        [ReadOnly]
        public NativeArray<float3> parentPosition;
        [ReadOnly]
        public NativeArray<quaternion> parentRotation;
        [ReadOnly]
        public NativeArray<float3> parentScale;
        [ReadOnly]
        public NativeArray<float3> originalLocalPosition;
        [ReadOnly]
        public NativeArray<float3> positionOffset;
        [ReadOnly]
        public NativeArray<bool> bindPosition;
        [ReadOnly] public NativeArray<bool> enable;

        public NativeArray<float3> currentPosition;
        public NativeArray<quaternion> currentRotation;
        public NativeArray<float3> currentScale;




        public void Execute(int index, TransformAccess transform)
        {
            if (!enable[index])
                return;
            {
                currentPosition[index] = transform.position;
                currentRotation[index] = transform.rotation;
                currentScale[index] = transform.localScale;
            }
            if (hasParent[index] && bindPosition[index])
            {
                float4x4 _localToWorldMatrix = float4x4.TRS(parentPosition[index], parentRotation[index], parentScale[index]);
                float3 _originalLocalPosition = originalLocalPosition[index];
                float3 _positionOffset = positionOffset[index];
                float3 pos = _originalLocalPosition + _positionOffset;
                float4 pos4 = new float4(pos, 1);
                float4 result = math.mul(_localToWorldMatrix, pos4);
                currentPosition[index] = result.xyz;
                transform.position = currentPosition[index];
            }
        }
    }



}
#endif