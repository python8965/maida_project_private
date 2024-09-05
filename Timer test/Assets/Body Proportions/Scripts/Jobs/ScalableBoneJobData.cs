#if !UNITY_WEBGL
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace OnlyNew.BodyProportions
{
    public unsafe class ScalableBoneJobData : IDisposable
    {
        //From parent ScalableBone
        public NativeArrayPlus<TypeOfChange> parentTypeOfChangeArray;
        public NativeArrayPlus<int> parentIndexArray;

        //From parent Transform access

        public NativeArrayPlus<float3> parentPositionArray;
        public NativeArrayPlus<quaternion> parentRotationArray;
        public NativeArrayPlus<float3> parentScaleArray;
        public NativeArrayPlus<bool> parentTransformedArray;
        public NativeArrayPlus<float4x4> parentLocalToWorldMatrixArray;

        //From TransformAccess
        public NativeArrayPlus<float3> currentPositionArray;
        public NativeArrayPlus<quaternion> currentRotationArray;
        public NativeArrayPlus<float3> currentScaleArray;
        public NativeArrayPlus<float3> lastPositionArray;
        public NativeArrayPlus<quaternion> lastRotationArray;
        public NativeArrayPlus<float3> lastScaleArray;
        public NativeArrayPlus<bool> transformedArray;
        public NativeArrayPlus<float4x4> localToWorldMatrixArray;

        //From ScalableBone
        public NativeArrayPlus<bool> hasParentArray;
        public NativeArrayPlus<bool> isBeingDraggedArray;
        public NativeArrayPlus<float3> positionOffsetArray;
        public NativeArrayPlus<quaternion> relativeRotationArray;
        public NativeArrayPlus<float3> originalLocalPositionArray;
        public NativeArrayPlus<TypeOfChange> typeOfChangeArray;
        public NativeArrayPlus<bool> bindPositionArray;
        public NativeArrayPlus<bool> BindRotationArray;
        public NativeArrayPlus<bool> enableArray;
        //From ScalableBone history
        public NativeArrayPlus<float3> prePositionOffsetArray;

        //Flag
        public NativeArrayPlus<bool> enableRefreshRotationArray;
        public NativeArrayPlus<bool> enableFollowParentArray;
        public NativeArrayPlus<bool> undoArray;


        //From Parent
        private void* _parentIndexBuffer;
        private void* _isBeingDraggedBuffer;
        private void* _positionOffsetBuffer;
        private void* _hasParentBuffer;
        private void* _relativeRotationBuffer;
        private void* _originalLocalPositionBuffer;
        private void* _bindPositionBuffer;
        private void* _bindRotationBuffer;
        private void* _enableBuffer;
        private int _length = 0;
        public int Length => _length;
        public bool isExist = false;
        public ScalableBoneJobData(int length, Allocator allocator)
        {
            Init(length, allocator);
        }

        private void Init(int length, Allocator allocator)
        {
            _length = length;
            parentTypeOfChangeArray = new NativeArrayPlus<TypeOfChange>(length, allocator);
            parentPositionArray = new NativeArrayPlus<float3>(length, allocator);
            parentRotationArray = new NativeArrayPlus<quaternion>(length, allocator);
            parentScaleArray = new NativeArrayPlus<float3>(length, allocator);
            parentTransformedArray = new NativeArrayPlus<bool>(length, allocator);
            parentLocalToWorldMatrixArray = new NativeArrayPlus<float4x4>(length, allocator);

            //From TransformAccess
            currentPositionArray = new NativeArrayPlus<float3>(length, allocator);
            currentRotationArray = new NativeArrayPlus<quaternion>(length, allocator);
            currentScaleArray = new NativeArrayPlus<float3>(length, allocator);
            lastPositionArray = new NativeArrayPlus<float3>(length, allocator);
            lastRotationArray = new NativeArrayPlus<quaternion>(length, allocator);
            lastScaleArray = new NativeArrayPlus<float3>(length, allocator);
            transformedArray = new NativeArrayPlus<bool>(length, allocator);
            localToWorldMatrixArray = new NativeArrayPlus<float4x4>(length, allocator);

            //From ScalableBone
            hasParentArray = new NativeArrayPlus<bool>(length, allocator);
            isBeingDraggedArray = new NativeArrayPlus<bool>(length, allocator);
            positionOffsetArray = new NativeArrayPlus<float3>(length, allocator);
            relativeRotationArray = new NativeArrayPlus<quaternion>(length, allocator);
            originalLocalPositionArray = new NativeArrayPlus<float3>(length, allocator);
            bindPositionArray = new NativeArrayPlus<bool>(length, allocator);
            BindRotationArray = new NativeArrayPlus<bool>(length, allocator);
            enableArray = new NativeArrayPlus<bool>(length, allocator);

            //From ScalableBone history
            prePositionOffsetArray = new NativeArrayPlus<float3>(length, allocator);

            parentIndexArray = new NativeArrayPlus<int>(length, allocator);
            enableRefreshRotationArray = new NativeArrayPlus<bool>(length, allocator);
            enableFollowParentArray = new NativeArrayPlus<bool>(length, allocator);
            undoArray = new NativeArrayPlus<bool>(length, allocator);
            typeOfChangeArray = new NativeArrayPlus<TypeOfChange>(length, allocator);

            InitTotalNativeArray();
            isExist = true;
        }

        public void AddSubArray(int length)
        {
            parentTypeOfChangeArray.AddSubArray(length);
            parentPositionArray.AddSubArray(length);
            parentRotationArray.AddSubArray(length);
            parentScaleArray.AddSubArray(length);
            parentTransformedArray.AddSubArray(length);
            parentLocalToWorldMatrixArray.AddSubArray(length);

            //From TransformAccess
            currentPositionArray.AddSubArray(length);
            currentRotationArray.AddSubArray(length);
            currentScaleArray.AddSubArray(length);
            lastPositionArray.AddSubArray(length);
            lastRotationArray.AddSubArray(length);
            lastScaleArray.AddSubArray(length);
            transformedArray.AddSubArray(length);
            localToWorldMatrixArray.AddSubArray(length);

            //From ScalableBone
            hasParentArray.AddSubArray(length);
            isBeingDraggedArray.AddSubArray(length);
            positionOffsetArray.AddSubArray(length);
            relativeRotationArray.AddSubArray(length);
            originalLocalPositionArray.AddSubArray(length);
            typeOfChangeArray.AddSubArray(length);
            bindPositionArray.AddSubArray(length);
            BindRotationArray.AddSubArray(length);
            enableArray.AddSubArray(length);
            //From ScalableBone history
            prePositionOffsetArray.AddSubArray(length);
            parentIndexArray.AddSubArray(length);

            enableRefreshRotationArray.AddSubArray(length);
            enableFollowParentArray.AddSubArray(length);
            undoArray.AddSubArray(length);


        }

        public void Dispose()
        {
            if (isExist)
            {

                parentTypeOfChangeArray.Dispose();
                parentPositionArray.Dispose();
                parentRotationArray.Dispose();
                parentScaleArray.Dispose();
                parentTransformedArray.Dispose();
                parentLocalToWorldMatrixArray.Dispose();

                //From TransformAccess
                currentPositionArray.Dispose();
                currentRotationArray.Dispose();
                currentScaleArray.Dispose();
                lastPositionArray.Dispose();
                lastRotationArray.Dispose();
                lastScaleArray.Dispose();
                transformedArray.Dispose();
                localToWorldMatrixArray.Dispose();

                //From ScalableBone
                hasParentArray.Dispose();
                isBeingDraggedArray.Dispose();
                positionOffsetArray.Dispose();
                relativeRotationArray.Dispose();
                originalLocalPositionArray.Dispose();
                typeOfChangeArray.Dispose();
                bindPositionArray.Dispose();
                BindRotationArray.Dispose();
                enableArray.Dispose();
                //From ScalableBone history
                prePositionOffsetArray.Dispose();
                parentIndexArray.Dispose();

                enableRefreshRotationArray.Dispose();
                enableFollowParentArray.Dispose();
                undoArray.Dispose();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncScalableBoneDynamicData(ScalableBone scalableBone, int totalIndex)
        {
            UnsafeUtility.WriteArrayElement(_isBeingDraggedBuffer, totalIndex, scalableBone.IsBeingDragged);
            UnsafeUtility.WriteArrayElement(_positionOffsetBuffer, totalIndex, scalableBone.positionOffset);
            UnsafeUtility.WriteArrayElement(_bindPositionBuffer, totalIndex, scalableBone.bindPosition);
            UnsafeUtility.WriteArrayElement(_bindRotationBuffer, totalIndex, scalableBone.bindRotation);
            UnsafeUtility.WriteArrayElement(_enableBuffer, totalIndex, scalableBone.Enabled);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncScalableBoneDynamicDataSimple(ScalableBone scalableBone, int totalIndex)
        {
            UnsafeUtility.WriteArrayElement(_positionOffsetBuffer, totalIndex, scalableBone.positionOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitScalableBoneData(ScalableBone scalableBone, int totalIndex)
        {
            //_hasParentArray[totalIndex] = scalableBone.parentScalableBone != null;
            //_positionOffsetArray[totalIndex] = scalableBone.positionOffset;
            //_relativeRotationArray[totalIndex] = scalableBone.relativeRotation;
            //_originalLocalPositionArray[totalIndex] = scalableBone.OriginalLocalPosition;
            UnsafeUtility.WriteArrayElement(_hasParentBuffer, totalIndex, scalableBone.parentScalableBone != null);
            UnsafeUtility.WriteArrayElement(_positionOffsetBuffer, totalIndex, scalableBone.positionOffset);
            UnsafeUtility.WriteArrayElement(_isBeingDraggedBuffer, totalIndex, scalableBone.IsBeingDragged);
            UnsafeUtility.WriteArrayElement(_originalLocalPositionBuffer, totalIndex, scalableBone.OriginalLocalPosition);
            UnsafeUtility.WriteArrayElement(_bindPositionBuffer, totalIndex, scalableBone.bindPosition);
            UnsafeUtility.WriteArrayElement(_bindRotationBuffer, totalIndex, scalableBone.bindRotation);
            UnsafeUtility.WriteArrayElement(_enableBuffer, totalIndex, scalableBone.Enabled);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetParentIndex(int index, int parentIndex)
        {
            UnsafeUtility.WriteArrayElement(_parentIndexBuffer, index, parentIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteEnableArray(ScalableBone scalableBone, int totalIndex, bool enabled)
        {
            UnsafeUtility.WriteArrayElement(_enableBuffer, totalIndex, enabled);
        }

        private void InitTotalNativeArray()
        {
            _parentIndexBuffer = parentIndexArray.GetTotalNativeArray().GetUnsafePtr();
            _isBeingDraggedBuffer = isBeingDraggedArray.GetTotalNativeArray().GetUnsafePtr();
            _positionOffsetBuffer = positionOffsetArray.GetTotalNativeArray().GetUnsafePtr();
            _originalLocalPositionBuffer = originalLocalPositionArray.GetTotalNativeArray().GetUnsafePtr();
            _hasParentBuffer = hasParentArray.GetTotalNativeArray().GetUnsafePtr();
            _relativeRotationBuffer = relativeRotationArray.GetTotalNativeArray().GetUnsafePtr();
            _bindPositionBuffer = bindPositionArray.GetTotalNativeArray().GetUnsafePtr();
            _bindRotationBuffer = BindRotationArray.GetTotalNativeArray().GetUnsafePtr();
            _enableBuffer = enableArray.GetTotalNativeArray().GetUnsafePtr();


        }
    }
}
#endif