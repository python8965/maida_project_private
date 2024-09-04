using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSceneScript : MonoBehaviour
{
    static public void drawString(string text, Vector3 worldPos, Color? colour = null) {
        UnityEditor.Handles.BeginGUI();

        var restoreColor = GUI.color;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }
        
        var style = new GUIStyle();
        
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.green;
        style.fontSize = 15;
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text, style);
        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();
    }
    
    public IReceiver receiver;
    GameObject[] objs;

    public Mesh mesh;
    public Material material;

    private void OnDrawGizmos()
    {
        
        for (int i = 0; i < 408 / 3; i++)
        {
            try
            {
                drawString((i).ToString(), objs[i].transform.position, Color.green);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (receiver == null)
        {
            receiver = gameObject.GetComponent<IReceiver>();
        }
        objs = new GameObject[200];
        var coord = receiver.GetCoord();
        for (int i = 0; i < 408 / 3; i++)
        {
            var Position = Helpers.GetReceivedPosition(coord, i);
            
            var GameObj = new GameObject();
            GameObj.name = "GameObj" + i;
            var filter = GameObj.AddComponent<MeshFilter>();
            var renderer = GameObj.AddComponent<MeshRenderer>();

            filter.mesh = mesh;
            renderer.material = material;
            GameObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            objs[i] = GameObj;
        }
    }

    // Update is called once per frame
    void Update()
    {
        var coord = receiver.GetCoord();
        
        for (int i = 0; i < 408 / 3; i++)
        {
            var Position = Helpers.GetReceivedPosition(coord, i);
            
            objs[i].transform.position = Position;
        }
    }
}
