using System;
using System.Collections.Generic;
using System.Text;

namespace RobloxScraper
{
    public class Config
    {
        public Config()
        {
            var ini = new IniFile();
            var maxDownloaders = ini.Read("MaxDownloaders");
            var maxProcessors = ini.Read("MaxProcessors");
            var startThread = ini.Read("StartThread");
            var maxThread = ini.Read("MaxThread");
            var threadsBeforeWrite = ini.Read("ThreadsBeforeWrite");

            MaxDownloaders = TryGetValue(maxDownloaders, MaxDownloaders);
            MaxPocessors = TryGetValue(maxProcessors, MaxPocessors);
            StartThread = TryGetValue(startThread, StartThread);
            MaxThread = TryGetValue(maxThread, MaxThread);
            ThreadsBeforeWrite = TryGetValue(threadsBeforeWrite, ThreadsBeforeWrite);
        }

        public int MaxDownloaders { get; set; } = 1;
        public int MaxPocessors { get; set; } = 1;
        public int StartThread { get; set; } = 0;
        public int MaxThread { get; set; } = 25000000;
        public int ThreadsBeforeWrite { get; set; } = 5000;

        private int TryGetValue(string value, int def)
        {
            int output = 0;
            if(int.TryParse(value, out output))
            {
                return output;
            }
            return def;
        }
    }
}
