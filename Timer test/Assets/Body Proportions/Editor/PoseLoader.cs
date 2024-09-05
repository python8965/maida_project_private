using OnlyNew.BodyProportions.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace OnlyNew.BodyProportions
{
    public class PoseLoader : EditorWindow
    {
        public Animator model;
        public BP_PosePreset posePreset;
        const string presetName = "";
        readonly GUIContent modelLabel = new GUIContent("Animator of model", "the model which you want to save and load its pose");
        readonly GUIContent posePresetLabel = new GUIContent("Pose Preset", "Pose preset stores transform of bones.");
        readonly GUIContent saveButtonContent = new GUIContent("Save Preset", "Save Pose to Asset");
        readonly GUIContent loadButtonContent = new GUIContent("Load Preset", "Load Pose to Asset");
        readonly GUIContent newButtonContent = new GUIContent("New", "Create pose preset and store it on drive");
        readonly GUIContent loadPositionButtonContent = new GUIContent("Load Position", "Load Pose to Asset");
        readonly GUIContent loadRotationButtonContent = new GUIContent("Load Rotation", "Load Pose to Asset");
        readonly GUIContent loadScaleButtonContent = new GUIContent("Load Scale", "Load Pose to Asset");
        const string helpInfo = "Tips:\n1.Pose Loader can help migrate poses between the same models with any hierarchy.\n2.You can use Pose Loader to create animation frames. ";
        List<Transform> transforms = new List<Transform>();
        [MenuItem("Tools/Body Proportions/Pose Loader", false, 3)]
        static void Init()
        {
            PoseLoader window = GetWindow<PoseLoader>(false, "Pose Loader");
            window.Show();
        }
        private void OnEnable()
        {
        }
        private void OnGUI()
        {
            EditorGUILayout.Space(5f);
            model = EditorGUILayout.ObjectField(modelLabel, model, typeof(Animator), true) as Animator;
            EditorGUILayout.BeginHorizontal();
            posePreset = EditorGUILayout.ObjectField(posePresetLabel, posePreset, typeof(BP_PosePreset), true) as BP_PosePreset;
            GameObject t = null;
            if (model)
            {
                t = model.gameObject;
            }


            if (GUILayout.Button(newButtonContent, GUILayout.Width(50f)))
            {
                if (CheckModel())
                {
                    try
                    {
                        string dirPath = "Assets/Body Proportions/Poses/";
                        string filename = t.gameObject.name + " pose";
                        BP_PosePreset asset = ScriptableObject.CreateInstance<BP_PosePreset>();

                        AssetCreator.CreateAssetWithCheck<BP_PosePreset>(dirPath, filename, asset);
                        if (asset != null)
                        {
                            posePreset = asset;
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }

            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space(20f);

            if (GUILayout.Button(saveButtonContent, GUILayout.Height(26f)))
            {
                if (CheckModel() && CheckPreset())
                {
                    t.GetComponentsInChildren<Transform>(true, transforms);
                    SaveAsset(t);
                }
            }
            var height = GUILayout.Height(18f);
            if (GUILayout.Button(loadButtonContent, GUILayout.Height(26f)))
            {
                if (CheckModel() && CheckPreset())
                {
                    t.GetComponentsInChildren<Transform>(true, transforms);
                    Undo.RecordObjects(transforms.ToArray(), t.gameObject.name);
                    LoadAsset(t, true, true, true);
                }
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(loadPositionButtonContent, height))
            {
                StopScalableBoneUpdateWhenLoad(t, true, false, false);
            }
            if (GUILayout.Button(loadRotationButtonContent, height))
            {
                StopScalableBoneUpdateWhenLoad(t, false, true, false);
            }
            if (GUILayout.Button(loadScaleButtonContent, height))
            {
                StopScalableBoneUpdateWhenLoad(t, false, false, true);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20f);
            EditorGUILayout.HelpBox(helpInfo, MessageType.Info);
        }

        private void StopScalableBoneUpdateWhenLoad(GameObject t, bool loadPos, bool loadRot, bool loadScale)
        {
            if (CheckModel() && CheckPreset())
            {
                t.GetComponentsInChildren<Transform>(true, transforms);
                Undo.RecordObjects(transforms.ToArray(), t.gameObject.name);
                ScalableBonesManager temp = null;
                var managers1 = t.GetComponentsInChildren<ScalableBonesManager>();
                List<ScalableBonesManager> existManagers = new List<ScalableBonesManager>();
                foreach (var manager in managers1)
                {
                    manager.StartCoroutine(EnableLater(manager));
                    temp = manager;
                    existManagers.Add(manager);
                }

                var managers2 = t.GetComponentsInParent<ScalableBonesManager>();

                foreach (var manager in managers2)
                {
                    if (!existManagers.Contains(manager))
                    {
                        manager.StartCoroutine(EnableLater(manager));
                        temp = manager;
                    }
                }

                if (temp != null)
                {
                    temp.StartCoroutine(LoadLater(t, loadPos, loadRot, loadScale));
                }
                else
                {
                    LoadAsset(t, loadPos, loadRot, loadScale);
                }
            }
        }

        bool Selected()
        {
            bool selected = false;
            var obj = Selection.activeGameObject;
            if (obj != null)
            {
                if (obj.GetComponent<Animator>() || obj.GetComponent<Animation>())
                {
                    selected = true;
                }
                else
                {
                    GUILayout.Label("Please select an Animator");
                }
            }
            else
            {
                GUILayout.Label("Please select an Animator");
            }

            return selected;
        }
        void SaveAsset(GameObject t)
        {
            SetterToAsset(t);
        }
        public void SetterToAsset(GameObject t)
        {
            if (t != null && posePreset)
            {
                posePreset.Clear();

                for (int i = 1; i < transforms.Count; i++)
                {
                    if (transforms[i] != null)
                    {
                        var bt = new BoneTransformData()
                        {

                            relativeLocalPosition = t.transform.InverseTransformPoint(transforms[i].position),
                            relativeLocalRotation = Quaternion.Inverse(t.transform.rotation) * transforms[i].rotation,
                            localScale = transforms[i].localScale
                        };
                        posePreset.TryAdd(transforms[i].gameObject.name, bt);
                    }

                }
                UnityEditor.EditorUtility.SetDirty(posePreset);
            }
        }
        void LoadAsset(GameObject t, bool loadPos, bool loadRot, bool loadScale)
        {
            if (posePreset != null && t != null)
            {
                BoneTransformData trans;

                //not override myself
                for (int i = 1; i < transforms.Count; i++)
                {
                    if (posePreset.TryGetValue(transforms[i].gameObject.name, out trans))
                    {
                        if (loadPos)
                            transforms[i].position = t.transform.TransformPoint(trans.relativeLocalPosition);
                        if (loadRot)
                            transforms[i].rotation = t.transform.rotation * trans.relativeLocalRotation;
                        if (loadScale)
                            transforms[i].localScale = trans.localScale;
                    }
                }
            }
        }

        public IEnumerator EnableLater(ScalableBonesManager manager)
        {
            var t_enabled = manager.enabled;
            manager.enabled = false;
            yield return null;
            yield return null;
            manager.enabled = t_enabled;
        }

        public IEnumerator LoadLater(GameObject t, bool loadPos, bool loadRot, bool loadScale)
        {
            yield return null;
            LoadAsset(t, loadPos, loadRot, loadScale);
            yield return null;
        }

        bool CheckModel()
        {
            if (model == null)
            {
                //Highlighter.Highlight("Pose Loader", "Animator of model");
                Debug.LogError("Please specify the animator of model");
                return false;
            }
            else
            {
                return true;
            }
        }

        bool CheckPreset()
        {
            if (posePreset == null)
            {
                //Highlighter.Highlight("Pose Loader", "Pose Preset");
                Debug.LogError("Please specify the pose preset");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}