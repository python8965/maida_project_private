using System.Collections.Generic;
using UnityEngine;
namespace OnlyNew.BodyProportions
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class ScalableBoneRenderer : MonoBehaviour
    {

#if UNITY_EDITOR
        public bool visible = true;



        public BoneType boneType = BoneType.ScalableBone;


        public Color surfaceColor = new Color(0.9716981f, 0.2008339f, 0.1069478f, 0.5450981f);
        public Color wireColor = new Color(0f, 0.5199656f, 0.856f, 1f);

        [SerializeField, HideInInspector]
        private ScalableBone[] scalableBones;

        public enum DirectionType
        {
            ConectStartToEnd,
            AccurateDirection
        }
        public enum BoneType
        {
            Line,
            ScalableBone
        };
        public delegate void OnAddBoneRendererCallback(ScalableBoneRenderer boneRenderer);
        public delegate void OnRemoveBoneRendererCallback(ScalableBoneRenderer boneRenderer);
        public static OnAddBoneRendererCallback onAddBoneRenderer;
        public static OnRemoveBoneRendererCallback onRemoveBoneRenderer;

        private void Awake()
        {
            scalableBones = GetComponentsInChildren<ScalableBone>();
            RebuildBonesStruct();
        }
        private void OnEnable()
        {
            onAddBoneRenderer(this);
        }
        private void OnDisable()
        {
            onRemoveBoneRenderer(this);
        }




        public struct BoneData
        {
            public float length;
            public Vector3 currentLocalEnd;
            public Vector3 childOriginalLocalPosition;

            public Transform start;
            public ScalableBone scalableBone;
        };
        private BoneData[] bonesData;
        private Transform[] tips;

        public BoneData[] BonesData
        {
            get => bonesData;
        }

        public Transform[] Tips
        {
            get => tips;
        }
        public void ClearBones()
        {
            tips = null;
            bonesData = null;
        }
        public void ReloadScalableBone()
        {
            scalableBones = GetComponentsInChildren<ScalableBone>();
        }
        public void RebuildBonesStruct()
        {
            if (scalableBones == null || scalableBones.Length == 0)
            {
                ClearBones();
                return;
            }
            //prepare boneDatas

            Dictionary<ScalableBone, List<ScalableBone>> bonesFamily = new Dictionary<ScalableBone, List<ScalableBone>>();
            foreach (var scalableBone in scalableBones)
            {
                if (scalableBone == null)
                    continue;
                var mask = UnityEditor.Tools.visibleLayers;
                if ((mask & (1 << transform.gameObject.layer)) == 0)
                    continue;
                var key = scalableBone.parentScalableBone;
                if (key != null)
                {
                    //if (scalableBone.parentScalableBone.visible == false)
                    //    continue;
                    if (UnityEditor.SceneVisibilityManager.instance.IsHidden(key.gameObject, false))
                        continue;
                    List<ScalableBone> children;
                    if (bonesFamily.TryGetValue(key, out children))
                    {
                        if (children == null)
                        {
                            children = new List<ScalableBone>();
                        }
                        children.Add(scalableBone);

                    }
                    else
                    {
                        children = new List<ScalableBone>
                        {
                            scalableBone
                        };
                        bonesFamily.Add(key, children);
                    }
                }
            }
            List<BoneData> bones = new List<BoneData>();
            List<Transform> tipList = new List<Transform>();
            foreach (var scalableBone in scalableBones)
            {
                if (scalableBone != null)
                {
                    if (bonesFamily.ContainsKey(scalableBone))
                    {
                        foreach (var child in bonesFamily[scalableBone])
                        {
                            BoneData b = new BoneData();
                            b.start = scalableBone.transform;
                            var localPostion = scalableBone.transform.InverseTransformPoint(child.transform.position);
                            b.childOriginalLocalPosition = child.OriginalLocalPosition;
                            b.currentLocalEnd = localPostion;
                            b.scalableBone = scalableBone;
                            bones.Add(b);
                        }
                    }
                    else    //If a bone is the last bone.
                    {
                        var tip = scalableBone.transform;
                        tipList.Add(tip);
                    }
                }

            }
            bonesData = bones.ToArray();
            tips = tipList.ToArray();
        }

#endif
    }
}
