#define BODY_PROPORTIONS
using System;
using System.Collections.Generic;
using UnityEngine;
namespace OnlyNew.BodyProportions
{
    [ExecuteAlways]
    [DefaultExecutionOrder(-20)]
    [DisallowMultipleComponent]
    [Serializable]
    public class ScalableBone : MonoBehaviour
    {

        [Tooltip("The bone which it follows. Usually it is configured by ScalableBonesManager.")]
        public ScalableBone parentScalableBone;
        [Tooltip("Original parent before setup")]
        [HideInInspector, SerializeField]
        private Transform parent;

        [Tooltip("When the value is true, it follows the parentScalableBone component.")]
        public bool bindPosition = true;
        [Tooltip("When the value is true, it rotates with the parentScalableBone component.")]
        public bool bindRotation = true;


        [Tooltip("Offset from original position")]
        public Vector3 positionOffset = Vector3.zero;
        public bool visible = true;
        private Vector3 prePositionOffset = Vector3.zero;

        [HideInInspector, SerializeField]
        private Vector3 originalLocalPosition;

        /// <summary>
        /// relative Rotation between this object and its parent
        /// </summary>

        [HideInInspector, SerializeField]
        public Quaternion relativeRotation;

        private Vector3 preLocalPosition, preLocalScale;

        [HideInInspector, SerializeField]
        private Quaternion preLocalRotation;


        [HideInInspector, SerializeField]
        private bool initialized = false;
        private bool transformed = false;

        public bool IsRegistered { set; get; }

        [HideInInspector]
        public int depth = 0;
        [HideInInspector]
        public int index = 0;
        [HideInInspector]
        public int totalIndex = 0;
        [HideInInspector]
        public int parentIndex = 0;
        [HideInInspector]
        public bool sorted = false;

        /// <summary>
        /// transformed by animation or manipulation
        /// </summary>
        public bool Transformed
        {
            get => transformed;
            //TODO:delete after Debug
            set => transformed = value;
        }

