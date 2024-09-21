using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeakRotation : MonoBehaviour
{
    public Transform LeakTo;

    public float weight;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        LeakTo.Rotate(Vector3.up,weight* 90.0f  );
    }
}
