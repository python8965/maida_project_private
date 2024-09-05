using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OnlyNew.BodyProportions
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScalableBonesManager))]
    public class ScalableBonesManagerEditor : Editor
    {
        readonly GUIContent setupButtonContent = new GUIContent("Auto Setup", "The button will unpack prefab gameObject and reorganize hierarchy of bones , and sets the parent of each child-bone as the root, then adds ScalableBone component for each child.");
        readonly GUIContent recoverButtonContent = new GUIContent("Recover", "The button will recover hierarchy of bones, and remove ScalableBone components from child-bones.");
        readonly GUIContent ikButtonContent = new GUIContent("Create Virtual Skeleton", "Virtual skeleton is a transform transmitter that provides a familiar environment for standard skeletal animation scripts such as IK.");
        readonly GUIContent helpButtonContent = new GUIContent("?", "visit web documentation");
        const string neverAskAgain = "ScalableBonesManager.NeverAskAgain";
        private SerializedProperty rootProperty, bindAllPositionsProperty, bindAllRotationsProperty, multiThreadedModeProperty;
        private GUIContent rootContent, bindAllPositionsContent, bindAllRotationsContent, multiThreadedModeContent;
        private string helpMessage = "Please backup the gameObject before installing ScalableBoneManager, because ScalableBoneManager will reorganize hierarchy of the skeleton. Your work may be broken!";
        //private MessageType messageType = MessageType.Warning;
        private const string warningMessage = "Please backup the gameObject before installing ScalableBoneManager, because ScalableBoneManager will reorganize hierarchy of the skeleton. Your work may be broken!";
        List<Transform> scalableBones = new List<Transform>();
        private bool enableHelpbox = false;
        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            rootProperty = serializedObject.FindProperty("root");
            bindAllPositionsProperty = serializedObject.FindProperty("bindAllPositions");
            bindAllRotationsProperty = serializedObject.FindProperty("bindAllRotations");
            multiThreadedModeProperty = serializedObject.FindProperty("multiThreadedMode");
            rootContent = new GUIContent("Root");
            bindAllPositionsContent = new GUIContent("Bind All Positions");
            bindAllRotationsContent = new GUIContent("Bind All Rotations");
            multiThreadedModeContent = new GUIContent("Multi Threaded Mode");
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        public override void OnInspectorGUI()
        {
            ScalableBonesManager t = target as ScalableBonesManager;
            EditorGUILayout.PropertyField(rootProperty, rootContent);
            EditorGUILayout.PropertyField(bindAllPositionsProperty, bindAllPositionsContent);
            EditorGUILayout.PropertyField(bindAllRotationsProperty, bindAllPositionsContent);
#if UNITY_WEBGL
            GUI.enabled = false;
            EditorGUILayout.PropertyField(multiThreadedModeProperty, multiThreadedModeContent);
            GUI.enabled = true;
#else
            EditorGUILayout.PropertyField(multiThreadedModeProperty, multiThreadedModeContent);
#endif
            if (t.initialized == false)
            {
                EditorGUILayout.HelpBox(helpMessage, MessageType.Warning);
            }

            if (!t.initialized)
            {
                if (GUILayout.Button(setupButtonContent, GUILayout.Height(26f)))
                {
                    if (t != null)
                    {
                        if (t.root != null)
                        {
                            if (t.root.parent != null)
                            {
                                if (EditorUtility.DisplayDialog("Please Backup",
                                        "Please backup the gameObject before installing ScalableBoneManager, because ScalableBoneManager will reorganize hierarchy of the Skeleton. Your work may be broken!",
                                        "I've backed up.",
                                        "Cancel",
                                        DialogOptOutDecisionType.ForThisSession,
                                        neverAskAgain
                                    ))
                                {
                                    if (CheckNames(t.root))
                                    {
                                        Undo.SetCurrentGroupName("Auto Setup");
                                        Undo.RegisterCompleteObjectUndo(t, "Setup");
                                        if (PrefabUtility.IsPartOfAnyPrefab(t.gameObject))
                                        {

                                            try
                                            {
                                                PrefabUtility.UnpackPrefabInstance(t.gameObject,
                                                    PrefabUnpackMode.Completely, InteractionMode.UserAction);
                                            }
                                            catch (System.Exception e)
                                            {
                                                Debug.LogException(e);
                                                EditorUtility.DisplayDialog("Failed", "Unpacked prefab failed. Please unpack the parent GameObject manually, and try again", "OK");
                                                return;
                                            }

                                        }

                                        //copy root
                                        var root_copy = AddBrotherObject(t.root, t.root.parent);
                                        t.rootCopy = root_copy;
                                        CopyHierarchy(t.root, root_copy);
                                        root_copy.gameObject.SetActive(false);
                                        root_copy.hideFlags = HideFlags.HideInHierarchy;
                                        //important
                                        scalableBones.Clear();
                                        AddComponnetWhileHaveChild(t.root);
                                        SetParentOfChildren(t.root, scalableBones);
                                        t.Ready();
                                        t.initialized = true;

                                        Undo.RegisterCompleteObjectUndo(t, "Setup");
                                        helpMessage = warningMessage;
                                    }
                                }
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("Failed", "The root must be the child of ScalableBonesManager!", "OK");
                            }
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Failed", "Please set your root bone!", "OK");
                        }

                    }
                }
            }
            else
            {
                if (GUILayout.Button(recoverButtonContent, GUILayout.Height(26f)))
                {
                    if (t != null)
                    {
                        if (t.initialized && t.root != null)
                        {
                            if (t.root.parent != null)
                            {
                                var rootCopyName = "_" + t.root.gameObject.name;
                                Transform rootcopy = null;
                                if (t.rootCopy != null)
                                {
                                    rootcopy = t.rootCopy;
                                }
                                if (rootcopy == null)
                                {
                                    for (int i = 0; i < t.root.parent.childCount; i++)
                                    {
                                        if (t.root.parent.GetChild(i).name == rootCopyName)
                                        {
                                            rootcopy = t.root.parent.GetChild(i);
                                        }
                                    }
                                }
                                if (rootcopy == null)
                                {
                                    foreach (var child in t.GetComponentsInChildren<Transform>(true))
                                    {
                                        if (child.gameObject.name == rootCopyName)
                                        {
                                            rootcopy = child;
                                        }
                                    }
                                }
                                if (rootcopy != null)
                                {
                                    Undo.SetCurrentGroupName("Recover");
                                    Undo.RegisterCompleteObjectUndo(t, "Recover initialized");
                                    t.OnDisable();
                                    var children = t.root.GetComponentsInChildren<Transform>(true);
                                    Dictionary<string, Transform> rootChildrenDict = new Dictionary<string, Transform>();
                                    foreach (var child in children)
                                    {
                                        rootChildrenDict.Add(child.gameObject.name, child);
                                    }
                                    RecoverHierarchy(rootcopy, rootChildrenDict);
                                    Undo.DestroyObjectImmediate(rootcopy.gameObject);
                                    t.Ready();
                                    t.initialized = false;

                                    Undo.RegisterCompleteObjectUndo(t, "Recover initialized");
                                }
                            }
                        }
                    }
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(ikButtonContent))
                {
                    if (t != null)
                    {
                        if (t.initialized && t.root != null)
                        {
                            if (t.root.parent != null)
                            {
                                CreateCoupledSkeleton(t.root);
                            }
                        }
                    }
                }

                if (GUILayout.Button(helpButtonContent, GUILayout.Height(18f), GUILayout.Width(18f)))
                {
                    Application.OpenURL("https://www.onlynew.tech/BodyProportionsIKConfiguration");
                }

                EditorGUILayout.EndHorizontal();
            }
            serializedObject.ApplyModifiedProperties();
        }


        private static void RecoverHierarchy(Transform rootcopy, Dictionary<string, Transform> rootChildrenDict)
        {
            for (int i = 0; i < rootcopy.childCount; i++)
            {
                Transform childTransform = rootcopy.GetChild(i);
                if (childTransform.name.StartsWith("_"))
                {
                    var originalName = childTransform.name.Substring(1);
                    var originalParentName = childTransform.parent?.name.Substring(1);
                    if (originalName != "" && originalParentName != "")
                    {
                        Transform rootChildren = null;
                        Transform itsParent = null;
                        if (rootChildrenDict.TryGetValue(originalName, out rootChildren) && rootChildrenDict.TryGetValue(originalParentName, out itsParent))
                        {
                            Undo.SetTransformParent(rootChildren, itsParent, "recover hierarchy");
                            Component bindBone = null;
                            if (rootChildren.TryGetComponent(typeof(ScalableBone), out bindBone))
                            {
                                Undo.DestroyObjectImmediate(bindBone);
                            }
                            rootChildren.position = childTransform.position;
                            rootChildren.rotation = childTransform.rotation;
                            rootChildren.localScale = childTransform.localScale;
                        }
                    }
                }
                RecoverHierarchy(childTransform, rootChildrenDict);
            }
        }

        void OnUndoRedoPerformed()
        {
            ScalableBonesManager t = target as ScalableBonesManager;
            if (t != null)
            {
                t.Ready();
            }
        }


        static Transform AddBrotherObject(Transform me, Transform parent)
        {
            //copy hierarchy
            GameObject objectCopy = new GameObject("_" + me.gameObject.name);
            Undo.RegisterCreatedObjectUndo(objectCopy, "created object");
            Undo.SetTransformParent(objectCopy.transform, parent, "change parent");
            Undo.RecordObject(objectCopy.transform, "record transform");
            objectCopy.transform.position = me.transform.position;
            objectCopy.transform.rotation = me.transform.rotation;
            objectCopy.transform.localScale = me.transform.localScale;
            return objectCopy.transform;
        }

        void AddComponnetWhileHaveChild(Transform me)
        {
            for (int i = 0; i < me.childCount; i++) //if I have child .foreach
            {
                var child = me.GetChild(i);

                //add component
                Undo.AddComponent<ScalableBone>(child.gameObject);
                scalableBones.Add(child);
                //config component
                ScalableBone childComponent = child.gameObject.GetComponent<ScalableBone>();

                //tell my child who is parent
                childComponent.SetParent(me);
            }
            for (int i = 0; i < me.childCount; i++) //if I have child .foreach
            {

                var child = me.GetChild(i);
                if (child.GetComponent<ProtectChildrenFromScalableBonesManager>() != null)
                {
                    Debug.Log(child.name + "'s children have been protected from ScalableBonesManager");
                    continue;
                }
                //my child inherits my career
                AddComponnetWhileHaveChild(child);
            }
        }
        static void CopyHierarchy(Transform me, Transform copy)
        {
            //copy hierarchy
            for (int i = 0; i < me.childCount; i++) //if I have child .foreach
            {
                var child = me.GetChild(i);
                var childCopy = AddBrotherObject(child, copy);

                if (child.GetComponent<ProtectChildrenFromScalableBonesManager>() == null)
                {    //My children do same thing.
                    CopyHierarchy(child, childCopy);
                }
            }
        }
        void SetParentOfChildren(Transform parent, List<Transform> scalableBones)
        {
            for (int i = 1; i < scalableBones.Count; i++)
            {
                if (scalableBones[i].parent != parent)
                {
                    //transforms[i].parent = root;
                    Undo.SetTransformParent(scalableBones[i], parent, "Set parent");
                }
            }
        }
        private bool CheckNames(Transform obj)
        {
            bool pass = true;
            HashSet<string> names = new HashSet<string>();
            var objs = obj.GetComponentsInChildren<Transform>(true);
            //check name duplication
            for (int i = 0; i < objs.Length; i++)
            {
                if (names.Contains(objs[i].gameObject.name))
                {
                    pass = false;
                    helpMessage = "Objects with duplicate names are not be supported. Please rename the object \"" + objs[i].gameObject.name + "\" and try again.";
                    Debug.LogError(helpMessage);
                    EditorGUIUtility.PingObject(objs[i].gameObject);
                }
                else
                {
                    names.Add(objs[i].gameObject.name);
                }
            }
            return pass;
        }

        private static void CreateCoupledSkeleton(Transform root)
        {
            var name = "_Virtual_" + root.name;
            for (int i = 0; i < root.parent.childCount; i++)
            {
                if (name == root.parent.GetChild(i).gameObject.name)
                {
                    EditorUtility.DisplayDialog("Failed", $"Virtual skeleton already exists. If you want to recreate it, delete '{name}' and try again.", "OK");
                    EditorGUIUtility.PingObject(root.parent.GetChild(i).gameObject);
                    return;
                }
            }

            var root_copy = new GameObject(name);
            root_copy.transform.parent = root.parent;
            root_copy.transform.position = root.position;
            root_copy.transform.rotation = root.rotation;
            var reader = root_copy.AddComponent<ReadTransform>();
            var writer = root_copy.AddComponent<WriteTransform>();
            var list = new List<TransformPair>();
            list.Add(new TransformPair(root, root_copy.transform));
            CopyChildObject(root.gameObject, root_copy, list);
            reader.bonePair = list.ToArray();
            writer.bonePair = reader.bonePair;
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorGUIUtility.PingObject(root_copy);
        }
        private static void CopyChildObject(GameObject obj, GameObject parent, List<TransformPair> list)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                var child = obj.transform.GetChild(i);
                var scalableBone = child.GetComponent<ScalableBone>();
                if (scalableBone != null && scalableBone.parentScalableBone == null)
                {
                    GameObject childCopy = new GameObject("_Virtual_" + child.gameObject.name);
                    childCopy.transform.parent = parent.transform;
                    childCopy.transform.position = child.position;
                    childCopy.transform.rotation = child.rotation;
                    list.Add(new TransformPair(child, childCopy.transform));
                    CopyChildScalableBone(child.gameObject, childCopy, list);
                }
            }
        }
        private static void CopyChildScalableBone(GameObject obj, GameObject parent, List<TransformPair> list)
        {
            var scalableBone = obj.GetComponent<ScalableBone>();
            if (scalableBone != null)
            {
                for (int i = 0; i < scalableBone.childrenScalableBone.Count; i++)
                {
                    var childScalableBone = scalableBone.childrenScalableBone[i];
                    if (childScalableBone != null)
                    {
                        GameObject childCopy = new GameObject("_Virtual_" + childScalableBone.gameObject.name);
                        childCopy.transform.parent = parent.transform;
                        childCopy.transform.position = childScalableBone.transform.position;
                        childCopy.transform.rotation = childScalableBone.transform.rotation;
                        list.Add(new TransformPair(childScalableBone.transform, childCopy.transform));
                        CopyChildScalableBone(childScalableBone.gameObject, childCopy, list);
                    }
                }
            }
        }

    }
}