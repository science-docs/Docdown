using System;
using System.Threading;
using System.Threading.Tasks;

namespace PdfiumViewer.Wpf.Util
{
    internal static class TaskUtility
    {
        private static SemaphoreSlim semaphore;

        static TaskUtility()
        {
            semaphore = new SemaphoreSlim(1);
        }

        public static async Task<T> Enqueue<T>(Func<T> taskFunction)
        {
            await semaphore.WaitAsync();
            try
            {
                return await Task.Run(taskFunction);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public static async Task Enqueue(Action action)
        {
            await semaphore.WaitAsync();
            try
            {
                await Task.Run(action);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
