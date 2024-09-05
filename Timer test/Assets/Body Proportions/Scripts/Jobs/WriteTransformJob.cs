using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace OnlyNew.BodyProportions
{
    [Unity.Burst.BurstCompile]
    public struct WriteTransformJob : IJobParallelForTransform
    {

        [ReadOnly]
        public NativeArray<bool> bindPosition;
        [ReadOnly]
        public NativeArray<bool> bindRotation;
        [ReadOnly]
        public NativeArray<float3> currentPosition;
        [ReadOnly]
        public NativeArray<quaternion> currentRotation;
        [ReadOnly] public NativeArray<bool> enable;

        public NativeArray<bool> enableFollowParent;
        public void Execute(int index, TransformAccess transform)
        {
            if (!enable[index])
                return;
            if (enableFollowParent[index])
            {
                if (bindPosition[index])
                    transform.position = currentPosition[index];
                if (bindRotation[index])
                    transform.rotation = currentRotation[index];
                enableFollowParent[index] = false;
            }
        }
    }
}