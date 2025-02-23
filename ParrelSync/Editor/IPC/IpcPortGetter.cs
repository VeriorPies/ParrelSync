
namespace ParrelSync.Ipc
{
    internal static class IpcPortGetter
    {
        public static int GetPort()
        {
            //return 7845;

            string filePath = ClonesManager.GetOriginalProjectPath();

            int hashCode = filePath.GetStableHashCode();
            return 58000 + (hashCode % 1000);
        }
    }
}
