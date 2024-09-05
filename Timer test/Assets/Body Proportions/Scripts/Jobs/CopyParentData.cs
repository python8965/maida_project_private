#if !UNITY_WEBGL
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace OnlyNew.BodyProportions
{
    [Unity.Burst.BurstCompile]
    public struct CopyParentData<T> : IJobParallelFor where T : struct
    {
        [ReadOnly]
        public NativeArray<T> dataFromParent;
        [ReadOnly]
        public NativeArray<bool> hasParentArray;
        [ReadOnly]
        public NativeArray<int> parentIndexArray;
        [WriteOnly]
        public NativeArray<T> parentDataForChildren;

        public void Execute(int index)
        {
            if (hasParentArray[index])
            {
                parentDataForChildren[index] = dataFromParent[parentIndexArray[index]];
            }
        }
        public CopyParentData<T1> InitJob<T1>(int i, int length, NativeArrayPlus<T1> dataFromParent,
            NativeArrayPlus<T1> parentDataForChildren,
            NativeArrayPlus<int> parentIndexArray,
            NativeArrayPlus<bool> hasParentArray) where T1 : struct
        {
            var job = new CopyParentData<T1>
            {
                dataFromParent = dataFromParent.GetSubArray(i - 1),
                parentDataForChildren = parentDataForChildren.GetSubArray(i),
                parentIndexArray = parentIndexArray.GetSubArray(i),
                hasParentArray = hasParentArray.GetSubArray(i)
            };
            return job;
        }
    }
    [Unity.Burst.BurstCompile]
    public struct CopyParentTransform : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float3> currentPositionFromParent;
        [ReadOnly]
        public NativeArray<quaternion> currentRotationFromParent;
        [ReadOnly]
        public NativeArray<float3> currentScaleFromParent;
        [ReadOnly]
        public NativeArray<int> parentIndexArray;
        [ReadOnly]
        public NativeArray<bool> hasParentArray;
        [WriteOnly]
        public NativeArray<float3> parentPosition;
        [WriteOnly]
        public NativeArray<quaternion> parentRotation;
        [WriteOnly]
        public NativeArray<float3> parentScale;
        public void Execute(int index)
        {
            if (hasParentArray[index])
            {
                var i = parentIndexArray[index];
                parentPosition[index] = currentPositionFromParent[i];
                parentRotation[index] = currentRotationFromParent[i];
                parentScale[index] = currentScaleFromParent[i];
            }
        }
    }
    [Unity.Burst.BurstCompile]
    public struct CopyParentPosAndRot : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float3> currentPositionFromParent;
        [ReadOnly]
        public NativeArray<quaternion> currentRotationFromParent;
        [ReadOnly]
        public NativeArray<int> parentIndexArray;
        [ReadOnly]
        public NativeArray<bool> hasParentArray;
        [WriteOnly]
        public NativeArray<float3> parentPosition;
        [WriteOnly]
        public NativeArray<quaternion> parentRotation;
        public void Execute(int index)
        {
            if (hasParentArray[index])
            {
                var i = parentIndexArray[index];
                parentPosition[index] = currentPositionFromParent[i];
                parentRotation[index] = currentRotationFromParent[i];
            }
        }
    }
    [Unity.Burst.BurstCompile]
    public struct CopyParentDataAll : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float3> currentPositionFromParent;
        [ReadOnly]
        public NativeArray<quaternion> currentRotationFromParent;
        [ReadOnly]
        public NativeArray<float3> currentScaleFromParent;
        [ReadOnly]
        public NativeArray<TypeOfChange> typeOfChangeFromParent;
        [ReadOnly]
        public NativeArray<bool> transformedFromParent;
        [ReadOnly]
        public NativeArray<int> parentIndexArray;
        [ReadOnly]
        public NativeArray<bool> hasParentArray;
        [WriteOnly]
        public NativeArray<float3> parentPosition;
        [WriteOnly]
        public NativeArray<quaternion> parentRotation;
        [WriteOnly]
        public NativeArray<TypeOfChange> parentTypeOfChange;
        [WriteOnly]
        public NativeArray<float3> parentScale;
        [WriteOnly]
        public NativeArray<bool> parentTransformed;
        public void Execute(int index)
        {
            if (hasParentArray[index])
            {
                var i = parentIndexArray[index];
                parentPosition[index] = currentPositionFromParent[i];
                parentRotation[index] = currentRotationFromParent[i];
                parentScale[index] = currentScaleFromParent[i];
                parentTypeOfChange[index] = typeOfChangeFromParent[i];
                parentTransformed[index] = transformedFromParent[i];
            }
        }
    }
}
#endif