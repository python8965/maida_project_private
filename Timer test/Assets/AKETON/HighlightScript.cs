using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public enum TargetMuscle
{
    quadriceps, 
    hamstrings, 
    glutes
}

static class TargetMuscleMethods
{

    public static Color GetColor(this TargetMuscle s1)
    {
        return s1 switch
        {
            TargetMuscle.quadriceps => Color.red,
            TargetMuscle.glutes => Color.green,
            TargetMuscle.hamstrings => Color.blue,
            _ => Color.black
        };
    }
}


public class HighlightScript : MonoBehaviour
{
    private static readonly int CurrentHighlightTarget = Shader.PropertyToID("_CurrentHighlightTarget");
    Material material;
    
    public TargetMuscle targetMuscle = TargetMuscle.quadriceps;
    
    // Start is called before the first frame update
    void Start()
    {
        material = GetComponentInChildren<Renderer>().sharedMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        material.SetColor(CurrentHighlightTarget, targetMuscle.GetColor());
    }
}
