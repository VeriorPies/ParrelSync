using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ParrelSync.Threading.Tasks
{
    internal static class TaskExtensions
    {
        internal static async void Forget(this Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }

        internal static async Task<bool> DetectDisconnectAsync(this Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException)
            {
                return true;
            }

            return false;
        }
    }
}
