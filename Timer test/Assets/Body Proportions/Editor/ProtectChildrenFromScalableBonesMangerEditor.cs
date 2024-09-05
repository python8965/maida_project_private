using UnityEditor;
namespace OnlyNew.BodyProportions
{
    [CustomEditor(typeof(ProtectChildrenFromScalableBonesManager))]
    public class ProtectChildrenFromScalableBonesMangerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("The component is a marker.\nBy default, after clicking the \"Auto Setup\" button, ScalableBoneManager automatically converts all bones to Scalable Bones. If a bone has a ProtectChildrenFromScalableBonesManager component, its children will not be converted to Scalable Bone.", MessageType.Info);
        }
    }
}
