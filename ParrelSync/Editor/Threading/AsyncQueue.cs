using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace ParrelSync.Threading
{
    internal class AsyncQueue<T>
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
            _semaphore.Release();
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (_queue.TryDequeue(out T item))
            {
                return item;
            }

            throw new InvalidOperationException("Something went wrong, there is nothing to dequeue");
        }
    }
}