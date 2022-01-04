using System;
using System.Threading;
using System.Threading.Tasks;

namespace OzricEngine
{
    /// <summary>
    /// An object that may appear in the future
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncObject<T>: IDisposable
    {
        private readonly SemaphoreSlim waiter = new SemaphoreSlim(0, 1);
        private T obj;

        public void Set(T t)
        {
            lock (this)
            {
                obj = t;
            }
            
            waiter.Release();
        }

        public async Task<T> Get(int millisecondsTimeout)
        {
            lock (this)
            {
                if (obj != null)
                    return obj;
            }

            await waiter.WaitAsync(millisecondsTimeout);

            lock (this)
            {
                return obj;
            }
        }

        public void Dispose()
        {
            waiter?.Dispose();
        }
    }
}