using System;
using System.Collections.Generic;
using System.Text;

namespace RobloxScraper
{
    public class Stat
    {
        public Stat() { }
        public Stat(long count, long timetaken)
        {
            Count = count;
            Time = timetaken;
        }

        public long Count { get; set; } = 0;
        public long Time { get; set; } = 0;

        public float Average
        { get
            {
                if(Count > 0)
                {
                    return (float)Time / (float)Count;
                }
                return float.NaN;
            }
        }
    }
}
