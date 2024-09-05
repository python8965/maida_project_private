#define BODY_PROPORTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;
namespace OnlyNew.BodyProportions
{
    [ExecuteAlways]
    [DefaultExecutionOrder(-15)]
    public class ScalableBonesManager : MonoBehaviour
    {

        [Tooltip("After clicking the \"Auto Setup\" button, all child bones of root will be configured automatically.")]
        public Transform root;
        [Tooltip("When it changes, the bindPosition of all the children ScalableBone of root changes.")]
        public bool bindAllPositions = true;
        [Tooltip("When it changes, the bindRotation of all the children ScalableBone of root changes.")]
        public bool bindAllRotations = true;


        private bool preBindAllPostions = true;

        private bool preBindAllRotations = true;
        [HideInInspector, SerializeField]
        public bool initialized = false;
#if UNITY_WEBGL
        [Tooltip("Multi-Threaded Mode is not supported for WebGL.")]
#else
        [Tooltip("Enabling multi-threaded mode can make the program faster when there are many characters in a scene.")]
#endif
        public bool multiThreadedMode = true;

        private bool preMultiThreadedMode = true;
        /// <summary>
        /// backup of original hierarchy
        /// </summary>
        [HideInInspector, SerializeField]
        public Transform rootCopy;
        public Action beforeUpdateBone, onUpdateBone, afterUpdateBone, updateAll;

        private List<List<ScalableBone>> scalableBoneDepthLists = new List<List<ScalableBone>>();
        private bool isDirty = false;
        private void Awake()
        {
            if (!initialized)
            {
                foreach (var child in GetComponentsInChildren<Transform>())
                {
                    if (child.gameObject.name.ToLower() == "root")
                    {
                        root = child;
                    }
                }
            }
#if UNITY_WEBGL
            multiThreadedMode = false;
#endif
            if (multiThreadedMode)
            {
#if !UNITY_WEBGL
                RefreshJobData();
#endif
            }

            preBindAllPostions = bindAllPositions;
            preBindAllRotations = bindAllRotations;
            preMultiThreadedMode = multiThreadedMode;
        }



        public void OnEnable()
        {
#if UNITY_WEBGL
            multiThreadedMode = false;
#endif
            if (!multiThreadedMode)
                RefreshBonesDelegate();
            else
            {
#if !UNITY_WEBGL
                SetScalableBoneActive(true);
#endif
            }
        }

        public void OnDisable()
        {
            if (!multiThreadedMode)
                ClearDelegates();

            else
            {
#if !UNITY_WEBGL
                SetScalableBoneActive(false);
#endif
            }
        }

        public void OnDestroy()
        {
            UnregisterChildrenBones();
            ClearDelegates();
        }
#if !UNITY_WEBGL && UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnReloadScipts()
        {
            var bones = FindObjectsByType<ScalableBone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var bone in bones)
            {
                bone.IsRegistered = false;
            }
            var managers = FindObjectsByType<ScalableBonesManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var manager in managers)
            {
                if (manager.multiThreadedMode)
                    manager.RefreshJobData();
            }
        }
