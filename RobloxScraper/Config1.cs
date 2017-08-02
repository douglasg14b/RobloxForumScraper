using System;
using System.Collections.Generic;
using System.Text;

namespace RobloxScraper
{
    public class Config1
    {
        public Config1()
        {
            var ini = new IniFile();
            var maxDownloaders = ini.Read("MaxDownloaders");
            var maxProcessors = ini.Read("MaxProcessors");
            var startThread = ini.Read("StartThread");
            var maxThread = ini.Read("MaxThread");
            var threadsBeforeWrite = ini.Read("ThreadsBeforeWrite");

            max_downloaders = TryGetValue(maxDownloaders, max_downloaders);
            max_processors = TryGetValue(maxProcessors, max_processors);
            start_thread = TryGetValue(startThread, start_thread);
            max_thread = TryGetValue(maxThread, max_thread);
            threads_before_write = TryGetValue(threadsBeforeWrite, threads_before_write);
        }

        [ThreadStatic]
        public int max_downloaders = 1;
        [ThreadStatic]
        public int max_processors = 1;
        [ThreadStatic]
        public int start_thread = 0;
        [ThreadStatic]
        public int max_thread = 25000000;
        [ThreadStatic]
        public int threads_before_write = 5000;

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
