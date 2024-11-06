using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    public IReceiver receiver;
    public Mesh boneMesh;
    
    private GameObject[] Meshs ;
    void Start()
    {
        Meshs = new GameObject[CSVReader.boneCsv.Count];

        for (int i = 0; i < CSVReader.boneCsv.Count; i++)
        {
            var row = CSVReader.boneCsv[i];
            var firstBoneID  = (int)row["FirstBoneID"];
            var lastBoneID  = (int)row["LastBoneID"];
            
            GameObject go = new GameObject();
            go.AddComponent<MeshFilter>();
            go.GetComponent<MeshFilter>().mesh = boneMesh;
            go.AddComponent<MeshRenderer>();
            go.GetComponent<MeshRenderer>().material.color = Color.white;
            go.name = "Mesh" + firstBoneID + lastBoneID;
            go.transform.parent = this.transform;
            Meshs[i] = go;
        }
    }

    // Update is called once per frame
    void Update()
    {
        var coord = receiver.GetCoord();
        
        for (int i = 0; i < CSVReader.boneCsv.Count; i++)
        {
            var row = CSVReader.boneCsv[i];
            
            var firstBoneID  = (int)row["FirstBoneID"];
            var lastBoneID  = (int)row["LastBoneID"];
            
            var firstBonePos = coord[firstBoneID];
            var lastBonePos = coord[lastBoneID];
            
            var length = Vector3.Distance(firstBonePos, lastBonePos);
            
            Debug.DrawLine(firstBonePos, lastBonePos, Color.cyan);



            Meshs[i].transform.position = (firstBonePos + lastBonePos) / 2.0f;
            Meshs[i].transform.rotation = Quaternion.FromToRotation(Vector3.up, lastBonePos - firstBonePos);
            float factor = 0.1f;
            
            Meshs[i].transform.localScale = new Vector3(1 * factor, (lastBonePos - firstBonePos).magnitude / 2, 1 * factor);
        }
    }
}
