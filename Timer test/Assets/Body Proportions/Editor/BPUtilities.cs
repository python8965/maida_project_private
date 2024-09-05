using System.IO;
using UnityEditor;
using UnityEngine;
namespace OnlyNew.BodyProportions.Utilities
{
    public static class AssetCreator
    {
        public static string GenerateUniqueFilePath(string path, string filename, string extension)
        {
            int count = 0;
            string fullFilePath;

            do
            {
                if (count == 0)
                {
                    fullFilePath = Path.Combine(path, $"{filename}.{extension}");
                }
                else
                {
                    fullFilePath = Path.Combine(path, $"{filename} ({count}).{extension}");
                }

                count++;
                if (count > 1000)
                {
                    Debug.LogError("too many file,please check for errors.");
                    break;
                }

            } while (File.Exists(fullFilePath));

            return fullFilePath;
        }
        public static void CreateAssetWithCheck<T>(string dirPath, string filename, T asset) where T : Object
        {
            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }
            if (!System.IO.Directory.Exists(dirPath))
            {
                Debug.LogError(dirPath + " does't exist. Failed to create new folder, please create it manually.");
            }
            else
            {
                var filepath = GenerateUniqueFilePath(dirPath, filename, "asset");
                AssetDatabase.CreateAsset(asset, filepath);
                EditorGUIUtility.PingObject(asset);
            }
        }
    }
}