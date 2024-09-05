using UnityEditor;
using UnityEngine;

namespace OnlyNew.BodyProportions
{
    [InitializeOnLoad]
    public class MouseManipulate
    {
        static bool isInitialized = false;
        static MouseManipulate()
        {
            if (!isInitialized)
            {
                SceneView.duringSceneGui += UpdateView;
                isInitialized = true;
            }
        }
        ~MouseManipulate()
        {
            if (isInitialized)
            {
                SceneView.duringSceneGui -= UpdateView;
                isInitialized = false;
            }
        }
        public static void Initialize()
        {
            if (!isInitialized)
            {
                SceneView.duringSceneGui += UpdateView;
                isInitialized = true;
            }
        }
        static GameObject selectedGameobject;
        static ScalableBone selectedBoneComponent;
        static bool inControl = false;
        static void UpdateView(SceneView sceneView)
        {
            Event e = Event.current;
            //Debug.Log(GUIUtility.hotControl + "," + e.type.ToString() + "," + Tools.current.ToString());
            switch (e.type)
            {
                case EventType.Used:
                    if (e.button == 0)
                    {
                        OnDragBone();
                    }
                    break;
                case EventType.MouseDrag:
                    if (Tools.current == Tool.Rotate || Tools.current == Tool.Transform)
                    {
                        OnDragBone();
                    }
                    else if (GUIUtility.hotControl != 0)
                    {
                        OnDragBone();
                    }
                    break;
                default: break;
            }


        }

        private static void OnDragBone()
        {
            if (Selection.activeGameObject == null)
                return;

            if (Selection.activeGameObject != selectedGameobject)
            {
                selectedGameobject = Selection.activeGameObject;
                selectedBoneComponent = selectedGameobject.GetComponent<ScalableBone>();
            }

            if (selectedBoneComponent != null)
            {
                selectedBoneComponent.IsBeingDragged = true;
                inControl = true;
                //Debug.Log(selectedGameobject.name + " is selected");
            }

        }

        private static void OnMouseUp()
        {
            if (GUIUtility.hotControl == 0)
            {
                if (inControl)
                {
                    selectedBoneComponent = selectedGameobject.GetComponent<ScalableBone>();
                    if (selectedBoneComponent != null)
                    {
                        var parent = selectedBoneComponent.transform.parent;
                        var parentBone = selectedBoneComponent.parentScalableBone;
                        if (parent != null)
                        {
                            ForEachChildren(selectedBoneComponent.transform, parent);
                        }
                        //Debug.Log(selectedGameobject.name + " mouse up");
                    }

                    inControl = false;
                }
            }
        }

        private static void ForEachChildren(Transform me, Transform parent)
        {
            for (int i = me.GetSiblingIndex() + 1; i < parent.childCount; i++)
            {
                var childComponent = parent.GetChild(i).GetComponent<ScalableBone>();
                if (childComponent != null)
                {
                    if (childComponent.parentScalableBone != null)
                    {
                        //if (childComponent.parentScalableBone.transform== me)
                        //{
                        //    ForEachChildren(childComponent.transform, me);
                        //}

                        //Undo.RecordObject(childComponent.transform, "exit drag");
                        //var pos = childComponent.transform.position;
                        //childComponent.transform.position = childComponent.transform.position + Vector3.one;
                        //childComponent.transform.position = pos;
                        //var quaternion = childComponent.transform.rotation;
                        //childComponent.transform.rotation = Quaternion.identity;
                        //childComponent.transform.rotation = quaternion;


                    }
                }
            }
        }
    }
}