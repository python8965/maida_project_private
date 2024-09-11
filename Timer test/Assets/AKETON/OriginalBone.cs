using System.Collections;
using System.Collections.Generic;
using OnlyNew.BodyProportions;
using UnityEngine;

public class OriginalBone : MonoBehaviour
{
    public Transform[] bones;
    public Vector3[] originalPositions;
    public Quaternion[] originalRotations;
    // Start is called before the first frame update
    void Start()
    {
        var reader = GetComponent<ReadTransform>();
        if (reader == null) return;
        if (reader.bonePair == null) return;

        bones = new Transform[reader.bonePair.Length];
        originalPositions = new Vector3[reader.bonePair.Length];
        originalRotations = new Quaternion[reader.bonePair.Length];
        
        for(int i = 0; i < reader.bonePair.Length; i++)
        {
            bones[i] = reader.bonePair[i].coupledBone;
            
            originalPositions[i] = bones[i].position;
            originalRotations[i] = bones[i].rotation;
        }
    }

    void LateUpdate()
    {
        if (bones == null)
            return;
        
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                bones[i].position = originalPositions[i];
                bones[i].rotation = originalRotations[i];
            }
        }
    }
}
