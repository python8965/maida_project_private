using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace OnlyNew.BodyProportions
{
    using BoneType = ScalableBoneRenderer.BoneType;

    [InitializeOnLoad]
    public static class BoneRenderTask
    {
        static List<ScalableBoneRenderer> ScalableBoneList = new List<ScalableBoneRenderer>();

        private static RendererDataBridge scalableBoneBridge;

        private static Material surfaceMat, wireMat;
        private static Shader shader;
        private static Mesh mesh;


        private static int hint = "ScalableBoneHandle".GetHashCode();

        private static int preVisibleLayers = 0;

        static BoneRenderTask()
        {
            ScalableBoneRenderer.onAddBoneRenderer += OnAddBoneRenderer;
            ScalableBoneRenderer.onRemoveBoneRenderer += OnRemoveBoneRenderer;
            SceneVisibilityManager.visibilityChanged += OnVisibilityChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;

            SceneView.duringSceneGui += DrawBones;

            preVisibleLayers = Tools.visibleLayers;
        }

        private static Material GetSurfaceMaterial()
        {

            if (!surfaceMat)
            {
                shader = Shader.Find("Unlit/ScalableBoneRenderer");

                surfaceMat = new Material(shader);
                surfaceMat.enableInstancing = true;
                surfaceMat.hideFlags = HideFlags.DontSaveInEditor;
            }

            return surfaceMat;

        }
        static Material GetWireMaterial()
        {
            if (!wireMat)
            {
                Shader shader = Shader.Find("Unlit/BoneWire");
                if (shader == null)
                {
                    Shader.Find("BonesRenderer");
                }
                wireMat = new Material(shader);
                wireMat.enableInstancing = true;
                wireMat.hideFlags = HideFlags.DontSaveInEditor;
            }

            return wireMat;
        }
        private static Mesh GetMesh()
        {
            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.name = "ScalableBoneMesh";
                mesh.subMeshCount = (int)RendererDataBridge.SubmeshType.Count;
                mesh.hideFlags = HideFlags.DontSave;

                // Bone vertices
                Vector3[] vertices = new Vector3[]
                {
                        new Vector3(0.0f    , 0f        , 0.0f   ),
                        new Vector3(0.127322f  , 0.127322f , 0.127322f ),
                        new Vector3(0.127322f  , 0.127322f , -0.127322f),
                        new Vector3(-0.127322f , 0.127322f , -0.127322f),
                        new Vector3(-0.127322f , 0.127322f , 0.127322f ),
                        new Vector3(0.127322f  , 0.381966f , 0.127322f ),
                        new Vector3(0.127322f  , 0.381966f , -0.127322f),
                        new Vector3(-0.127322f , 0.381966f , -0.127322f),
                        new Vector3(-0.127322f , 0.381966f , 0.127322f ),
                        new Vector3(0f       , 1f           , 0f    ),
                };

                mesh.vertices = vertices;

                // Build indices for different sub meshes
                int[] boneFaceIndices = new int[]
                {
                        0,2,1,
                        0,3,2,
                        0,4,3,
                        0,1,4,
                        1,6,5,
                        1,2,6,
                        2,7,6,
                        2,3,7,
                        3,8,7,
                        3,4,8,
                        4,5,8,
                        4,1,5,
                        9,5,6,
                        9,6,7,
                        9,7,8,
                        9,8,5
                };
                mesh.SetIndices(boneFaceIndices, MeshTopology.Triangles, (int)RendererDataBridge.SubmeshType.Surface);
                mesh.RecalculateNormals();
                int[] boneWires = new int[]
                {
                        0, 1, 0, 2, 0, 3, 0, 4,
                        1, 2, 2, 3, 3, 4, 4, 1,
                        1,5,2,6,3,7,4,8 ,
                        5,6,6,7,7,8,8,5 ,
                        9,5,9,6,9,7,9,8
                };
                mesh.SetIndices(boneWires, MeshTopology.Lines, (int)RendererDataBridge.SubmeshType.Line);
            }
            return mesh;
        }

        private static Matrix4x4 ComputeScalableBoneMatrix(ScalableBoneRenderer.BoneData boneData, Transform transform, Vector3 start, Vector3 end, float length, Vector3 scale)
        {
            var unscaledEnd = new Vector3(
                            boneData.childOriginalLocalPosition.x / boneData.start.localScale.x,
                            boneData.childOriginalLocalPosition.y / boneData.start.localScale.y,
                            boneData.childOriginalLocalPosition.z / boneData.start.localScale.z
                            );
            var unscaledEndWorld = transform.TransformPoint(unscaledEnd);
            var direction = unscaledEndWorld - start;
            var originalLength = direction.magnitude;
            Vector3 directionNormailized = direction.normalized;
            Vector3 tangent = Vector3.Cross(directionNormailized, transform.up);
            if (Vector3.SqrMagnitude(tangent) < 0.1f)
                tangent = Vector3.Cross(directionNormailized, transform.right);
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(directionNormailized, tangent);

            Vector3 t_scale = originalLength * scale;
            var t_length = (transform.TransformPoint(boneData.childOriginalLocalPosition) - start).magnitude;

            var matrix = Matrix4x4.Translate(start)
                * Matrix4x4.Rotate(Quaternion.LookRotation(bitangent, directionNormailized))
                * Matrix4x4.Scale(new Vector3(t_scale.z, t_length, t_scale.y));
            return matrix;
        }

        static void Init()
        {
            if (scalableBoneBridge == null || surfaceMat == null || shader == null)
            {
                scalableBoneBridge = new RendererDataBridge();
                scalableBoneBridge.mesh = GetMesh();
                scalableBoneBridge.surfaceMat = GetSurfaceMaterial();
                //scalableBoneBridge.wireMat = GetWireMaterial();
            }
            if (scalableBoneBridge == null)
            {
                Debug.LogError("scalableBoneBridge initialization fail");
            }
        }
        private static void DrawBones(SceneView sceneview)
        {
            if (Tools.visibleLayers != preVisibleLayers)
            {
                OnVisibilityChanged();
                preVisibleLayers = Tools.visibleLayers;
            }

            var gizmoColor = Gizmos.color;
            Init();
            scalableBoneBridge.Clear();

            for (var i = 0; i < ScalableBoneList.Count; i++)
            {
                var boneRenderer = ScalableBoneList[i];

                if (boneRenderer.BonesData == null)
                    continue;

                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null)
                {
                    StageHandle stageHandle = prefabStage.stageHandle;
                    if (stageHandle.IsValid() && !stageHandle.Contains(boneRenderer.gameObject))
                        continue;
                }



                if (boneRenderer.visible)
                {
                    var size = 0.025f;
                    var boneType = boneRenderer.boneType;
                    var surfaceColor = boneRenderer.surfaceColor;
                    var wireColor = boneRenderer.wireColor;

                    for (var j = 0; j < boneRenderer.BonesData.Length; j++)
                    {
                        var bone = boneRenderer.BonesData[j];
                        if (!bone.scalableBone.visible)
                            continue;
                        if (bone.start == null || bone.currentLocalEnd == null)
                            continue;

                        UserOperationProcess(bone, boneType, surfaceColor, wireColor, size);
                    }

                    for (var k = 0; k < boneRenderer.Tips.Length; k++)
                    {
                        var tip = boneRenderer.Tips[k];
                        if (tip == null)
                            continue;

                        UserOperationProcessForTip(tip, Vector3.zero, boneType, surfaceColor, size);
                    }
                }


            }

            scalableBoneBridge.SendToGPU();

            Gizmos.color = gizmoColor;
        }
        private static void UserOperationProcess(ScalableBoneRenderer.BoneData boneData, BoneType boneType, Color surfaceColor, Color wireColor
            , float size)
        {
            var localEnd = boneData.currentLocalEnd;
            var transform = boneData.start;
            Vector3 start = transform.position;
            Vector3 end = transform.TransformPoint(boneData.childOriginalLocalPosition);

            GameObject bone = transform.gameObject;
            float length = (end - start).magnitude;

            int id = GUIUtility.GetControlID(hint, FocusType.Passive);
            Event evt = Event.current;

            switch (evt.GetTypeForControl(id))
            {
                case EventType.Layout:
                    {

                        float maxWidth = transform.localScale.y > transform.localScale.z ? transform.localScale.y : transform.localScale.z;
                        HandleUtility.AddControl(id, HandleUtility.DistanceToLine(start, end) * (1 / length) * 0.06f * (1 / maxWidth));
                        break;
                    }
                case EventType.MouseMove:
                    if (id == HandleUtility.nearestControl)
                        HandleUtility.Repaint();
                    break;
                case EventType.MouseDown:
                    {
                        if (evt.alt)
                            break;

                        if (HandleUtility.nearestControl == id && evt.button == 0)
                        {
                            if (!SceneVisibilityManager.instance.IsPickingDisabled(bone, false))
                            {
                                GUIUtility.hotControl = id; // Grab mouse focus
                                OnMouseDown(bone, evt);
                                evt.Use();
                            }
                        }
                        break;
                    }
                case EventType.MouseUp:
                    {
                        if (GUIUtility.hotControl == id && evt.button == 0)
                        {
                            GUIUtility.hotControl = 0;
                            evt.Use();
                        }
                        break;
                    }
                case EventType.Repaint:
                    {
                        Color highlight = wireColor;

                        bool hoveringBone = GUIUtility.hotControl == 0 && HandleUtility.nearestControl == id;
                        hoveringBone = hoveringBone && !SceneVisibilityManager.instance.IsPickingDisabled(transform.gameObject, false);

                        if (hoveringBone)
                        {
                            highlight = Handles.preselectionColor;
                        }
                        else if (Selection.Contains(bone) || Selection.activeObject == bone)
                        {
                            highlight = Handles.selectedColor;
                        }

                        if (boneType == BoneType.ScalableBone && boneData.childOriginalLocalPosition != Vector3.zero)
                        {
                            scalableBoneBridge.AddData(ComputeScalableBoneMatrix(boneData, transform, start, end, length, transform.localScale), surfaceColor, highlight);

                        }
                        else if (boneType == BoneType.Line)
                        {
                            Handles.color = highlight;
                            Handles.DrawLine(start, end, 2f);
                            Handles.color = new Color(highlight.r, highlight.g, highlight.b, 0.6f);
                            Handles.SphereHandleCap(0, start, Quaternion.identity, 0.0125f, EventType.Repaint);
                        }
                    }
                    break;
            }
        }

        private static void UserOperationProcessForTip(Transform transform, Vector3 localEnd, BoneType shape, Color color, float size)
        {
            Vector3 start = transform.position;
            Vector3 end = transform.TransformPoint(localEnd);

            GameObject boneGO = transform.gameObject;

            float length = (end - start).magnitude;
            bool tipBone = (length < 0.00001f);

            int id = GUIUtility.GetControlID(hint, FocusType.Passive);
            Event evt = Event.current;

            switch (evt.GetTypeForControl(id))
            {
                case EventType.Layout:
                    {
                        HandleUtility.AddControl(id, tipBone ? HandleUtility.DistanceToCircle(start, size * 0.25f) : HandleUtility.DistanceToLine(start, end));
                        break;
                    }
                case EventType.MouseMove:
                    if (id == HandleUtility.nearestControl)
                        HandleUtility.Repaint();
                    break;
                case EventType.MouseDown:
                    {
                        if (evt.alt)
                            break;

                        if (HandleUtility.nearestControl == id && evt.button == 0)
                        {
                            if (!SceneVisibilityManager.instance.IsPickingDisabled(boneGO, false))
                            {
                                GUIUtility.hotControl = id;
                                OnMouseDown(boneGO, evt);
                                evt.Use();
                            }
                        }
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        if (!evt.alt && GUIUtility.hotControl == id)
                        {
                            if (!SceneVisibilityManager.instance.IsPickingDisabled(boneGO, false))
                            {
                                DragAndDrop.PrepareStartDrag();
                                DragAndDrop.objectReferences = new UnityEngine.Object[] { transform };
                                DragAndDrop.StartDrag(ObjectNames.GetDragAndDropTitle(transform));

                                GUIUtility.hotControl = 0;

                                evt.Use();
                            }
                        }
                        break;
                    }
                case EventType.MouseUp:
                    {
                        if (GUIUtility.hotControl == id && (evt.button == 0))
                        {
                            GUIUtility.hotControl = 0;
                            evt.Use();
                        }
                        break;
                    }
                case EventType.Repaint:
                    {
                        Color highlight = color;

                        bool hoveringBone = GUIUtility.hotControl == 0 && HandleUtility.nearestControl == id;
                        hoveringBone = hoveringBone && !SceneVisibilityManager.instance.IsPickingDisabled(transform.gameObject, false);

                        if (hoveringBone)
                        {
                            highlight = Handles.preselectionColor;
                        }
                        else if (Selection.Contains(boneGO) || Selection.activeObject == boneGO)
                        {
                            highlight = Handles.selectedColor;
                        }

                        if (tipBone)
                        {
                            Handles.color = highlight;
                            Handles.SphereHandleCap(0, start, Quaternion.identity, 0.5f * size, EventType.Repaint);
                        }
                    }
                    break;
            }
        }

        public static void OnAddBoneRenderer(ScalableBoneRenderer obj)
        {
            ScalableBoneList.Add(obj);
        }

        public static void OnRemoveBoneRenderer(ScalableBoneRenderer obj)
        {
            ScalableBoneList.Remove(obj);
        }

        public static void OnVisibilityChanged()
        {
            foreach (var boneRenderer in ScalableBoneList)
            {
                boneRenderer.RebuildBonesStruct();
            }

            SceneView.RepaintAll();
        }

        public static void OnHierarchyChanged()
        {
            foreach (var boneRenderer in ScalableBoneList)
            {
                boneRenderer.ReloadScalableBone();
                boneRenderer.RebuildBonesStruct();
            }

            SceneView.RepaintAll();
        }
        public static void OnMouseDown(GameObject gameObject, Event evt)
        {
            if (evt.shift || EditorGUI.actionKey)
            {
                Object[] existingSelection = Selection.objects;
                if (WasSelected(gameObject))
                {
                    Selection.objects = RemoveObjectFromArray(existingSelection, gameObject);
                }
                else
                {
                    Selection.objects = AddObjectToArray(existingSelection, gameObject);
                }
            }
            else
            {
                Selection.activeObject = gameObject;
            }
        }
        /// <summary>
        /// Add object to array
        /// </summary>
        private static T[] AddObjectToArray<T>(T[] array, T obj)
        {
            var tempArray = new T[array.Length + 1];
            System.Array.Copy(array, tempArray, array.Length);
            tempArray[array.Length] = obj;
            return tempArray;
        }

        /// <summary>
        /// remove obj from array 
        /// </summary>
        private static T[] RemoveObjectFromArray<T>(T[] array, T obj)
        {
            var tempArray = new T[array.Length - 1];

            int index = System.Array.IndexOf(array, obj);
            System.Array.Copy(array, tempArray, index);
            System.Array.Copy(array, index + 1, tempArray, index, tempArray.Length - index);
            return tempArray;
        }

        private static bool WasSelected(GameObject gameObject)
        {
            bool result;
            if (EditorGUI.actionKey)
            {
                result = Selection.Contains(gameObject);
            }
            else
            {
                result = Selection.activeGameObject == gameObject;
            }
            return result;
        }

        /// <summary>
        /// The data is waiting here to be sent to the gpu.
        /// </summary>
        private class RendererDataBridge
        {

            public Material surfaceMat, wireMat;

            public enum SubmeshType
            {
                Surface,
                Line,
                Count
            }

            public Mesh mesh;

            List<Vector4> highlights = new List<Vector4>();
            List<Matrix4x4> matrixList = new List<Matrix4x4>();
            List<Vector4> colors = new List<Vector4>();

            const int MaxMeshDrawCount = 1023;
            private static int CalculateBatchCount(int totalCount)
            {
                return Mathf.CeilToInt((totalCount / (float)MaxMeshDrawCount));
            }
            public void AddData(Matrix4x4 matrix, Color color, Color highlight)
            {
                matrixList.Add(matrix);
                colors.Add(color);
                highlights.Add(highlight);
            }

            public void SendToGPU()
            {
                if (matrixList.Count == 0 || colors.Count == 0 || highlights.Count == 0)
                    return;

                Matrix4x4[] matrices = null;
                int count = System.Math.Min(matrixList.Count, System.Math.Min(colors.Count, highlights.Count));

                Material mat = surfaceMat;
                mat.SetPass(0);
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                CommandBuffer commandBuffer = new CommandBuffer();


                int batch = CalculateBatchCount(count);
                for (int i = 0; i < batch; ++i)
                {
                    commandBuffer.Clear();
                    matrices = GetBatch(i, matrixList);
                    propertyBlock.SetVectorArray("_Color", GetBatch(i, colors));


                    commandBuffer.DrawMeshInstanced(mesh, (int)SubmeshType.Surface, surfaceMat, -1, matrices, matrices.Length, propertyBlock);
                    Graphics.ExecuteCommandBuffer(commandBuffer);

                    commandBuffer.Clear();
                    propertyBlock.SetVectorArray("_Color", GetBatch(i, highlights));

                    commandBuffer.DrawMeshInstanced(mesh, (int)SubmeshType.Line, surfaceMat, -1, matrices, matrices.Length, propertyBlock);
                    Graphics.ExecuteCommandBuffer(commandBuffer);
                }
            }
            public void Clear()
            {
                colors.Clear();
                matrixList.Clear();
                highlights.Clear();
            }
            private static T[] GetBatch<T>(int index, List<T> array)
            {
                int rangeCount;
                if (index < (CalculateBatchCount(array.Count) - 1))
                {
                    rangeCount = MaxMeshDrawCount;
                }
                else
                {
                    rangeCount = array.Count - (index * MaxMeshDrawCount);
                }
                var batch = array.GetRange(index * MaxMeshDrawCount, rangeCount);
                return batch.ToArray();
            }


        }
    }
}
