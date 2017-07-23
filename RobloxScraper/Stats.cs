using System;
using System.Collections.Generic;
using System.Text;

namespace RobloxScraper
{
    public class Stats
    {
        public long Count { get; set; } = 0;
        public long TimeTaken { get; set; } = 0;

        public float Average
        { get
            {
                return (float)TimeTaken / (float)Count;
            }
        }
    }
}
