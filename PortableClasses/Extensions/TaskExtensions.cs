using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PortableClasses.Extensions
{
    public static class TaskExtensions
    {
        public static Task<T> TryTo<T>(this Task<T> task, int maxTries = 10)
        {
            T returnValue = default; bool isCompleted = false; int count = 0;

            while (!isCompleted && count++ < maxTries)
            {
                try
                {
                    returnValue = task.Result;
                    isCompleted = true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    ShowDebugError(ex.Message);
                    if (count >= maxTries) throw ex;
                    Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
                }
                catch (Exception) { throw; }
            }

            return Task.FromResult(returnValue);
        }

        public static Task TryTo(this Task task, int maxTries = 10)
        {
            bool isCompleted = false; int count = 0;

            while (!isCompleted && count++ < maxTries)
            {
                try
                {
                    task.Wait();
                    isCompleted = true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    ShowDebugError(ex.Message);
                    if (count >= maxTries) throw ex;
                    Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
                }
                catch (Exception) { throw; }
            }

            return Task.CompletedTask;
        }

        [Conditional("DEBUG")]
        private static void ShowDebugError(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