#endif
        private void LateUpdate()
        {
            try
            {
#if !UNITY_WEBGL
                //int length = ScalableBoneJobManager.Instance.scalableBones.Count;
                //int managerCapcity = (updateAll == null) ? 0 : updateAll.GetInvocationList().Length;
                //Debug.Log($"Manager:{managerCapcity} ScalableBoneTask buffer:{length},{scalableBoneDepthLists.Count}");
#endif
                Profiler.BeginSample("ScalableBoneUpdate");
                if (multiThreadedMode != preMultiThreadedMode)
                {
                    if (preMultiThreadedMode)
                    {
#if !UNITY_WEBGL
                        UnregisterChildrenBones();
#endif
                    }
                    else
                    {
                        ClearDelegates();
                    }

                    if (multiThreadedMode)
                    {
#if !UNITY_WEBGL
                        RebuildLocalDepthLists();
                        RegisterChildrenBones();
#endif
                    }
                    else
                    {
                        RefreshBonesDelegate();
                    }
                    preMultiThreadedMode = multiThreadedMode;
                }

                if (multiThreadedMode)
                {
#if !UNITY_WEBGL
                    ScalableBoneJobManager.IsInitilized();
                    if (isDirty)
                    {
                        isDirty = false;
                        RefreshJobData();
                    }
#endif
                }
                else
                {
                    //beforeUpdateBone?.Invoke();
                    //onUpdateBone?.Invoke();
                    //afterUpdateBone?.Invoke();
                    updateAll?.Invoke();
                }
                Profiler.EndSample();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void RefreshJobData()
        {
            UnregisterChildrenBones();
            RebuildLocalDepthLists();
            RegisterChildrenBones();
            SetScalableBoneActive(enabled);
        }

        public void Ready()
        {
            if (multiThreadedMode)
            {
#if !UNITY_WEBGL
                RefreshJobData();
#endif
            }
            else
            {
                RefreshBonesDelegate();
            }
        }

        public void RefreshBonesDelegate()
        {
            ClearDelegates();
            if (root != null)
            {
                for (int i = 0; i < root.childCount; i++)
                {
                    var comp = root.GetChild(i).GetComponent<ScalableBone>();
                    if (comp != null)
                    {
                        comp.scalableBonesManager = this;
                        beforeUpdateBone += comp.BeforeUpdateBone;
                        onUpdateBone += comp.OnUpdateBone;
                        afterUpdateBone += comp.AfterUpdateBone;
                        updateAll += comp.UpdateAll;
                    }
                }
            }
        }
        public void SetScalableBoneActive(bool t_enabled)
        {
            if (root != null)
            {
                for (int i = 0; i < root.childCount; i++)
                {
                    var comp = root.GetChild(i).GetComponent<ScalableBone>();
                    if (comp != null)
                    {
                        comp.Enabled = t_enabled;
                    }
                }
            }
        }

        private void ClearDelegates()
        {
            beforeUpdateBone = null;
            onUpdateBone = null;
            afterUpdateBone = null;
            updateAll = null;
        }

        private void RegisterChildrenBones()
        {
#if !UNITY_WEBGL
            ScalableBoneJobManager scalableBoneJobManager = null;
            if (ScalableBoneJobManager.IsInitilized())
            {
                scalableBoneJobManager = ScalableBoneJobManager.Instance;
            }
            if (scalableBoneJobManager != null)
            {
                for (int i = 0; i < scalableBoneDepthLists.Count; i++)
                {
                    var list = scalableBoneDepthLists[i];
                    if (list != null)
                    {
                        scalableBoneJobManager.RegisterScalableBones(list, i);
                    }
                    else
                    {
                        Debug.LogError("the scalableBoneDepthList is not initialized correctly");
                    }
                }
            }
            else
            {
                Debug.LogError("the scalableBoneTask is not initialized correctly");
            }
#endif

        }

        private void RebuildLocalDepthLists()
        {
            scalableBoneDepthLists.Clear();
            if (root != null)
            {
                for (int i = 0; i < root.childCount; i++)
                {
                    var comp = root.GetChild(i).GetComponent<ScalableBone>();
                    if (comp != null)
                    {
                        comp.scalableBonesManager = this;
                        if (scalableBoneDepthLists != null)
                        {
                            var depth = comp.UpdateDepth();
                            InitScalableBoneSubList(depth);

                            FillScalableBoneSubList(depth, comp);

                        }
                    }
                }
            }

        }
        private void FillScalableBoneSubList(int depth, ScalableBone comp)
        {
            var list = scalableBoneDepthLists[depth];

            if (list == null)
            {
                list = new List<ScalableBone>();
            }
            if (!list.Contains(comp))
            {
                list.Add(comp);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitScalableBoneSubList(int depth)
        {
            for (; depth >= scalableBoneDepthLists.Count;)
            {
                scalableBoneDepthLists.Add(new List<ScalableBone>());
            }
        }


        private void UnregisterChildrenBones()
        {
#if !UNITY_WEBGL
            if (root != null && ScalableBoneJobManager.IsExist)
            {
                var task = ScalableBoneJobManager.Instance;
                if (task != null)
                {
                    for (int i = 0; i < scalableBoneDepthLists.Count; i++)
                    {
                        foreach (var bone in scalableBoneDepthLists[i])
                        {

                            task.UnregisterScalableBone(bone);
                        }

                    }
                }

            }
#endif
        }
        private void OnValidate()
        {
            if (bindAllPositions != preBindAllPostions)
            {
                var children = GetComponentsInChildren<ScalableBone>();
                foreach (var child in children)
                {
                    child.bindPosition = bindAllPositions;
                }
                preBindAllPostions = bindAllPositions;
            }
            if (bindAllRotations != preBindAllRotations)
            {
                var children = GetComponentsInChildren<ScalableBone>();
                foreach (var child in children)
                {
                    child.bindRotation = bindAllRotations;
                }
                preBindAllRotations = bindAllRotations;
            }
        }

        public void OnDestroyScalableBone(ScalableBone scalableBone)
        {
            if (multiThreadedMode)
            {
#if !UNITY_WEBGL
                if (ScalableBoneJobManager.IsExist && Application.IsPlaying(scalableBone))
                {
                    ScalableBoneJobManager.Instance.UnregisterScalableBone(scalableBone);
                    //UnregisterChildrenBones();
                    //RebuildLocalDepthLists();
                    //RegisterChildrenBones();
                    isDirty = true;
                }
#endif
            }
            else
            {
                beforeUpdateBone -= scalableBone.BeforeUpdateBone;
                onUpdateBone -= scalableBone.OnUpdateBone;
                afterUpdateBone -= scalableBone.AfterUpdateBone;
                updateAll -= scalableBone.UpdateAll;
            }
        }
    }
}
