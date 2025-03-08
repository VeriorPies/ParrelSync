using System.Threading;

namespace ParrelSync.Ipc
{
    internal static class AssemblyReloadWaitHandler
    {
        private static int _targetCount;
        private static int _currentCount;
        private static ManualResetEventSlim manualReset = new ManualResetEventSlim(false);

        public static void SetCountAndWait(int count)
        {
            _targetCount = count;
            manualReset.Wait(3000);
        }

        public static void IncrementCurrentCount()
        {
            int incrementedCount = Interlocked.Increment(ref _currentCount);
            if (incrementedCount == _targetCount)
            {
                manualReset.Set();
            }
        }
    }
}
