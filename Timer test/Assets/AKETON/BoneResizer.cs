using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneResizer : MonoBehaviour
{
    public Vector3 scale = Vector3.one;

    public int depth = 1;
    
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
        var actualscale = scale;
        
        this.transform.localScale = scale;
        
        void RecursiveSolve(Transform t, int depth)
        {
            if (depth == 0)
            {
                return;
            }
            
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                
                RecursiveSolve(child, depth--);
                
                var childScale = child.localScale;

                var newScale = GetCounterScale();

                child.localScale = newScale;
            }
        }
        
        RecursiveSolve(transform, depth);
    }

    Vector3 GetCounterScale()
    {
        return new Vector3(1.0f / scale.x, 1.0f / scale.y, 1.0f / scale.z);
    }
}
