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
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
                }
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
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
                }
            }

            return Task.CompletedTask;
        }
    }
}
