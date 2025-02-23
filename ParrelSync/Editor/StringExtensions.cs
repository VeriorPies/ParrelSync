
namespace ParrelSync
{
    internal static class StringExtensions
    {
#if !UNITY_2021_2_OR_NEWER
        internal static string[] Split(this string text, string separator)
        {
            string[] separatorArr = new string[] { separator };
            return text.Split(separatorArr, System.StringSplitOptions.None);
        }
#endif

        internal static int GetStableHashCode(this string text)
        {
            unchecked
            {
                uint hash = 0x811c9dc5;
                uint prime = 0x1000193;

                for (int i = 0; i < text.Length; ++i)
                {
                    byte value = (byte)text[i];
                    hash = hash ^ value;
                    hash *= prime;
                }

                return (int)hash;
            }
        }
    }
}