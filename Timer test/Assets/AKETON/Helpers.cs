using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;


public class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    public static List<Dictionary<string, object>> Read(string file)
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
    
    public static List<Dictionary<string, object>> jointCsv = Read("joints");
}

public class Helpers
{
    
    public static Vector3 PointB = new Vector3(0, -20, 0);
    public const int CoordSize = 408;
    public const int CoordVectorSize = 408 / 3;
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

    public static Vector3 TransformReceivedPosition(Vector3[] Points, Vector3 V) //원래 있던 함수입니다.
    {
        
        
        
        var Position = (V - Points[10]) / 50 +PointB;
        
        
        Vector3 pointA = (Points[134] - Points[135]) / 50 + PointB;
        Vector3 pointC = new Vector3(0, 0, 0);
        
        // var Position = new Vector3(
        //     V.x / 50 - Points[30] / 50,
        //     V.y / 50 - Points[31] / 50 - 20,
        //     V.z / 50 - Points[32] / 50
        // );
            
        // //134 joint
        // Vector3 pointA = new Vector3( 
        //     Points[402] / 50 - Points[405] / 50,
        //     Points[403] / 50 - Points[406] / 50 - 20,
        //     Points[404] / 50 - Points[407] / 50
        // );
        //
        // Vector3 pointB = new Vector3(0, -20, 0);
        // Vector3 pointC = new Vector3(0, 0, 0);
        
        // 두 벡터 계산
        Vector3 vector1 = pointA - PointB;
        Vector3 vector2 = pointC - PointB;

        // 두 벡터 사이의 각도 계산
        float angle = Vector3.Angle(vector1, vector2);

        // 평면 위의 회전 축 계산 (pointA, pointB, pointC가 이루는 평면의 법선 벡터)
        Vector3 rotationAxis = Vector3.Cross(vector1, vector2).normalized;

        // 기준점에서 평면 위의 각도만큼 회전 이동
        Vector3 position = Position;
        Vector3 vector3 = Quaternion.AngleAxis(angle, rotationAxis) * (position - PointB);
        Position = PointB + vector3;
        Position = Quaternion.AngleAxis(angle * ((float)Math.PI / 180f), rotationAxis) * Position;
            
            
        //osition.RotateAround(pointB, rotationAxis, angle);
            
        return Position;
    }
    
    public static Vector3 GetReceivedPosition(Vector3[] coord, int Point) // 원래 있던 함수입니다.
    {
        
        return TransformReceivedPosition(coord, coord[Point]);
    }
    
    
}