        public bool IsBeingDragged
        {
            get
            {
                if (isBeingDragged)
                {
                    isBeingDragged = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            set => isBeingDragged = value;
        }
        public Vector3 OriginalLocalPosition { get => originalLocalPosition; }
        private bool isBeingDragged = false;
        internal TypeOfChange typeOfChange = TypeOfChange.None;
        TypeOfChange preTypeOfChange = TypeOfChange.None;
        [HideInInspector, SerializeField]
        public ScalableBonesManager scalableBonesManager = null;
        [HideInInspector, SerializeField]
        public List<ScalableBone> childrenScalableBone = new List<ScalableBone>();
        [HideInInspector, SerializeField]
        public int currentVersion = 0;

        [HideInInspector, SerializeField]
        private bool hasParent = false;

        public bool HasParent => hasParent;
        public bool Enabled { set; get; }

        private void Awake()
        {
            if (!initialized)
            {
                if (parent == null)
                {
                    parent = transform.parent;
                }
                if (parent != null)
                {
                    originalLocalPosition = transform.localPosition;
                    relativeRotation = Quaternion.Inverse(parent.rotation) * transform.rotation;
                    parentScalableBone = parent.GetComponent<ScalableBone>();
                    parentScalableBone?.AddChildren(this);
                    hasParent = true;
                }
                currentVersion = VersionInfo.dataVersion;
                initialized = true;
            }
            if (currentVersion < VersionInfo.dataVersion)
            {
                if (parentScalableBone != null)
                {
                    parentScalableBone.AddChildren(this);
                    hasParent = true;
                }
                currentVersion = VersionInfo.dataVersion;
            }
        }
        private void OnDestroy()
        {
            scalableBonesManager?.OnDestroyScalableBone(this);
        }
        private bool CheckIfIChanged()
        {
            return transform.localPosition != preLocalPosition ||
                   !(transform.localRotation == preLocalRotation) ||
                   transform.localScale != preLocalScale;
        }
        public void UpdateAll()
        {
            BeforeUpdateBone();
            OnUpdateBone();
            AfterUpdateBone();
        }
        /// <summary>
        /// We will check if transform of the bone have changed in this method.
        /// </summary>
        public void BeforeUpdateBone()
        {
            //transformed = false;
            transformed = CheckIfIChanged();

        }
        /// <summary>
        /// We will decide how to move and rotate the object based on its state and its superiors.
        /// </summary>
        public void OnUpdateBone()
        {
            if (parent == null) return;
            if (parentScalableBone == null)
            {
                parentScalableBone = parent.GetComponent<ScalableBone>();
            }
            typeOfChange = TypeOfChange.None;
            if (isBeingDragged)
            {
                typeOfChange = TypeOfChange.IsBeingDragged;
            }
            if (parentScalableBone != null)
            {

                if (!isBeingDragged)
                {
                    if (parentScalableBone.typeOfChange == TypeOfChange.IsBeingDragged || parentScalableBone.typeOfChange == TypeOfChange.ParentOrGrandParentIsBeingDragged)
                    {
                        typeOfChange = TypeOfChange.ParentOrGrandParentIsBeingDragged;
                    }
                    else
                    {
                        if (parentScalableBone.Transformed)
                        {
                            typeOfChange = TypeOfChange.Moving;
                        }
                        else
                        {
                            if (!this.Transformed)
                            {
                                if (parentScalableBone.typeOfChange != TypeOfChange.None)
                                {
                                    typeOfChange = TypeOfChange.Moving;
                                }
                                else
                                {
                                    if (positionOffset != prePositionOffset)
                                    {
                                        typeOfChange = TypeOfChange.OnlyOffsetChanged;
                                    }
                                    else
                                    {
                                        typeOfChange = TypeOfChange.None;
                                    }
                                }

                            }
                            else
                            {
                                typeOfChange = TypeOfChange.Moving;
                            }
                        }
                    }
                }

                //TODO : 8*8=64
                switch (typeOfChange)
                {
                    case TypeOfChange.None:
                        break;
                    case TypeOfChange.Moving:
                        RefreshRelativeRotation();
                        FollowParentBone();
                        break;
                    case TypeOfChange.ParentOrGrandParentIsBeingDragged:
#if UNITY_EDITOR
                        UnityEditor.Undo.RecordObject(transform, "childbone rotation");
#endif
                        FollowParentBone();
                        break;
                    case TypeOfChange.OnlyOffsetChanged:
                        FollowParentBone();
                        break;
                    default:
                        break;
                }
            }
            isBeingDragged = false;
        }
        /// <summary>
        /// We will refresh historical data in this method.
        /// </summary>
        public void AfterUpdateBone()
        {
            UpdateHistory();
        }
        private void UpdateHistory()
        {

            preLocalPosition = transform.localPosition;
            preLocalRotation = transform.localRotation;
            preLocalScale = transform.localScale;
            prePositionOffset = positionOffset;
            preTypeOfChange = typeOfChange;

        }


        private void RefreshRelativeRotation()
        {
            if (parent != null)
            {
                relativeRotation = Quaternion.Inverse(parent.transform.rotation) * transform.rotation;
            }
        }

        private void FollowParentBone()
        {
            if (!initialized)
            {
                Awake();
            }

            if (parent != null)
            {
                if (bindPosition)
                {
                    //fix position according to original localposition with parent
                    transform.position = parent.TransformPoint(originalLocalPosition + positionOffset);
                }

                if (bindRotation)
                {
                    //fix rotation according to relativeRotation with parent
                    transform.rotation = parent.rotation * relativeRotation;
                }
            }
        }

        internal void SetIndex(int t_index)
        {
            index = t_index;
        }
        //TODO:support undo operation
        internal int UpdateDepth()
        {
            ScalableBone bone = this;
            int tDepth = 0;
            if (bone.parentScalableBone == null)
                bone.parentScalableBone = bone.parent.GetComponent<ScalableBone>();
            while (bone.parentScalableBone != null)
            {
                tDepth++;
                bone = bone.parentScalableBone;
                if (bone.parentScalableBone == null)
                    bone.parentScalableBone = bone.parent.GetComponent<ScalableBone>();
            }

            depth = tDepth;
            return depth;
        }

        internal void UpdateParentIndex()
        {
            if (parentScalableBone)
                parentIndex = parentScalableBone.index;
        }

        private void LogEachFrame(params object[] args)
        {

            var str = "";
            for (int i = 0; i < args.Length; i++)
            {
                str += "," + args[i].ToString();
            }

            Debug.Log(Time.frameCount + ":" + gameObject.name + ":" + str);


        }

        public void SetParent(Transform t_parent)
        {
            parent = t_parent;
            parentScalableBone = parent.GetComponent<ScalableBone>();
        }
        /// <summary>
        /// replace "transform.position=value" with "SetPosition(value)",if you hope its children bones move with this bone
        /// </summary>
        /// <param name="pos"></param>
        public void SetPosition(Vector3 pos)
        {
            isBeingDragged = true;
            transform.position = pos;
        }
        /// <summary>
        /// replace "transform.rotation=value" with "SetRotation(value)",if you hope its children bones rotate with this bone
        /// </summary>
        /// <param name="rot"></param>
        public void SetRotation(Quaternion rot)
        {
            isBeingDragged = true;
            transform.rotation = rot;
        }
        internal void AddChildren(ScalableBone scalableBone)
        {
            if (!childrenScalableBone.Contains(scalableBone))
                childrenScalableBone.Add(scalableBone);
        }
    }

    public enum TypeOfChange
    {
        None,
        IsBeingDragged,
        Moving,
        ParentOrGrandParentIsBeingDragged,
        OnlyOffsetChanged
    }
}