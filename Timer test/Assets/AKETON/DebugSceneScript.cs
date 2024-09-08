using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class DebugSceneScript : MonoBehaviour
{
    
    
    public IReceiver receiver;
    GameObject[] objs;

    private Mesh mesh;
    private Material material;

    public Color debugColor = Color.white;

    private void Awake()
    {
        mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        material = new Material(Shader.Find("Standard"))
        {
            color = debugColor
        };
        objs = new GameObject[408 / 3];
    }

    private void OnDrawGizmos()
    {
        
        
        if (objs != null)
        {
            if (objs.Length == 0)
            {
                return;
            }
            var style = new GUIStyle();
            style.fontSize = 15;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = debugColor;
            
            
            for (int i = 0; i < 408 / 3; i++)
            {
                Handles.Label(objs[i].transform.position  ,i.ToString(), style);
            }
        }
        
        Handles.Label(Vector3.zero, "Test");
    }

    // Start is called before the first frame update
    void Start()
    {
        var coord = receiver.GetCoord();
        for (int i = 0; i < 408 / 3; i++)
        {
            var Position = Helpers.GetReceivedPosition(coord, i);
            
            var GameObj = new GameObject
            {
                name = "GameObj" + i,
                transform =
                {
                    localScale = new Vector3(0.1f, 0.1f, 0.1f),
                    position = Position,
                    parent = transform
                }
            };

            var filter = GameObj.AddComponent<MeshFilter>();
            var renderer = GameObj.AddComponent<MeshRenderer>();

            filter.mesh = mesh;
            renderer.material = material;
            
            
            objs[i] = GameObj;
        }
    }

    // Update is called once per frame
    void Update()
    {
        material.SetColor(0 , debugColor);
        
        if (receiver == null)
        {
            Debug.Log("Receiver is null" + gameObject.name);
        }
        
        var coord = receiver.GetCoord();
        
        for (int i = 0; i < 408 / 3; i++)
        {
            var Position = Helpers.GetReceivedPosition(coord, i);
            
            objs[i].transform.position = Position;
        }
    }
}
