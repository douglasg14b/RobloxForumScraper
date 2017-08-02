using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace RobloxScraper.Extensions
{
    public static class StopwatchExtensions
    {
        public static async Task TimeAsync(this Stopwatch stopwatch, Func<Task> action, Func<long, Task> callback)
        {
            stopwatch.Restart();
            await action();
            await callback(stopwatch.ElapsedMilliseconds);      
        }

        public static async Task<T> TimeAsync<T>(this Stopwatch stopwatch, Func<Task<T>> action, Func<long, Task> callback)
        {
            stopwatch.Restart();
            T result = await action();
            await callback(stopwatch.ElapsedMilliseconds);
            return result;
        }

        public static void Time(this Stopwatch stopwatch, Action action, Action<long> callback)
        {
            stopwatch.Restart();
            action();
            callback(stopwatch.ElapsedMilliseconds);
        }

        public static T Time<T>(this Stopwatch stopwatch, Func<T> action, Action<long> callback)
        {
            stopwatch.Restart();
            T result = action();
            callback(stopwatch.ElapsedMilliseconds);
            return result;
        }
    }
}
