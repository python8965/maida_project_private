using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = System.Object;


public class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    private static List<Dictionary<string, object>> Read(string file)
    {
        var list = new List<Dictionary<string, object>>();
        TextAsset data = Resources.Load (file) as TextAsset;

        var lines = Regex.Split (data.text, LINE_SPLIT_RE);

        if(lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);
        for(var i=1; i < lines.Length; i++) {

            var values = Regex.Split(lines[i], SPLIT_RE);
            if(values.Length == 0 ||values[0] == "") continue;

            var entry = new Dictionary<string, object>();
            for(var j=0; j < header.Length && j < values.Length; j++ ) {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                object finalvalue = value;
                int n;
                float f;
                if(int.TryParse(value, out n)) {
                    finalvalue = n;
                } else if (float.TryParse(value, out f)) {
                    finalvalue = f;
                }
                entry[header[j]] = finalvalue;
            }
            list.Add (entry);
        }
        return list;
    }

    public static bool isMirrored;
    
    public static Dictionary<string, List<Dictionary<string, object>>> cachedCsv = new();

    private static List<Dictionary<string, object>> GetCachedCsvByKey(string key)
    {
        if (!cachedCsv.ContainsKey(key))
        {
            cachedCsv.Add(key, Read(key));
        }
                
        return cachedCsv[key];
    }

    public static List<Dictionary<string, object>> jointCsv {
        get
        {
            if (isMirrored)
            {
                
                return GetCachedCsvByKey("joints_reverse");
            }
            else
            {
                return GetCachedCsvByKey("joints");
            }
            
            
        }
        
    }
    
    public static List<Dictionary<string, object>> boneCsv => GetCachedCsvByKey("bones");
}

public static class Helpers
{
    public const int CoordSize = 408;
    public const int CoordVectorSize = 408 / 3;
    public static Vector3 PointB = new Vector3(0, -20, 0);
    public static Transform RecursiveFindChild(Transform parent, string childName) 
        //비효율적인 함수라 추후 최적화 가능 , 재귀적으로 이름으로 자식 오브젝트를 찾습니다.
    {
        foreach (Transform child in parent)
        {
            if(child.name == childName)
            {
                return child;
            }
            else
            {
                Transform found = RecursiveFindChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }

    public static Transform GetBone(Transform transform, Avatar avatar, string boneName)
    {
        var findbone = avatar.humanDescription.human.ToList()
            .Find(bone => bone.humanName == boneName);

        //HumanBodyBones.LeftUpperLeg;

        var bone = RecursiveFindChild(transform, findbone.boneName);

        return bone;
    }
    
    public static Transform FindIKRig(Transform parent, string childName)
    { // 바나나 오브젝트 내의 IK 릭을 찾는 함수
        var x = parent.Find($"IKRig/{childName}");
        return x;
    }
    
    
    public static Vector3 GetReceivedPosition(Vector3[] coord, int Point) // 원래 있던 함수입니다.
    {
        return coord[Point];
    }
    
    public static void SetValue(object obj, string propertyPath, object value)
    {
        string[] properties = propertyPath.Split('.');
        SetValueRecursive(obj, properties, 0, value);
    }

    private static void SetValueRecursive(object obj, string[] fields, int index, object value)
    {
        if (index < fields.Length - 1)
        {
            FieldInfo fieldInfo = obj.GetType().GetField(fields[index]);
            PropertyInfo propertyInfo = obj.GetType().GetProperty(fields[index]);
            object childObj;
            
            if (fieldInfo != null)
            {
                childObj = fieldInfo.GetValue(obj);
                
            }
            else if (propertyInfo != null)
            {
                childObj = propertyInfo.GetValue(obj);
                
            }
            else
            {
                throw new ArgumentException($"Property '{fields[index]}' not found on object of type '{obj.GetType()}'");
            }
            
            SetValueRecursive(childObj, fields, index + 1, value);
        }
        else
        {
            FieldInfo fieldInfo = obj.GetType().GetField(fields[index]);
            PropertyInfo propertyInfo = obj.GetType().GetProperty(fields[index]);
            
            
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
            else if (propertyInfo != null)
            {
                propertyInfo.SetValue(obj, value);
            }
            else
            {
                throw new ArgumentException($"Property '{fields[index]}' not found on object of type '{obj.GetType()}'");
            }
        }
    }
}

[System.Serializable]
        
        
          
              public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
        
        
          
              {
        
        
          
                  [SerializeField]
        
        
          
                  private List<TKey> keys = new List<TKey>();
        
        
          
          

        
        
          
                  [SerializeField]
        
        
          
                  private List<TValue> values = new List<TValue>();
        
        
          
          

        
        
          
                  // save the dictionary to lists
        
        
          
                  public void OnBeforeSerialize()
        
        
          
                  {
        
        
          
                      keys.Clear();
        
        
          
                      values.Clear();
        
        
          
                      foreach (KeyValuePair<TKey, TValue> pair in this)
        
        
          
                      {
        
        
          
                          keys.Add(pair.Key);
        
        
          
                          values.Add(pair.Value);
        
        
          
                      }
        
        
          
                  }
        
        
          
          

        
        
          
                  // load dictionary from lists
        
        
          
                  public void OnAfterDeserialize()
        
        
          
                  {
        
        
          
                      this.Clear();
        
        
          
          

        
        
          
                      if (keys.Count != values.Count)
        
        
          
                          throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));
        
        
          
          

        
        
          
                      for (int i = 0; i < keys.Count; i++)
        
        
          
                          this.Add(keys[i], values[i]);
        
        
          
                  }
        
        
          
              }