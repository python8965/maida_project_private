using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OnlyNew.BodyProportions
{
    public class AnimationConverter : EditorWindow
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
        public bool discardScale = false;
        [Tooltip("The path of new clip")]
        public string path = "Body Proportions/Output";

        readonly GUIContent convertFromLabel = new GUIContent("Convert From", "The animator's gameObject has hierarchy of skeleton before setup of ScalableBonesManager");
        readonly GUIContent rootBoneLabel = new GUIContent("RootBone of ConvertFrom", "Root bone of the model which you want to convert its animation");
        readonly GUIContent convertToLabel = new GUIContent("Convert To", "The animator's gameObject has hierarchy of skeleton after setup of ScalableBonesManager");
        readonly GUIContent clipLabel = new GUIContent("Clip", "The animation clip which you want to convert.");
        readonly GUIContent CreatedByUnityLabel = new GUIContent("Is created by Unity", "Was the animation waiting to be converted created by Unity?");
        readonly GUIContent frameRateLabel = new GUIContent("Output FrameRate", "The framerate of new animation clip. It is recommended to use a higher frame rate to get a smooth and precise animation.");
        readonly GUIContent pathLabel = new GUIContent("Path", "The path of new clip.");
        readonly GUIContent discardScaleLabel = new GUIContent("Discard Scale", "If checked, Discard scale animation.");

        //const string rotationProperty = "localEulerAnglesBaked";
        [MenuItem("Tools/Body Proportions/Animation Converter", false, 1)]
        static void Init()
        {
            AnimationConverter window = GetWindow<AnimationConverter>(false, "Animation Converter");
            window.Show();
        }

        private void OnGUI()
        {
            convertFrom = EditorGUILayout.ObjectField(convertFromLabel, convertFrom, typeof(Animator), true) as Animator;
            rootBoneOfConvertFrom = EditorGUILayout.ObjectField(rootBoneLabel, rootBoneOfConvertFrom, typeof(GameObject), true) as GameObject;
            convertTo = EditorGUILayout.ObjectField(convertToLabel, convertTo, typeof(Animator), true) as Animator;
            clip = EditorGUILayout.ObjectField(clipLabel, clip, typeof(AnimationClip), true) as AnimationClip;
            isCreatedByUnity = EditorGUILayout.Toggle(CreatedByUnityLabel, isCreatedByUnity);
            frameRate = EditorGUILayout.IntField(frameRateLabel, frameRate);
            discardScale = EditorGUILayout.Toggle(discardScaleLabel, discardScale);
            path = EditorGUILayout.TextField(pathLabel, path);

            //EditorGUILayout.LabelField("eg. Body Proportions/Output");
            if (GUILayout.Button("Convert", GUILayout.Height(26f)))
            {
                ConvertClip(convertFrom, rootBoneOfConvertFrom, convertTo, clip, isCreatedByUnity, frameRate, discardScale, path);
            }
        }
        /// <summary>
        /// This method will help you to convert animation. 
        /// </summary>
        /// <param name="convertFrom">The hierarchy of skeleton before setup of ScalableBonesManager</param>
        /// <param name="convertTo">The hierarchy of skeleton after setup of ScalableBonesManager</param>
        /// <param name="clip">The animation clip which you want to convert.</param>
        /// <param name="frameRate">The frameRate of new animation clip</param>
        /// <param name="path">The path of new clip. eg. "Body Proportions/Output"</param>
        public static AnimationClip ConvertClip(Animator convertFrom, GameObject rootBoneOfConvertFrom, Animator convertTo, AnimationClip clip, bool theAnimationIsCreatedByUnity, int frameRate, bool noScale, string path)
        {
            AnimationClip newClip = null;
            if (CheckAndCreatePath(path))
            {
                if (convertFrom != null && convertTo != null && rootBoneOfConvertFrom != null && clip != null && path != "")
                {
                    newClip = InternalConvertClip(convertFrom, rootBoneOfConvertFrom, convertTo, clip, theAnimationIsCreatedByUnity, frameRate, noScale, path);
                }
            }
            else
            {
                Debug.LogError("can't create folder because the format of path is wrong. Please check your output path ");
            }
            return newClip;
        }

        public static bool CheckAndCreatePath(string path)
        {
            string t_path = "";
            if (path.First<char>() != '/')
            {
                t_path = Path.Combine(Application.dataPath, path);
            }
            else
            {
                t_path = Path.Combine(Application.dataPath, path.Substring(1));
            }

            if (!Directory.Exists(t_path))
            {
                Directory.CreateDirectory(t_path);
            }
            return Directory.Exists(t_path);
        }
        static AnimationClip InternalConvertClip(Animator convertFrom, GameObject rootBoneOfConvertFrom, Animator convertTo, AnimationClip clip, bool theAnimationIsCreatedByUnity, int frameRate, bool noScale, string path)
        {
            var bindgings = AnimationUtility.GetCurveBindings(clip);
            string str = clip.name + "\n";
            HashSet<float> keyTimes = new HashSet<float> { };
            Dictionary<string, List<CurveProperty>> curvePropertyDict = new Dictionary<string, List<CurveProperty>>();

            curvePropertyDict.Clear();
            keyTimes.Clear();

            bool recordAllDetails = !theAnimationIsCreatedByUnity;
            //record bones transform
            if (!theAnimationIsCreatedByUnity)
            {
                var originalBones = rootBoneOfConvertFrom.GetComponentsInChildren<Transform>();
                Undo.RecordObjects(originalBones, "record bones transform");
            }
            //Prepare data
            for (int i = 0; i < bindgings.Length; i++)
            {
                EditorCurveBinding binding = bindgings[i];
                var t_path = binding.path;

                var objectName = t_path.Split('/').Last();
                if (!curvePropertyDict.ContainsKey(objectName))
                {
                    curvePropertyDict.Add(objectName, new List<CurveProperty>());
                }

                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                Keyframe[] keyframes = curve.keys;
                curvePropertyDict[objectName].Add(new CurveProperty(binding.propertyName, binding.type, curve, keyframes, t_path, binding));

                str += t_path + "," + binding.propertyName + "," + binding.type.ToString() + "\n";
                //foreach (var item in keyframes)
                //{
                //    str += ArrayString(
                //        "value", item.value,
                //        "inTangent", item.inTangent,
                //        "outTangent", item.outTangent,
                //        "inWeight", item.inWeight,
                //        "outWeight", item.outTangent,
                //        //"tangentMode", item.tangentMode.ToString(),
                //        "weightMode", item.weightedMode.ToString(), "\n"
                //        );
                //}
                str += "\n";
            }

            var objsTo = convertTo.GetComponentsInChildren<Transform>(true);
            var objsFrom = convertFrom.GetComponentsInChildren<Transform>(true);
            CheckNames(objsFrom);
            CheckNames(objsTo);
            GameObject convertFromObj, convertToCopy;
            //var dictFrom = CopyObjectTree(convertFrom.gameObject, out convertFromCopy);

            Dictionary<string, GameObject> dictFrom = new Dictionary<string, GameObject>();
            dictFrom.Add(convertFrom.gameObject.name, convertFrom.gameObject);
            GetTreeDict(convertFrom.gameObject, dictFrom);
            convertFromObj = convertFrom.gameObject;
            var dictTo = CopyObjectTree(convertTo.gameObject, out convertToCopy);

            GetChildrenDict<ScalableBone>(convertTo.gameObject, out Dictionary<string, ScalableBone> scalableBoneDict);

            Dictionary<EditorCurveBinding, AnimationCurve> NotTransformCurves = new Dictionary<EditorCurveBinding, AnimationCurve>();

            //collect keyframe.time
            for (int i = 0; i < objsFrom.Length; i++)
            {
                string name = GetPathName(objsFrom[i].gameObject, convertFrom.gameObject);

                List<CurveProperty> curvePropertyList = new List<CurveProperty>();

                if (curvePropertyDict.TryGetValue(name, out curvePropertyList))
                {
                    foreach (var curveProperty in curvePropertyList)
                    {
                        if (curveProperty.type == typeof(Transform) || curveProperty.type == typeof(Animator))
                        {
                            foreach (var keyframe in curveProperty.keyframes)
                            {
                                keyTimes.Add(keyframe.time);
                            }
                        }
                    }
                }
            }

            //Find Max KeyTime
            var maxKeyTime = 0f;
            foreach (var keyTime in keyTimes)
            {
                if (maxKeyTime < keyTime)
                {
                    maxKeyTime = keyTime;
                }
            }


            //add every frame. This step may be deprecated in the future
            float t_frameRate;
            if (frameRate <= 0)
            {
                t_frameRate = clip.frameRate;
            }
            else
            {
                t_frameRate = frameRate;
            }
            List<float> fullKeyTimes = new List<float>();
            var keyTimeDelta = 1f / t_frameRate;
            var maxFrameCount = (maxKeyTime * t_frameRate) + 1;
            for (int i = 0; i <= maxFrameCount * 2; i++)
            {
                var t_keyTime = i * keyTimeDelta;
                if (t_keyTime > maxKeyTime + 0.001f)
                {
                    break;
                }
                fullKeyTimes.Add(t_keyTime);
            }

            Dictionary<string, CurveData> curveDatas = new Dictionary<string, CurveData>();
            Dictionary<string, Quaternion> preObjToLocalRotation = new Dictionary<string, Quaternion>();
            foreach (var keyTime in fullKeyTimes)
            {
                HashSet<Transform> objsTransformed = new HashSet<Transform>();
                Dictionary<string, TransformedDetails> objsTransformedDetails = new Dictionary<string, TransformedDetails>();
                HashSet<Transform> objsShouldBeConverted = new HashSet<Transform>();

                HashSet<Transform> objsBeConverted = new HashSet<Transform>();
                //animating skeleton for fbx
                if (!theAnimationIsCreatedByUnity)
                {
                    clip.SampleAnimation(convertFromObj, keyTime);
                }
                //assume each bone has been transformed
                GameObject rootBoneCopy = null;
                if (!theAnimationIsCreatedByUnity)
                {
                    rootBoneCopy = dictFrom[rootBoneOfConvertFrom.name];
                    var transforms = rootBoneCopy.GetComponentsInChildren<Transform>();
                    for (int i = 1; i < transforms.Length; i++)
                    {
                        Transform item = transforms[i];
                        objsTransformed.Add(item.transform);
                    }
                }
                //Read clip of objsFrom value
                for (int i = 0; i < objsFrom.Length; i++)
                {
                    string pathName = GetPathName(objsFrom[i].gameObject, convertFrom.gameObject);

                    var transform = objsFrom[i].transform;
                    List<CurveProperty> curveProperties;
                    VectorBinding position = new VectorBinding(0);
                    VectorBinding rotation = new VectorBinding(0);
                    VectorBinding quaternion = new VectorBinding(0);
                    VectorBinding scale = new VectorBinding(1);

                    //read curve value
                    if (curvePropertyDict.TryGetValue(pathName, out curveProperties))
                    {
                        foreach (var curveProperty in curveProperties)
                        {
                            if (curveProperty.type == typeof(Transform))
                            {
                                var value = curveProperty.curve.Evaluate(keyTime);
                                switch (curveProperty.propertyName)
                                {
                                    case "m_LocalPosition.x":
                                        position.X = value;
                                        break;
                                    case "m_LocalPosition.y":
                                        position.Y = value;
                                        break;
                                    case "m_LocalPosition.z":
                                        position.Z = value;
                                        break;
                                    case "localEulerAnglesRaw.x":
                                        rotation.X = value;
                                        break;
                                    case "localEulerAnglesRaw.y":
                                        rotation.Y = value;
                                        break;
                                    case "localEulerAnglesRaw.z":
                                        rotation.Z = value;
                                        break;
                                    case "m_LocalRotation.x":
                                        quaternion.X = value;
                                        break;
                                    case "m_LocalRotation.y":
                                        quaternion.Y = value;
                                        break;
                                    case "m_LocalRotation.z":
                                        quaternion.Z = value;
                                        break;
                                    case "m_LocalRotation.w":
                                        quaternion.W = value;
                                        break;
                                    case "m_LocalScale.x":
                                        scale.X = value;
                                        break;
                                    case "m_LocalScale.y":
                                        scale.Y = value;
                                        break;
                                    case "m_LocalScale.z":
                                        scale.Z = value;
                                        break;
                                    default:
                                        Debug.LogError("Unsupported curve property found:" + curveProperty.propertyName);
                                        break;
                                }
                            }
                            else //if (curveProperty.type == typeof(Animator))
                            {
                                var objFrom = objsFrom[i].gameObject;

                                if (objFrom.transform.IsChildOf(rootBoneOfConvertFrom.transform))
                                {
                                    //replace path
                                    if (objFrom == convertFromObj)
                                    {
                                        curveProperty.binding.path = "";
                                    }
                                    else
                                    {
                                        var objTo = dictTo[objFrom.name];
                                        curveProperty.binding.path = AnimationUtility.CalculateTransformPath(objTo.transform, convertToCopy.transform);
                                    }
                                }

                                if (!NotTransformCurves.ContainsKey(curveProperty.binding))
                                    NotTransformCurves.Add(curveProperty.binding, curveProperty.curve);
                            }

                        }
                        if (position.TheVectorIsCorrupted() ||
                            rotation.TheVectorIsCorrupted() ||
                            scale.TheVectorIsCorrupted() || quaternion.TheQuaternionIsCorrupted()
                            )
                        {
                            Debug.LogError("animation clip data is not complete");
                            return null;
                        }
                    }
                    else
                    {
                        //Debug.Log(name + " can't be found in clip"); 
                    }

                    if (!position.TheVectorIsComplete)
                    {
                        position.Set(transform.localPosition);
                    }
                    if (!rotation.TheVectorIsComplete)
                    {
                        rotation.Set(transform.localEulerAngles);
                    }
                    if (!quaternion.TheQuaternionIsComplete)
                    {
                        quaternion.Set(transform.localRotation);
                    }
                    if (!scale.TheVectorIsComplete)
                    {
                        scale.Set(transform.localScale);
                    }

                    //apply animation to convertFrom
                    string name = objsFrom[i].gameObject.name;
                    var objCopy = dictFrom[name];
                    objCopy.transform.localPosition = position.ToVector3();

                    if (rotation.TheVectorIsComplete)
                    {
                        objCopy.transform.localEulerAngles = rotation.ToVector3();
                    }
                    else if (quaternion.TheQuaternionIsComplete)
                    {
                        objCopy.transform.localRotation = quaternion.ToQuaternion();
                    }
                    objCopy.transform.localScale = scale.ToVector3();
                    if (position.TheVectorIsComplete || rotation.TheVectorIsComplete || scale.TheVectorIsComplete || quaternion.TheQuaternionIsComplete)
                    {
                        var details = new TransformedDetails();
                        details.positionChanged = position.TheVectorIsComplete;
                        details.rotationChanged = rotation.TheVectorIsComplete || quaternion.TheQuaternionIsComplete;
                        details.scaleChanged = scale.TheVectorIsComplete;
                        objsTransformedDetails.Add(objCopy.name, details);
                        objsTransformed.Add(objCopy.transform);
                    }
                }



                //Copy each transform From convertFrom to convertTo objects
                //initialize transform
                foreach (var key in dictFrom.Keys) //the dictFrom now includes animator root
                {
                    var objFrom = dictFrom[key];
                    GameObject objTo = null;
                    if (objFrom == convertFromObj)
                        objTo = convertToCopy;
                    else
                        dictTo.TryGetValue(key, out objTo);
                    if (objTo != null)
                    {
                        objTo.transform.position = objFrom.transform.position;
                        objTo.transform.rotation = objFrom.transform.rotation;
                        //objTo.transform.localScale = objFrom.transform.localScale;
                    }
                }

                //Add child
                foreach (var obj in objsTransformed)
                {
                    var children = obj.GetComponentsInChildren<Transform>(true);
                    foreach (var child in children)
                    {
                        objsShouldBeConverted.Add(child);
                    }
                }

                //send tranform data from convertFrom to convertToCopy
                foreach (var obj in objsShouldBeConverted)
                {
                    var relativeLocalPosition = convertFromObj.transform.InverseTransformPoint(obj.position);
                    var relativeLocalRotation = Quaternion.Inverse(convertFromObj.transform.rotation) * obj.rotation;
                    var localScale = obj.localScale;
                    List<CurveProperty> curvePropertyList;
                    GameObject objTo;
                    Dictionary<string, Keyframe[]> keysDict = new Dictionary<string, Keyframe[]>();

                    string pathName = GetPathName(obj.gameObject, convertFrom.gameObject);
                    if (curvePropertyDict.TryGetValue(pathName, out curvePropertyList))
                    {
                        foreach (var curveProperty in curvePropertyList)
                        {
                            var propertyName = curveProperty.propertyName;
                            if (!keysDict.ContainsKey(propertyName))
                                keysDict.Add(propertyName, curveProperty.keyframes);
                        }
                    }
                    string replacedNameWhileRoot = obj.gameObject == convertFromObj ? convertToCopy.name : obj.gameObject.name;
                    if (dictTo.TryGetValue(replacedNameWhileRoot, out objTo))
                    {
                        if (obj.parent)
                        {
                            var parentName = obj.parent.gameObject.name;
                            if (dictTo.TryGetValue(parentName, out GameObject parent))
                            {
                                objTo.transform.position = parent.transform.TransformPoint(obj.localPosition);
                            }
                            else
                            {
                                objTo.transform.position = convertToCopy.transform.TransformPoint(relativeLocalPosition);
                            }
                        }
                        else
                        {
                            objTo.transform.position = convertToCopy.transform.TransformPoint(relativeLocalPosition);
                        }

                        var rot = convertToCopy.transform.rotation * relativeLocalRotation;
                        objTo.transform.rotation = rot;
                        //objTo.transform.localScale = convertToCopy.transform.localScale;
                        objsBeConverted.Add(objTo.transform);

                        Quaternion localRotation;
                        localRotation = objTo.transform.localRotation;
                        //collect bindings and keys
                        var t_path = AnimationUtility.CalculateTransformPath(objTo.transform, convertToCopy.transform);
                        if (objsTransformedDetails.TryGetValue(obj.gameObject.name, out TransformedDetails transformedDetails))
                        {
                            if (transformedDetails.positionChanged || recordAllDetails)
                            {
                                CollectCurveData(curveDatas, keyTime, objTo.transform.localPosition.x, t_path, "m_LocalPosition.x", typeof(Transform));
                                CollectCurveData(curveDatas, keyTime, objTo.transform.localPosition.y, t_path, "m_LocalPosition.y", typeof(Transform));
                                CollectCurveData(curveDatas, keyTime, objTo.transform.localPosition.z, t_path, "m_LocalPosition.z", typeof(Transform));
                                if (scalableBoneDict.TryGetValue(obj.gameObject.name, out ScalableBone scalableBone))
                                {
                                    if (obj.parent != null)
                                    {
                                        var parentName = obj.parent.gameObject.name;
                                        if (dictTo.TryGetValue(parentName, out GameObject parent))
                                        {
                                            var positionOffset = parent.transform.InverseTransformPoint(objTo.transform.position) - scalableBone.OriginalLocalPosition;
                                            CollectCurveData(curveDatas, keyTime, positionOffset.x, t_path, "positionOffset.x", typeof(ScalableBone));
                                            CollectCurveData(curveDatas, keyTime, positionOffset.y, t_path, "positionOffset.y", typeof(ScalableBone));
                                            CollectCurveData(curveDatas, keyTime, positionOffset.z, t_path, "positionOffset.z", typeof(ScalableBone));
                                        }
                                    }
                                }
                            }
                            if (transformedDetails.rotationChanged || recordAllDetails)
                            {
                                CollectCurveData(curveDatas, keyTime, localRotation.x, t_path, "m_LocalRotation.x", typeof(Transform));
                                CollectCurveData(curveDatas, keyTime, localRotation.y, t_path, "m_LocalRotation.y", typeof(Transform));
                                CollectCurveData(curveDatas, keyTime, localRotation.z, t_path, "m_LocalRotation.z", typeof(Transform));
                                CollectCurveData(curveDatas, keyTime, localRotation.w, t_path, "m_LocalRotation.w", typeof(Transform));
                                //CollectCurveData(curveDatas, keyTime, eulerAngle.x, t_path, rotationProperty + ".x", typeof(Transform));
                                //CollectCurveData(curveDatas, keyTime, eulerAngle.y, t_path, rotationProperty + ".y", typeof(Transform));
                                //CollectCurveData(curveDatas, keyTime, eulerAngle.z, t_path, rotationProperty + ".z", typeof(Transform));
                            }
                            if (transformedDetails.scaleChanged || recordAllDetails)
                            {
                                if (!noScale)
                                {
                                    CollectCurveData(curveDatas, keyTime, objTo.transform.localScale.x, t_path, "m_LocalScale.x", typeof(Transform));
                                    CollectCurveData(curveDatas, keyTime, objTo.transform.localScale.y, t_path, "m_LocalScale.y", typeof(Transform));
                                    CollectCurveData(curveDatas, keyTime, objTo.transform.localScale.z, t_path, "m_LocalScale.z", typeof(Transform));
                                }

                            }
                        }
                        else
                        {
                            CollectCurveData(curveDatas, keyTime, objTo.transform.localPosition.x, t_path, "m_LocalPosition.x", typeof(Transform));
                            CollectCurveData(curveDatas, keyTime, objTo.transform.localPosition.y, t_path, "m_LocalPosition.y", typeof(Transform));
                            CollectCurveData(curveDatas, keyTime, objTo.transform.localPosition.z, t_path, "m_LocalPosition.z", typeof(Transform));
                            //CollectCurveData(curveDatas, keyTime, eulerAngle.x, t_path, rotationProperty + ".x", typeof(Transform));
                            //CollectCurveData(curveDatas, keyTime, eulerAngle.y, t_path, rotationProperty + ".y", typeof(Transform));
                            //CollectCurveData(curveDatas, keyTime, eulerAngle.z, t_path, rotationProperty + ".z", typeof(Transform));
                            CollectCurveData(curveDatas, keyTime, localRotation.x, t_path, "m_LocalRotation.x", typeof(Transform));
                            CollectCurveData(curveDatas, keyTime, localRotation.y, t_path, "m_LocalRotation.y", typeof(Transform));
                            CollectCurveData(curveDatas, keyTime, localRotation.z, t_path, "m_LocalRotation.z", typeof(Transform));
                            CollectCurveData(curveDatas, keyTime, localRotation.w, t_path, "m_LocalRotation.w", typeof(Transform));
                        }
                    }
                    else
                    {
                        Debug.LogError("can't find " + obj.gameObject.name);
                    }
                }

            }



            //get object reference binding
            var objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            List<ObjectReferenceKeyframe[]> objReferenceCurvesList = new List<ObjectReferenceKeyframe[]>();
            foreach (var objectBinding in objectBindings)
            {
                objReferenceCurvesList.Add(AnimationUtility.GetObjectReferenceCurve(clip, objectBinding));
            }

            //load clip from disk

            var settings = AnimationUtility.GetAnimationClipSettings(clip);

            AnimationClip newClip;
            string clipPath = "Assets" + "/" + path + "/" + clip.name + "_converted" + ".anim";
            newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (newClip == null)
            {
                newClip = new AnimationClip();
                string temp_path = clipPath.Replace("|", "_");
                AssetDatabase.CreateAsset(newClip, temp_path);
            }
            newClip.ClearCurves();
            newClip.legacy = clip.legacy;
            newClip.frameRate = t_frameRate;
            newClip.localBounds = clip.localBounds;
            newClip.wrapMode = clip.wrapMode;
            newClip.name = clip.name + "_converted";
            AnimationUtility.SetAnimationEvents(newClip, clip.events);
            AnimationUtility.SetAnimationClipSettings(newClip, settings);


            //save curve to clip
            foreach (var curveData in curveDatas.Values)
            {
                AnimationCurve curve = new AnimationCurve();
                curve.keys = curveData.keys.ToArray();
                curveData.curve = curve;
                //set keys tangent mode
                for (int i = 0; i < curve.keys.Length; i++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curveData.curve, i, AnimationUtility.TangentMode.ClampedAuto);
                    AnimationUtility.SetKeyRightTangentMode(curveData.curve, i, AnimationUtility.TangentMode.ClampedAuto);
                }
                newClip.SetCurve(curveData.binding.path, curveData.binding.type, curveData.binding.propertyName, curveData.curve);
            }
            for (int i = 0; i < objectBindings.Length; i++)
            {
                EditorCurveBinding objectBinding = objectBindings[i];
                AnimationUtility.SetObjectReferenceCurve(newClip, objectBinding, objReferenceCurvesList[i]);
            }
            foreach (var binding in NotTransformCurves.Keys)
            {
                AnimationUtility.SetEditorCurve(newClip, binding, NotTransformCurves[binding]);
            }

            //save clip
            if (newClip != null)
            {
#if UNITY_2020 //BP
                EditorUtility.SetDirty(newClip);
#elif UNITY_2020_3_OR_NEWER
                AssetDatabase.SaveAssetIfDirty(newClip);
#endif
            }

            Debug.Log(str);
            if (!theAnimationIsCreatedByUnity)
            {
                Undo.PerformUndo();
            }
            //GC
            //DestroyImmediate(convertFromCopy);
            DestroyImmediate(convertToCopy);
            return newClip;
        }

        private static string GetPathName(GameObject target, GameObject root)
        {
            if (target == root)
                return "";
            else
                return target.name;
        }

        private static void CollectCurveData(Dictionary<string, CurveData> curveDatas, float keyTime, float value, string t_path, string property, Type inType)
        {
            if (property == "")
            {
                Debug.LogError("CollectCurveData " + " failed");
                return;
            }
            //TODO: rotation or euler
            string pathAndProperty = t_path + "/" + property;
            Keyframe oldKey = new Keyframe(keyTime, value);

            Keyframe newKey = oldKey;

            newKey.value = value;
            newKey.time = keyTime;
            if (curveDatas.ContainsKey(pathAndProperty))
            {

                curveDatas[pathAndProperty].keys.Add(newKey);
            }
            else
            {
                CurveData curveData = new CurveData(EditorCurveBinding.FloatCurve(t_path, inType, property));
                curveData.keys.Add(newKey);
                curveDatas.Add(pathAndProperty, curveData);
            }
        }


        private static Dictionary<string, GameObject> CopyObjectTree(GameObject obj, out GameObject t_root)
        {
            Dictionary<string, GameObject> dict = new Dictionary<string, GameObject>();
            GameObject root = new GameObject(obj.name);
            root.hideFlags = HideFlags.HideAndDontSave;
            dict.Add(root.name, root);
            CopyChildObject(obj, root, dict);
            t_root = root;
            return dict;
        }
        private static void GetTreeDict(GameObject obj, Dictionary<string, GameObject> dict)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                var child = obj.transform.GetChild(i);
                if (!dict.ContainsKey(child.name))
                    dict.Add(child.name, child.gameObject);
                GetTreeDict(child.gameObject, dict);
            }

        }
        private static void CopyChildObject(GameObject obj, GameObject parent, Dictionary<string, GameObject> dict)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                var child = obj.transform.GetChild(i);
                GameObject childCopy = new GameObject(child.gameObject.name);
                childCopy.transform.SetParent(parent.transform);
                childCopy.transform.localPosition = child.localPosition;
                childCopy.transform.localRotation = child.localRotation;
                childCopy.transform.localScale = child.localScale;
                if (!dict.ContainsKey(childCopy.name))
                    dict.Add(childCopy.name, childCopy);
                CopyChildObject(child.gameObject, childCopy, dict);
            }
        }

        private static void CheckNames(Transform[] objs)
        {
            HashSet<string> names = new HashSet<string>();
            //check name dulication
            for (int i = 0; i < objs.Length; i++)
            {
                if (names.Contains(objs[i].gameObject.name))
                {
                    Debug.LogError("GameObjects \"" + objs[i].gameObject.name + "\" with duplicate names will be ignored!");
                }
                else
                {
                    names.Add(objs[i].gameObject.name);
                }
            }
        }
        private static void GetChildrenDict<T>(GameObject obj, out Dictionary<string, T> dict) where T : MonoBehaviour
        {
            dict = new Dictionary<string, T>();
            var children = obj.GetComponentsInChildren<T>();
            foreach (var item in children)
            {
                if (!dict.ContainsKey(item.gameObject.name))
                {
                    dict.Add(item.gameObject.name, item);
                }
            }
        }
        private static string ArrayString(params object[] args)
        {
            var str = "";
            for (int i = 0; i < args.Length; i++)
            {
                str += args[i].ToString() + ",";
            }
            return str;
        }

        private class VectorBinding
        {

            private float x;
            private float y;
            private float z;
            private float w;
            private bool hasX;
            private bool hasY;
            private bool hasZ;
            private bool hasW;

            public float X { get => x; set { x = value; hasX = true; } }
            public float Y { get => y; set { y = value; hasY = true; } }
            public float Z { get => z; set { z = value; hasZ = true; } }
            public float W { get => w; set { w = value; hasW = true; } }

            public bool HasX { get => hasX; }
            public bool HasY { get => hasY; }
            public bool HasZ { get => hasZ; }
            public bool HasW { get => hasW; }

            public bool TheVectorIsComplete
            {
                get { return hasX && hasY && hasZ; }
            }
            public bool TheQuaternionIsComplete
            {
                get { return hasX && hasY && hasZ && hasW; }
            }

            public bool TheVectorIsCorrupted()
            {
                return !TheVectorIsComplete && (hasX || hasY || hasZ);
            }
            public bool TheQuaternionIsCorrupted()
            {
                return !TheQuaternionIsComplete && (hasX || hasY || hasZ || hasW);
            }
            public VectorBinding(float defaultValue)
            {
                this.x = defaultValue;
                this.y = defaultValue;
                this.z = defaultValue;
                this.w = defaultValue;
                hasX = false;
                hasY = false;
                hasZ = false;
                hasW = false;
            }
            public Vector3 ToVector3()
            {
                return new Vector3(x, y, z);
            }
            public Quaternion ToQuaternion()
            {
                return new Quaternion(x, y, z, w);
            }
            public void Set(Vector3 vector)
            {
                this.x = vector.x;
                this.y = vector.y;
                this.z = vector.z;
            }
            public void Set(Quaternion quaternion)
            {
                this.x = quaternion.x;
                this.y = quaternion.y;
                this.z = quaternion.z;
                this.w = quaternion.w;
            }
        }
        private class CurveProperty
        {
            public string path;
            public string propertyName;
            public Type type;
            public AnimationCurve curve;
            public Keyframe[] keyframes;
            public EditorCurveBinding binding;
            public CurveProperty(string propertyName, Type type, AnimationCurve curve, Keyframe[] keyframes, string path, EditorCurveBinding binding)
            {
                this.propertyName = propertyName;
                this.type = type;
                this.curve = curve;
                this.keyframes = keyframes;
                this.path = path;
                this.binding = binding;
            }
        }
        private class TransformedDetails
        {
            public bool positionChanged = false;
            public bool rotationChanged = false;
            public bool scaleChanged = false;
        }
        private class CurveData
        {
            public EditorCurveBinding binding;
            public AnimationCurve curve;
            public List<Keyframe> keys = new List<Keyframe>();
            public CurveData(EditorCurveBinding editorCurveBinding)
            {
                this.binding = editorCurveBinding;
            }
        }
    }

}
