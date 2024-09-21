using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CustomizeBones
{
    public string name;
    public Transform[] bones;
}

public class CustomizeScript : MonoBehaviour
{
    public CustomizeBones[] bones;

    public double[] scale;

    private bool isChanged;

    private void Reset()
    {
        bones = new[]
        {
            new CustomizeBones
            {
                name = "Spine",
            },
            new CustomizeBones
            {
                name = "LeftArm",
            },
            new CustomizeBones
            {
                name = "RightArm",
            },
            new CustomizeBones
            {
                name = "LeftLeg",
            },
            new CustomizeBones
            {
                name = "RightLeg",
            }
        };
        
        scale = new double[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            scale[i] = 1.0;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        MarkDirty();
    }
    
    [ContextMenu("MarkDirty")]
    void MarkDirty()
    {
        isChanged = true;
    }

    void SetScale(int index, double scale)
    {
        if (index >= bones.Length)
        {
            return;
        }
        
        this.scale[index] = scale;
        
        isChanged = true;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < bones.Length; i++)
        {
            var bone = bones[i];
            
            var scalev = (float)scale[i];
            var scaleav = (scalev + 1.0f) / 2.0f;

            for (int j = 0; j < bone.bones.Length; j++)
            {
                Debug.Log(bone.bones[j].name);
                bone.bones[j].localScale = new Vector3( scaleav, scalev,scaleav);
            }
        }
    }
}
