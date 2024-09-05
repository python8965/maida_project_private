#if !UNITY_WEBGL
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace OnlyNew.BodyProportions
{
    public class NativeArray2D<T> : IDisposable where T : struct
    {
        //[NativeDisableParallelForRestriction]
        private List<NativeArray<T>> nativeArrays;
        private int m_depth;
        private Allocator m_allocator;

        public NativeArray2D(int depth, Allocator allocator)
        {
            m_allocator = allocator;
            m_depth = depth;
            nativeArrays = new List<NativeArray<T>>(depth);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns>SubArray Index</returns>
        public void AddSubArray(int length)
        {
            nativeArrays.Add(new NativeArray<T>(length, m_allocator));
        }

        public List<NativeArray<T>> GetTotalNativeArray()
        {
            return nativeArrays;
        }

        public NativeArray<T> this[int index]
        {
            get
            {
                return nativeArrays[index];
            }
        }

        public NativeArray<T> GetSubArray(int index)
        {
            return nativeArrays[index];
        }

        public void Dispose()
        {
            if (nativeArrays != null)
            {
                foreach (var item in nativeArrays)
                {
                    if (item.IsCreated)
                        item.Dispose();
                }
            }

        }

    }
}
#endif