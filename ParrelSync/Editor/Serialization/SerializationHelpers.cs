using System;

namespace ParrelSync.Serialization
{
    internal static class SerializationHelpers
    {
        public static void WriteInt(int value, byte[] bytes, ref int position)
        {
            WriteBlittabe(value, bytes, ref position);
        }

        public static void WriteShort(short value, byte[] bytes, ref int position)
        {
            WriteBlittabe(value, bytes, ref position);
        }

        public static void WriteByte(byte value, byte[] bytes, ref int position)
        {
            WriteBlittabe(value, bytes, ref position);
        }

        public static void WriteBool(bool value, byte[] bytes, ref int position)
        {
            WriteBlittabe(value, bytes, ref position);
        }

        private static unsafe void WriteBlittabe<T>(T blittableStruct, byte[] byteArray, ref int startPosition) where T : unmanaged
        {
            int size = sizeof(T);
            if (startPosition < 0 || startPosition + size > byteArray.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startPosition), "The start position is out of range or the byte array is too small.");
            }

            fixed (byte* ptr = &byteArray[startPosition])
            {
                Buffer.MemoryCopy(&blittableStruct, ptr, size, size);
            }

            startPosition += size;
        }

        public static bool TryReadByte(byte[] buffer, int filledCount, ref int position, out byte value)
        {
            return TryReadBlittable(buffer, filledCount, ref position, out value);
        }

        public static bool TryReadBool(byte[] buffer, int filledCount, ref int position, out bool value)
        {
            return TryReadBlittable(buffer, filledCount, ref position, out value);
        }

        public static bool TryReadInt(byte[] buffer, int filledCount, ref int position, out int value)
        {
            return TryReadBlittable(buffer, filledCount, ref position, out value);
        }

        public static bool TryReadShort(byte[] buffer, int filledCount, ref int position, out short value)
        {
            return TryReadBlittable(buffer, filledCount, ref position, out value);
        }

        public static unsafe bool TryReadBlittable<T>(byte[] buffer, int filledCount, ref int position, out T value) where T : unmanaged
        {
            if (position + sizeof(T) > filledCount)
            {
                value = default;
                return false;
            }

            fixed (byte* ptr = &buffer[position])
            {
                value = *(T*)ptr;
            }

            position += sizeof(T);

            return true;
        }
    }
}