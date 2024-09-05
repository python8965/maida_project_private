
using UnityEngine;

namespace OnlyNew.BodyProportions
{
    public static class VersionInfo
    {
        /// <summary>
        /// eg. 020600 = "2.6.0"
        /// </summary>
        public const int dataVersion = 020000;
#if UNITY_EDITOR
        public const string dataVersionKey = "BodyProportions.DataVersion";

        [UnityEditor.InitializeOnLoadMethod]

        public static void Upgrade()
        {
            UpgradeToVersion(dataVersion);
        }

        public static void UpgradeToVersion(int version)
        {
            var scalableBone = GameObject.FindAnyObjectByType<ScalableBone>();
            if (scalableBone != null)
            {
                if (scalableBone.currentVersion < dataVersion)
                {
                    if (UnityEditor.EditorUtility.DisplayDialog("Upgrade",
                            "To upgrade the Scalable Bone data, you must re-open the scene. If you have not yet saved the scene. Please click Cancel and reload the scene manually.",
                            "I will re-open the scene"))
                    {
                    }
                }
            }

        }
#endif
    }
}
