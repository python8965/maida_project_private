#if !UNITY_WEBGL
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace OnlyNew.BodyProportions
{
    [Unity.Burst.BurstCompile]
    public struct CalculateAllJob : IJobParallelForTransform
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
        public NativeArray<TypeOfChange> parentTypeOfChange;
        [ReadOnly]
        public NativeArray<bool> parentTransformed;
        [ReadOnly]
        public NativeArray<float3> originalLocalPosition;
        [ReadOnly]
        public NativeArray<float3> positionOffset;
        [ReadOnly]
        public NativeArray<bool> bindPosition;
        [ReadOnly]
        public NativeArray<bool> bindRotation;

        [ReadOnly] public NativeArray<bool> enable;

        [WriteOnly]
        public NativeArray<TypeOfChange> typeOfChange;

        public NativeArray<float3> currentPosition;
        public NativeArray<quaternion> currentRotation;
        public NativeArray<float3> currentScale;

        public NativeArray<float3> lastPosition;
        public NativeArray<quaternion> lastRotation;
        public NativeArray<float3> lastScale;

        public NativeArray<bool> transformed;
        public NativeArray<bool> isBeingDragged;
        public NativeArray<quaternion> relativeRotation;
        public NativeArray<float3> prePositionOffset;

        public NativeArray<bool> enableRefreshRelativeRotation;
        public NativeArray<bool> enableFollowParent;
        public NativeArray<bool> undoRecord;


        public void Execute(int index, TransformAccess transform)
        {
            if (!enable[index])
                return;

            //read transform
            {
                currentPosition[index] = transform.position;
                currentRotation[index] = transform.rotation;
                currentScale[index] = transform.localScale;
            }
            //check if transform change 
            {
                var pos = currentPosition[index];
                var rot = currentRotation[index];
                var scale = currentScale[index];
                transformed[index] = !IsEqual(pos, lastPosition[index]) ||
                                     !IsEqual(rot, lastRotation[index]) ||
                                     !IsEqual(scale, lastScale[index]);
            }

            var _enableRefreshRelativeRotation = false;
            var _enableFollowParent = false;
            //calculate flag
            {
                var _isBeingDragged = isBeingDragged[index];
                var _transformed = transformed[index];
                var _parentTransformed = parentTransformed[index];
                var _parentTypeOfChange = parentTypeOfChange[index];
                var _positionOffset = positionOffset[index];
                var _prePositionOffset = prePositionOffset[index];
                var _hasParent = hasParent[index];
                isBeingDragged[index] = false;
                var _typeOfChange = TypeOfChange.None;
                if (_isBeingDragged)
                {
                    _typeOfChange = TypeOfChange.IsBeingDragged;
                }
                if (_hasParent)
                {
                    if (!_isBeingDragged)
                    {
                        if (_parentTypeOfChange == TypeOfChange.IsBeingDragged || _parentTypeOfChange == TypeOfChange.ParentOrGrandParentIsBeingDragged)
                        {
                            _typeOfChange = TypeOfChange.ParentOrGrandParentIsBeingDragged;
                        }
                        else
                        {
                            if (_parentTransformed)
                            {
                                _typeOfChange = TypeOfChange.Moving;
                            }
                            else
                            {
                                if (!_transformed)
                                {
                                    if (_parentTypeOfChange != TypeOfChange.None)
                                    {
                                        _typeOfChange = TypeOfChange.Moving;
                                    }
                                    else
                                    {
                                        if (!_positionOffset.Equals(_prePositionOffset))
                                        {
                                            _typeOfChange = TypeOfChange.OnlyOffsetChanged;
                                        }
                                        else
                                        {
                                            _typeOfChange = TypeOfChange.None;
                                        }
                                    }

                                }
                                else
                                {
                                    _typeOfChange = TypeOfChange.Moving;
                                }

                            }
                        }
                    }
                    switch (_typeOfChange)
                    {
                        case TypeOfChange.None:
                            break;
                        case TypeOfChange.IsBeingDragged:
                        case TypeOfChange.Moving:
                            _enableRefreshRelativeRotation = true;
                            _enableFollowParent = true;
                            break;
                        case TypeOfChange.ParentOrGrandParentIsBeingDragged:
#if UNITY_EDITOR
                            undoRecord[index] = true;
                            //Undo.RecordObject(transform, "childbone rotation");
#endif
                            _enableFollowParent = true;
                            break;
                        case TypeOfChange.OnlyOffsetChanged:
                            _enableFollowParent = true;
                            break;
                        default:
                            break;
                    }
                }
                isBeingDragged[index] = false;
                typeOfChange[index] = _typeOfChange;

            }
            //refresh relativeRotation
            if (_enableRefreshRelativeRotation)
            {
                relativeRotation[index] = math.mul(math.inverse(parentRotation[index]), currentRotation[index]);
            }
            //calculate rotation
            if (_enableFollowParent && !_enableRefreshRelativeRotation && bindRotation[index])
            {
                var rot = math.mul(parentRotation[index], relativeRotation[index]);
                currentRotation[index] = rot;
            }
            //calculate position
            if (_enableFollowParent && bindPosition[index])
            {
                float4x4 _localToWorldMatrix = float4x4.TRS(parentPosition[index], parentRotation[index], parentScale[index]);
                float3 _originalLocalPosition = originalLocalPosition[index];
                float3 _positionOffset = positionOffset[index];
                float3 pos = _originalLocalPosition + _positionOffset;
                float4 pos4 = new float4(pos, 1);
                float4 result = math.mul(_localToWorldMatrix, pos4);
                currentPosition[index] = result.xyz;
            }
            //update history
            {
                lastPosition[index] = currentPosition[index];
                lastRotation[index] = currentRotation[index];
                lastScale[index] = currentScale[index];
                prePositionOffset[index] = positionOffset[index];
            }
            //output flag
            {
                enableFollowParent[index] = _enableFollowParent;
                enableRefreshRelativeRotation[index] = _enableRefreshRelativeRotation;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEqual(float3 lhs, float3 rhs)
        {
            float num1 = lhs.x - rhs.x;
            float num2 = lhs.y - rhs.y;
            float num3 = lhs.z - rhs.z;
            return (double)num1 * (double)num1 + (double)num2 * (double)num2 + (double)num3 * (double)num3 < 9.9999994396249292E-11;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEqual(quaternion lhs, quaternion rhs)
        {
            var dot = math.dot(lhs, rhs);
            return (double)dot > 0.99999898672103882;
        }
    }

}
#endif