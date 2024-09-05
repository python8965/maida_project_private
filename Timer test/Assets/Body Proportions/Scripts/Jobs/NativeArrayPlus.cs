#if !UNITY_WEBGL
using System;
using Unity.Collections;
namespace OnlyNew.BodyProportions
{
    public class NativeArrayPlus<T> : IDisposable where T : struct
    {
        //[NativeDisableParallelForRestriction]
        private NativeArray<T> nativeArray;
        private NativeList<SubArrayStartAndLength> subArrayStartAndLengths;
        private int m_capcity;
        private int m_usedCapacity;
        public int SubArrayCount
        {
            get { return subArrayStartAndLengths.Length; }
        }

        public NativeArrayPlus(int capcity, Allocator allocator)
        {
            m_capcity = capcity;
            m_usedCapacity = 0;
            nativeArray = new NativeArray<T>(m_capcity, allocator);
            subArrayStartAndLengths = new NativeList<SubArrayStartAndLength>(allocator);
        }
        public NativeArrayPlus(int capcity, Allocator allocator, NativeArrayOptions options)
        {
            m_capcity = capcity;
            m_usedCapacity = 0;
            nativeArray = new NativeArray<T>(m_capcity, allocator, options);
            subArrayStartAndLengths = new NativeList<SubArrayStartAndLength>(allocator);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns>SubArray Index</returns>
        public int AddSubArray(int length)
        {
            if (m_usedCapacity + length > m_capcity)
            {
                throw new IndexOutOfRangeException();
            }

            subArrayStartAndLengths.Add(new SubArrayStartAndLength { start = m_usedCapacity, length = length });
            m_usedCapacity += length;
            return subArrayStartAndLengths.Length - 1;
        }

        public NativeArray<T> GetTotalNativeArray()
        {
            return nativeArray;
        }

        public NativeArray<T> this[int index]
        {
            get
            {
                if (index >= subArrayStartAndLengths.Length)
                {
                    throw new IndexOutOfRangeException();
                }
                SubArrayStartAndLength startAndLength = subArrayStartAndLengths[index];
                return nativeArray.GetSubArray(startAndLength.start, startAndLength.length);
            }
        }

        public NativeArray<T> GetSubArray(int index)
        {
            if (index >= subArrayStartAndLengths.Length)
            {
                throw new IndexOutOfRangeException();
            }
            SubArrayStartAndLength startAndLength = subArrayStartAndLengths[index];
            return nativeArray.GetSubArray(startAndLength.start, startAndLength.length);
        }

        public void Dispose()
        {
            if (nativeArray.IsCreated)
                nativeArray.Dispose();
            if (subArrayStartAndLengths.IsCreated)
                subArrayStartAndLengths.Dispose();
        }

        public struct SubArrayStartAndLength
        {
            public int start;
            public int length;
        }
    }
}
#endif