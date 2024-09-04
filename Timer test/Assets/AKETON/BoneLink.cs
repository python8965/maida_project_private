using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneLink : MonoBehaviour
{
    public GameObject bone;
    // Start is called before the first frame update
    void Start()
    {
        if (bone == null)
        {
            Debug.LogError("bone is null");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
