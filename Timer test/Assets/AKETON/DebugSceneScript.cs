using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSceneScript : MonoBehaviour
{
    Receiver receiver;
    GameObject[] objs;

    public Mesh mesh;
    public Material material;

    // Start is called before the first frame update
    void Start()
    {
        receiver = gameObject.AddComponent<Receiver>();
        objs = new GameObject[200];
        for (int i = 0; i < 408 / 3; i++)
        {
            var Position = Helpers.GetReceivedPosition(receiver.coord, i);
            
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
        for (int i = 0; i < 408 / 3; i++)
        {
            var Position = Helpers.GetReceivedPosition(receiver.coord, i);
            
            objs[i].transform.position = Position;
        }
    }
}
