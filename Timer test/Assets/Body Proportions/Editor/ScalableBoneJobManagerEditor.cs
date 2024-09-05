#if !UNITY_WEBGL
using UnityEditor;
using UnityEngine;

namespace OnlyNew.BodyProportions
{
    [CustomEditor(typeof(ScalableBoneJobManager))]
    public class ScalableBoneJobManagerEditor : Editor
    {

        GUIStyle guiStyle = new GUIStyle();
        static bool foldout = false;
        private void OnEnable()
        {
            guiStyle.normal.textColor = Color.gray;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "Info");
            if (foldout)
            {
                var t = target as ScalableBoneJobManager;
                EditorGUILayout.LabelField($"Bone Count:{t.BoneCount}", guiStyle);
                EditorGUILayout.LabelField($"Max Bone Depth:{t.MaxDepth}", guiStyle);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
#endif