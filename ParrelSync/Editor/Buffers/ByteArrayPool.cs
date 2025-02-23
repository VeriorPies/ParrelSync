#if !UNITY_2021_2_OR_NEWER

using System;
using System.Collections.Concurrent;

namespace ParrelSync.Buffers
{
    /// <summary>
    /// Simple byte array pool implementation for versions of Unity not on .NET Standard 2.1.
    /// Inspired from .NET BCL
    /// </summary>
    internal class ByteArrayPool
    {
        private const int NumBuckets = 27; // Utilities.SelectBucketIndex(1024 * 1024 * 1024 + 1)

        private readonly ConcurrentBag<byte[]>[] _buckets;

        public ByteArrayPool()
        {
            _buckets = new ConcurrentBag<byte[]>[NumBuckets];

            for (int i = 0; i < _buckets.Length; i++)
            {
                _buckets[i] = new ConcurrentBag<byte[]>();
            }
        }

        public byte[] Rent(int minimumLength)
        {
            if (minimumLength == 0)
            {
                return Array.Empty<byte>();
            }

            int bucketIndex = SelectBucketIndex(minimumLength);

            var bucket = _buckets[bucketIndex];

            if (!bucket.TryTake(out byte[] result))
            {
                return new byte[GetMaxSizeForBucket(bucketIndex)];
            }

            return result;
        }

        public void Return(byte[] array)
        {
            if (array.Length == 0)
            {
                return;
            }

            // Determine with what bucket this array length is associated
            int bucketIndex = SelectBucketIndex(array.Length);

            int bucketSize = GetMaxSizeForBucket(bucketIndex);

            if (array.Length != bucketSize)
            {
                throw new InvalidOperationException("Returned buffer is not from the pool.");
            }

            var bucket = _buckets[bucketIndex];

            bucket.Add(array);
        }

        // taken from ArrayPool inplementation from .NET
        internal static int SelectBucketIndex(int bufferSize)
        {
            // Buffers are bucketed so that a request between 2^(n-1) + 1 and 2^n is given a buffer of 2^n
            // Bucket index is log2(bufferSize - 1) with the exception that buffers between 1 and 16 bytes
            // are combined, and the index is slid down by 3 to compensate.
            // Zero is a valid bufferSize, and it is assigned the highest bucket index so that zero-length
            // buffers are not retained by the pool. The pool will return the Array.Empty singleton for these.
            return Log2((uint)bufferSize - 1 | 15) - 3;
        }

        // taken from ArrayPool inplementation from .NET
        internal static int GetMaxSizeForBucket(int binIndex)
        {
            int maxSize = 16 << binIndex;
            return maxSize;
        }

        private static int Log2(uint number)
        {
            return (int)Math.Log(number, 2);
        }
    }
}
#endif