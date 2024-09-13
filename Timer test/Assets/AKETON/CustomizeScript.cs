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
        if (isChanged)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];

                for (int j = 0; j < bone.bones.Length; j++)
                {
                    bone.bones[j].localScale = new Vector3(1.0f, (float)scale[i], 1.0f);
                }
            }
            
            isChanged = false;
        }
    }
}
