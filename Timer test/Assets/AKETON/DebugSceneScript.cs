using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class DebugSceneScript : MonoBehaviour
{
    
    
    public IReceiver receiver;
    GameObject[] objs;

    [FormerlySerializedAs("isModify")] public bool isRaw;

    private Mesh mesh;
    private Material material;

    public float scaleConstant = 1.0f;
    public Vector3 debugOffset;

    public Color debugColor = Color.white;


    public bool isDrawText;

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
        #if UNITY_EDITOR
        if (isDrawText)
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
                    Handles.Label(objs[i].transform.position, i.ToString(), style);
                }
            }
        }
        #endif
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
                    localScale = new Vector3(0.1f, 0.1f, 0.1f) / scaleConstant,
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
            var Position = Vector3.zero;
            if (!isRaw)
            {
                Position = Helpers.GetReceivedPosition(coord, i);
            }
            else
            {
                Position = coord[i];
            }
            
            
            
            objs[i].transform.position = Position;
        }
        
        
        var dicts = CSVReader.boneCsv;
        foreach (var dict in dicts)
        {
            
            string boneType = (string)dict["BoneType"];
            string boneName = (string)dict["IKName"];
            int firstIndex = (int)dict["FirstBoneID"];
            int lastIndex = (int)dict["LastBoneID"];
            
            
            

            if (firstIndex > lastIndex)
            {
                Debug.LogError($"FirstIndex {firstIndex} is greater than LastIndex {lastIndex}");
            }

            if (boneType.Equals("Single"))
            {
                Vector3 startPoint = Helpers.GetReceivedPosition(coord,firstIndex);
                Vector3 endPoint = Helpers.GetReceivedPosition(coord,lastIndex);
                
                Debug.DrawLine(startPoint, endPoint, Color.green);
            } else if (boneType.Equals("Sequence"))
            {
                for (int i = firstIndex; i < lastIndex; i++)
                {
                    Vector3 startPoint = Helpers.GetReceivedPosition(coord, i);
                    Vector3 endPoint = Helpers.GetReceivedPosition(coord, i + 1);
                    
                    Debug.DrawLine(startPoint, endPoint, Color.red / 4  * i);
                }
                
            }
            
            
            
                
            
        }
    }
}
