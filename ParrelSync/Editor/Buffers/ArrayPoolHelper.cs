
namespace ParrelSync.Buffers
{
    internal static class ArrayPoolHelper
    {
#if !UNITY_2021_2_OR_NEWER
        private static readonly ByteArrayPool s_byteArrayPool = new ByteArrayPool();
#endif

        public static byte[] Rent(int minimumLength)
        {
#if !UNITY_2021_2_OR_NEWER
            return s_byteArrayPool.Rent(minimumLength);
#else
            return System.Buffers.ArrayPool<byte>.Shared.Rent(minimumLength);
#endif
        }

        public static void Return(byte[] array)
        {
#if !UNITY_2021_2_OR_NEWER
            s_byteArrayPool.Return(array);
#else
            System.Buffers.ArrayPool<byte>.Shared.Return(array);
#endif
        }
    }
}