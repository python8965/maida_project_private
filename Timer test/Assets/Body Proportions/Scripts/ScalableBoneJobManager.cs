#if !UNITY_WEBGL
#if UNITY_EDITOR
#define EDITOR_JOB
#endif
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace OnlyNew.BodyProportions
{

    [ExecuteAlways]
    [DefaultExecutionOrder(-14)]
    public partial class ScalableBoneJobManager : MonoBehaviour
    {
        internal List<List<ScalableBone>> scalableBoneDepthLists = new List<List<ScalableBone>>();
        internal List<ScalableBone> scalableBones = new List<ScalableBone>();
        internal bool dirty = false;
        private static ScalableBoneJobManager _instance;
        public static bool IsExist { get { return _instance != null; } }
        public static ScalableBoneJobManager Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<ScalableBoneJobManager>(true);
                    if (_instance == null)
                    {
                        var obj = new GameObject("Scalable Bone Job Manager");
                        _instance = obj.AddComponent<ScalableBoneJobManager>();
                        Debug.Log("As you enable multi-threading mode, ScalableBonesManager automatically created the Scalable Bone Job Manager.");
#if UNITY_EDITOR
                        if (Application.isPlaying == false)
                            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif
                    }
                }
                return _instance;
            }
        }

        private bool transformAccessIsExist = false;
        private bool transformAccessArraysIsExist = false;
        [Tooltip("If you want the job manager to stop working, sleep is faster than disable.")]
        public bool sleep = false;
        [Tooltip("Fast Mode is lightweight. It is equivalent to setting all bindRotation to false. So Fast Mode is not suitable for making animation while application is playing.")]
        public bool fastMode = false;

        public int BoneCount { get { return scalableBones.Count; } }
        public int MaxDepth { get { return scalableBoneDepthLists.Count; } }
        public static bool IsInitilized()
        {
            if (_instance != null)
                return true;
            var i = Instance;
            return i != null;
        }
        protected virtual void Awake()
        {
            //if (Application.isPlaying)
            //    DontDestroyOnLoad(this);
            Refresh();
        }

        private void OnEnable()
        {
            dirty = true;
        }
        private void OnDisable()
        {
            scalableBoneJobData?.Dispose();
            DisposeTransformAccess();
            DisposeTransformArrays();
        }
        void Refresh()
        {
            var managers = FindObjectsByType<ScalableBonesManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var manager in managers)
            {
                if (manager.multiThreadedMode)
                    manager.RefreshJobData();
            }
        }
        private void OnDestroy()
        {
            var bones = FindObjectsByType<ScalableBone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var bone in bones)
            {
                bone.IsRegistered = false;
            }
        }
        public void DisposeTransformAccess()
        {
            if (transformAccessIsExist)
            {
                if (transformAccessArray.isCreated)
                    transformAccessArray.Dispose();
                transformAccessIsExist = false;
            }
        }
        // Update is called once per frame
        void LateUpdate()
        {
            try
            {
                if (sleep) return;
                if (scalableBoneDepthLists.Count == 0) return;
                RebuildAccess();
                if (scalableBones.Count == 0) return;
                UpdateEachFrame();
            }
            catch (Exception e)
            {
                Refresh();
                throw e;
            }
        }
        void RebuildAccess()
        {
            if (dirty)
            {
                RebuildDepthList();
                RebuildList();

                InitializeJobsArray();

                RemapParentIndex();
                totalCount = scalableBones.Count;
                scalableBoneJobData?.Dispose();
                scalableBoneJobData = new ScalableBoneJobData(totalCount, Allocator.Persistent);
                InitScalableBoneData();

                //TODO: remove from list while destroy scalableBone
                RebuildTransformAccessArrays();
#if EDITOR_JOB
                RebuildTransformAccessArrayForEditor();
#endif
                BuildJobs();

                dirty = false;
            }
        }

        private void RebuildTransformAccessArrayForEditor()
        {
            if (scalableBones != null)
            {
                DisposeTransformAccess();
                transformAccessArray = new TransformAccessArray(totalCount, innerLoopBatchCount);
                transformAccessIsExist = true;
                for (int i = 0; i < totalCount; i++)
                {
                    ScalableBone scalableBone = scalableBones[i];

                    transformAccessArray.Add(scalableBone.transform);
                    scalableBone.totalIndex = i;
                }

            }
        }

        private void RebuildTransformAccessArrays()
        {
            if (scalableBoneDepthLists != null)
            {
                var maxDepth = scalableBoneDepthLists.Count;
                if (maxDepth > 0)
                {
                    DisposeTransformArrays();

                    transformAccessArrays = new TransformAccessArray[maxDepth];

                    for (int i = 0; i < scalableBoneDepthLists.Count; i++)
                    {
                        var length = scalableBoneDepthLists[i].Count;
                        transformAccessArrays[i] = new TransformAccessArray(length, innerLoopBatchCount);
                        transformAccessArraysIsExist = true;
                        scalableBoneJobData.AddSubArray(scalableBoneDepthLists[i].Count);
                        var list = scalableBoneDepthLists[i];

                        for (int j = 0; j < length; j++)
                        {
                            var scalableBone = list[j];
                            transformAccessArrays[i].Add(scalableBone.transform);
                        }
                    }
                }
            }
        }

        private void DisposeTransformArrays()
        {
            if (transformAccessArraysIsExist)
            {
                if (transformAccessArrays != null)
                {
                    foreach (var array in transformAccessArrays)
                    {
                        if (array.isCreated)
                            array.Dispose();
                    }
                }
                transformAccessArraysIsExist = false;
            }
        }


        private void RebuildList()
        {
            scalableBones.Clear();
            foreach (var VARIABLE in scalableBoneDepthLists)
            {
                foreach (var scalableBone in VARIABLE)
                {
                    scalableBones.Add(scalableBone);
                }
            }
        }

        private void RebuildDepthList()
        {
            for (int i = scalableBoneDepthLists.Count - 1; i >= 0; i--)
            {
                if (scalableBoneDepthLists[i] == null)
                {
                    scalableBoneDepthLists.RemoveAt(i);
                }
                else
                {
                    if (scalableBoneDepthLists[i].Count == 0)
                    {
                        scalableBoneDepthLists.RemoveAt(i);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }


        private void InitializeJobsArray()
        {
            var count = scalableBoneDepthLists.Count;
            copyParentDataAll = new CopyParentDataAll[count];
            calculateAllJobs = new CalculateAllJob[count];
            scalableBoneUpdateJobs = new ScalableBoneUpdateJob[count];
            scalableBoneUpdateSimpleJobs = new ScalableBoneUpdateSimpleJob[count];

        }



        void BuildJobs()
        {

            var count = scalableBoneDepthLists.Count;
            if (count <= 0)
                return;
            for (var i = 0; i < count; i++)
            {
                var list = scalableBoneDepthLists[i];
                if (list != null)
                {
                    var length = list.Count;
                    if (i > 0)
                    {


                        copyParentDataAll[i] = new CopyParentDataAll()
                        {
                            currentPositionFromParent = scalableBoneJobData.currentPositionArray.GetSubArray(i - 1),
                            currentRotationFromParent = scalableBoneJobData.currentRotationArray.GetSubArray(i - 1),
                            currentScaleFromParent = scalableBoneJobData.currentScaleArray.GetSubArray(i - 1),
                            typeOfChangeFromParent = scalableBoneJobData.typeOfChangeArray.GetSubArray(i - 1),
                            transformedFromParent = scalableBoneJobData.transformedArray.GetSubArray(i - 1),
                            parentPosition = scalableBoneJobData.parentPositionArray.GetSubArray(i),
                            parentRotation = scalableBoneJobData.parentRotationArray.GetSubArray(i),
                            parentScale = scalableBoneJobData.parentScaleArray.GetSubArray(i),
                            parentTypeOfChange = scalableBoneJobData.parentTypeOfChangeArray.GetSubArray(i),
                            parentTransformed = scalableBoneJobData.parentTransformedArray.GetSubArray(i),
                            parentIndexArray = scalableBoneJobData.parentIndexArray.GetSubArray(i),
                            hasParentArray = scalableBoneJobData.hasParentArray.GetSubArray(i),

                        };

                    }
                    calculateAllJobs[i] = new CalculateAllJob
                    {
                        enableFollowParent = scalableBoneJobData.enableFollowParentArray.GetSubArray(i),
                        enableRefreshRelativeRotation = scalableBoneJobData.enableRefreshRotationArray.GetSubArray(i),
                        parentPosition = scalableBoneJobData.parentPositionArray.GetSubArray(i),
                        parentRotation = scalableBoneJobData.parentRotationArray.GetSubArray(i),
                        parentScale = scalableBoneJobData.parentScaleArray.GetSubArray(i),
                        originalLocalPosition = scalableBoneJobData.originalLocalPositionArray.GetSubArray(i),
                        positionOffset = scalableBoneJobData.positionOffsetArray.GetSubArray(i),
                        currentPosition = scalableBoneJobData.currentPositionArray.GetSubArray(i),
                        currentRotation = scalableBoneJobData.currentRotationArray.GetSubArray(i),
                        relativeRotation = scalableBoneJobData.relativeRotationArray.GetSubArray(i),
                        //***
                        hasParent = scalableBoneJobData.hasParentArray.GetSubArray(i),
                        isBeingDragged = scalableBoneJobData.isBeingDraggedArray.GetSubArray(i),
                        transformed = scalableBoneJobData.transformedArray.GetSubArray(i),
                        parentTypeOfChange = scalableBoneJobData.parentTypeOfChangeArray.GetSubArray(i),
                        parentTransformed = scalableBoneJobData.parentTransformedArray.GetSubArray(i),
                        typeOfChange = scalableBoneJobData.typeOfChangeArray.GetSubArray(i),
                        undoRecord = scalableBoneJobData.undoArray.GetSubArray(i),
                        prePositionOffset = scalableBoneJobData.prePositionOffsetArray.GetSubArray(i),
                        //***
                        lastPosition = scalableBoneJobData.lastPositionArray.GetSubArray(i),
                        lastRotation = scalableBoneJobData.lastRotationArray.GetSubArray(i),
                        lastScale = scalableBoneJobData.lastScaleArray.GetSubArray(i),
                        currentScale = scalableBoneJobData.currentScaleArray.GetSubArray(i),

                        bindPosition = scalableBoneJobData.bindPositionArray.GetSubArray(i),
                        bindRotation = scalableBoneJobData.BindRotationArray.GetSubArray(i),
                        enable = scalableBoneJobData.enableArray.GetSubArray(i)
                    };

                    scalableBoneUpdateJobs[i] = new ScalableBoneUpdateJob
                    {
                        parentPosition = scalableBoneJobData.parentPositionArray.GetSubArray(i),
                        parentRotation = scalableBoneJobData.parentRotationArray.GetSubArray(i),
                        parentScale = scalableBoneJobData.parentScaleArray.GetSubArray(i),
                        originalLocalPosition = scalableBoneJobData.originalLocalPositionArray.GetSubArray(i),
                        positionOffset = scalableBoneJobData.positionOffsetArray.GetSubArray(i),
                        currentPosition = scalableBoneJobData.currentPositionArray.GetSubArray(i),
                        currentRotation = scalableBoneJobData.currentRotationArray.GetSubArray(i),
                        relativeRotation = scalableBoneJobData.relativeRotationArray.GetSubArray(i),
                        //***
                        hasParent = scalableBoneJobData.hasParentArray.GetSubArray(i),
                        isBeingDragged = scalableBoneJobData.isBeingDraggedArray.GetSubArray(i),
                        transformed = scalableBoneJobData.transformedArray.GetSubArray(i),
                        parentTypeOfChange = scalableBoneJobData.parentTypeOfChangeArray.GetSubArray(i),
                        parentTransformed = scalableBoneJobData.parentTransformedArray.GetSubArray(i),
                        typeOfChange = scalableBoneJobData.typeOfChangeArray.GetSubArray(i),
                        prePositionOffset = scalableBoneJobData.prePositionOffsetArray.GetSubArray(i),
                        //***
                        lastPosition = scalableBoneJobData.lastPositionArray.GetSubArray(i),
                        lastRotation = scalableBoneJobData.lastRotationArray.GetSubArray(i),
                        lastScale = scalableBoneJobData.lastScaleArray.GetSubArray(i),
                        currentScale = scalableBoneJobData.currentScaleArray.GetSubArray(i),
                        bindPosition = scalableBoneJobData.bindPositionArray.GetSubArray(i),
                        bindRotation = scalableBoneJobData.BindRotationArray.GetSubArray(i),
                        enable = scalableBoneJobData.enableArray.GetSubArray(i)
                    };
                    scalableBoneUpdateSimpleJobs[i] = new ScalableBoneUpdateSimpleJob
                    {
                        parentPosition = scalableBoneJobData.parentPositionArray.GetSubArray(i),
                        parentRotation = scalableBoneJobData.parentRotationArray.GetSubArray(i),
                        parentScale = scalableBoneJobData.parentScaleArray.GetSubArray(i),
                        originalLocalPosition = scalableBoneJobData.originalLocalPositionArray.GetSubArray(i),
                        positionOffset = scalableBoneJobData.positionOffsetArray.GetSubArray(i),
                        currentPosition = scalableBoneJobData.currentPositionArray.GetSubArray(i),
                        currentRotation = scalableBoneJobData.currentRotationArray.GetSubArray(i),
                        currentScale = scalableBoneJobData.currentScaleArray.GetSubArray(i),
                        bindPosition = scalableBoneJobData.bindPositionArray.GetSubArray(i),
                        enable = scalableBoneJobData.enableArray.GetSubArray(i),
                        hasParent = scalableBoneJobData.hasParentArray.GetSubArray(i),

                    };

                }
            }


            writeTransformJob = new WriteTransformJob
            {
                enableFollowParent = scalableBoneJobData.enableFollowParentArray.GetTotalNativeArray(),
                currentPosition = scalableBoneJobData.currentPositionArray.GetTotalNativeArray(),
                currentRotation = scalableBoneJobData.currentRotationArray.GetTotalNativeArray(),
                bindPosition = scalableBoneJobData.bindPositionArray.GetTotalNativeArray(),
                bindRotation = scalableBoneJobData.BindRotationArray.GetTotalNativeArray(),
                enable = scalableBoneJobData.enableArray.GetTotalNativeArray(),
            };

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RegisterScalableBones(List<ScalableBone> scalableBoneLists, int depth)
        {
            if (scalableBoneLists != null)
            {
                //InitDepthBuffer
                for (; depth >= scalableBoneDepthLists.Count;)
                {
                    scalableBoneDepthLists.Add(new List<ScalableBone>());
                }
                var list = scalableBoneDepthLists[depth];
                if (list == null)
                {
                    list = new List<ScalableBone>();
                }
                //Add ScalableBone to buffer
                foreach (var scalableBone in scalableBoneLists)
                {
                    if (!scalableBone.IsRegistered)
                    {
                        list.Add(scalableBone);
                        dirty = true;
                        scalableBone.IsRegistered = true;
                    }
                }
            }

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnregisterScalableBone(ScalableBone scalableBone)
        {
            if (scalableBone.IsRegistered)
            {
                if (scalableBone.depth < scalableBoneDepthLists.Count)
                {
                    var list = scalableBoneDepthLists[scalableBone.depth];
                    if (list != null)
                    {
                        list.Remove(scalableBone);
                        scalableBone.IsRegistered = false;
                        dirty = true;
                    }
                }
            }
            else
            {
                //Debug.LogError($"cannot unregister the bone");
            }
        }

    }

    public struct ScalableBoneData
    {
        public Quaternion parentRotation;
        public Quaternion relativeRotation;
        public int index;
        public Transform transform;
    }

}
#endif