using UnityEngine;
namespace OnlyNew.BodyProportions
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(20001)]
    [ExecuteAlways]
    public class WriteTransform : MonoBehaviour
    {
        [Tooltip("If true, copy bonePair from ReadTransform")]
        public bool SyncBonePair = false;
        [Tooltip("This array stores the relationships between the real and virtual skeletons.")]
        [SerializeField]
        public TransformPair[] bonePair;
        void OnEnable()
        {
            Sync();
        }

        void OnValidate()
        {
            Sync();
        }

        void Sync()
        {
            if (SyncBonePair)
            {
                var reader = GetComponent<ReadTransform>();
                if (reader != null)
                {
                    if (reader.bonePair != null)
                    {
                        bonePair = reader.bonePair;
                    }
                }
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (bonePair == null)
                return;
            WriteDirectly();
        }

        private void WriteDirectly()
        {
            foreach (var pair in bonePair)
            {
                if (pair.coupledBone != null && pair.decoupledBone != null)
                {
                    pair.decoupledBone.position = pair.coupledBone.position;
                    pair.decoupledBone.rotation = pair.coupledBone.rotation;
                }
            }
        }
        //[Range(0f, 1f)]
        //public float weight = 1.0f;
        //private void WriteWeightly()
        //{
        //    if (weight >= 1f)
        //    {
        //        WriteDirectly();
        //    }
        //    else
        //    {
        //        foreach (var pair in bonePair)
        //        {
        //            if (pair.coupledBone != null && pair.decoupledBone != null)
        //            {
        //                pair.decoupledBone.position = Vector3.Lerp(pair.decoupledBone.position, pair.coupledBone.position, weight);
        //                pair.decoupledBone.rotation = Quaternion.Slerp(pair.decoupledBone.rotation, pair.coupledBone.rotation, weight); ;
        //            }
        //        }
        //    }
        //}

    }
}
