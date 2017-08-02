using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RobloxScraper.Telemetry
{
    public class Stat
    {
        public Stat() { }
        public Stat(long count, long timetaken)
        {
            this.count = count;
            time = timetaken;
        }

        public long count = 0;
        public long time = 0;

        public async void Incriment(long time)
        {
            Interlocked.Increment(ref count);
            Interlocked.Add(ref this.time, time);
        }

        public float AverageTime
        {
            get
            {
                if (count > 0)
                {
                    return (float)time / (float)count;
                }
                return float.NaN;
            }
        }

        public static Stat operator +(Stat s1, Stat s2)
        {
            return new Stat(s1.count + s2.count, s1.time + s2.time);
        }

    }
}
