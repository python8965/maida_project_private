using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneResizer : MonoBehaviour
{
    public Vector3 scale = Vector3.one;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        this.transform.localScale = scale;
        
        void ChildRecursive(Vector3 _scale)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                
                var childScale = transform.GetChild(i).localScale;
                var newx = 1.0f / _scale.x;
                var newy = 1.0f  / _scale.y;
                var newz = 1.0f  / _scale.z;
            
                var newScale = new Vector3(newx, newy, newz);
            
                transform.GetChild(i).localScale = newScale;
            }
        }

        ChildRecursive(scale);
    }
}
