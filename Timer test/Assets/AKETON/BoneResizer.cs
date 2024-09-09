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
        
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);

            
            child.localScale = new Vector3(1.0f / child.lossyScale.x, 1.0f / child.lossyScale.y, 1.0f / child.lossyScale.z);
        }
        
        void RecursiveSolve(Transform t, int depth)
        {
            if (depth == 0)
            {
                for (int i = 0; i < t.childCount; i++)
                {
                    var child = t.GetChild(i);

                    var newScale = GetCounterScale();

                    child.localScale = newScale;
                }
                
                return;
            }
            
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                child.localScale = Vector3.one;
                RecursiveSolve(child, depth-1);
            }
        }
        
        RecursiveSolve(transform, depth);
    }

    Vector3 GetCounterScale()
    {
        return new Vector3(1.0f / scale.x, 1.0f / scale.y, 1.0f / scale.z);
    }
}
