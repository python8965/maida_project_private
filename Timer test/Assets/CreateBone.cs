using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bone01 : MonoBehaviour
{
    public GameObject start;
    public GameObject end;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 startPoint = start.transform.position;
        Vector3 endPoint = end.transform.position;
        transform.localScale = new Vector3(1, (endPoint - startPoint).magnitude / 2, 1);
        transform.rotation = Quaternion.FromToRotation(Vector3.up, endPoint - startPoint);
        transform.position = (endPoint + startPoint) / 2;
    }
}
