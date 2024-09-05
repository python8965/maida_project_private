using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OnlyNew.BodyProportions
{
    public class BatchAnimationConverter : EditorWindow
    {
        [Tooltip("The hierarchy of skeleton before setup of ScalableBonesManager")]
        public Animator convertFrom;
        [Tooltip("Root bone of the model which you want to convert its animation")]
        public GameObject rootBoneOfConvertFrom;

        [Tooltip("The hierarchy of skeleton after setup of ScalableBonesManager")]
        public Animator convertTo;

        [Tooltip("The animation clip which you want to convert.")]
        public AnimationClip clip;

        public bool isCreatedByUnity = false;
        public int frameRate = 60;
        public bool noScale = false;
        [Tooltip("The path of new clip")]
        public string path = "Body Proportions/Output";

        readonly GUIContent convertFromLabel = new GUIContent("Convert From", "The animator's gameObject has hierarchy of skeleton before setup of ScalableBonesManager");
        readonly GUIContent rootBoneLabel = new GUIContent("RootBone of ConvertFrom", "Root bone of the model which you want to convert its animation");
        readonly GUIContent convertToLabel = new GUIContent("Convert To", "The animator's gameObject has hierarchy of skeleton after setup of ScalableBonesManager");
        readonly GUIContent clipLabel = new GUIContent("Clip", "The animation clip which you want to convert.");
        readonly GUIContent CreatedByUnityLabel = new GUIContent("Is created by Unity", "Was the animation waiting to be converted created by Unity?");
        readonly GUIContent frameRateLabel = new GUIContent("Output FrameRate", "The framerate of new animation clip. It is recommended to use a higher frame rate to get a smooth and precise animation.\"");
        readonly GUIContent pathLabel = new GUIContent("Path", "The path of new clip.");
        readonly GUIContent discardScaleLabel = new GUIContent("Discard Scale", "If checked, Discard scale animation.");

        //const string rotationProperty = "localEulerAnglesBaked";
        [MenuItem("Tools/Body Proportions/Batch Animation Converter", false, 2)]
        static void Init()
        {
            BatchAnimationConverter window = GetWindow<BatchAnimationConverter>(false, "Batch Animattion Converter");
            window.Show();
        }

        private void OnGUI()
        {
            convertFrom = EditorGUILayout.ObjectField(convertFromLabel, convertFrom, typeof(Animator), true) as Animator;
            rootBoneOfConvertFrom = EditorGUILayout.ObjectField(rootBoneLabel, rootBoneOfConvertFrom, typeof(GameObject), true) as GameObject;
            convertTo = EditorGUILayout.ObjectField(convertToLabel, convertTo, typeof(Animator), true) as Animator;
            isCreatedByUnity = EditorGUILayout.Toggle(CreatedByUnityLabel, isCreatedByUnity);
            frameRate = EditorGUILayout.IntField(frameRateLabel, frameRate);
            noScale = EditorGUILayout.Toggle(discardScaleLabel, noScale);
            path = EditorGUILayout.TextField(pathLabel, path);

            //EditorGUILayout.LabelField("eg. Body Proportions/Output");
            if (GUILayout.Button("Convert", GUILayout.Height(26f)))
            {
                ConvertAnimatorController(convertFrom, rootBoneOfConvertFrom, convertTo, clip, isCreatedByUnity, frameRate, noScale, path);
            }
            GUILayout.Space(20f);
            EditorGUILayout.HelpBox("Batch conversion consumes longer time. Please try Animation Converter first to make sure your animation format is supported, as well as to make sure your configuration is correct.", MessageType.Info);

        }

        public void ConvertAnimatorController(Animator convertFrom, GameObject rootBoneOfConvertFrom, Animator convertTo, AnimationClip clip, bool theAnimationIsCreatedByUnity, int frameRate, bool noScale, string path)

        {
            if (convertFrom == null || convertTo == null || rootBoneOfConvertFrom == null || path == "")
                return;
            if (convertFrom.runtimeAnimatorController == null)
            {
                Debug.LogError("animation controller can't be null.");
                return;
            }
            var name = convertFrom.runtimeAnimatorController.name;
            string controllerFolder = path + "/" + name;
            if (AnimationConverter.CheckAndCreatePath(controllerFolder))
            {
                string controllerPath = "Assets/" + controllerFolder + "/" + name + "_converted" + ".overrideController";

                AnimatorOverrideController overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(controllerPath);

                if (overrideController == null)
                {
                    overrideController = new AnimatorOverrideController(convertFrom.runtimeAnimatorController);




                    string temp_path = controllerPath.Replace("|", "_");
                    AssetDatabase.CreateAsset(overrideController, temp_path);
                }

                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
                overrideController.GetOverrides(overrides);
                for (int i = 0; i < overrides.Count; i++)
                {
                    var newClip = AnimationConverter.ConvertClip(convertFrom, rootBoneOfConvertFrom, convertTo, overrides[i].Key, isCreatedByUnity, frameRate, noScale, controllerFolder);
                    if (newClip == null)
                    {
                        Debug.LogError("Failed to convert " + overrides[i].Key.name);
                        break;
                    }
                    overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, newClip);
                }

                overrideController.ApplyOverrides(overrides);

                AssetDatabase.SaveAssetIfDirty(overrideController);
                EditorGUIUtility.PingObject(overrideController);
            }

        }



    }

}
