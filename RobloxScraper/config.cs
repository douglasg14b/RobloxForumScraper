using System;
using System.Collections.Generic;
using System.Text;

namespace RobloxScraper
{
    public static class Config
    {
        [ThreadStatic]
        public static bool initilized;
        /* Configurable settings */

        [ThreadStatic]
        public static int m_max_downloaders;
        [ThreadStatic]
        public static int m_max_processors;
        [ThreadStatic]
        public static int m_start_thread;
        [ThreadStatic]
        public static int m_max_thread;
        [ThreadStatic]
        public static int m_threads_before_write;
        [ThreadStatic]
        public static int m_logical_processors;

        public static int MaxDownloaders
        {
            get
            {
                if (!initilized)
                {
                    Initialize();
                }
                return m_max_downloaders;
            }
        }
        public static int MaxProcessors
        {
            get
            {
                if (!initilized)
                {
                    Initialize();
                }
                return m_max_processors;
            }
        }
        public static int StartThread
        {
            get
            {
                if (!initilized)
                {
                    Initialize();
                }
                return m_start_thread;
            }
        }
        public static int MaxThread
        {
            get
            {
                if (!initilized)
                {
                    Initialize();
                }
                return m_max_thread;
            }
        }
        public static int ThreadsBeforeWrite
        {
            get
            {
                if (!initilized)
                {
                    Initialize();
                }
                return m_threads_before_write;
            }
        }
        public static int LogicalProcessors
        {
            get
            {
                if (!initilized)
                {
                    Initialize();
                }
                return m_logical_processors;
            }
        }


        /* Unconfigurable settings */

        [ThreadStatic]
        public static int m_max_processing_queue;
        [ThreadStatic]
        public static float m_max_database_queue_modifier;

        public static int MaxProcessingQueue
        {
            get
            {
                if (!initilized)
                {
                    Initialize();
                }
                return m_max_processing_queue;
            }
        }
        public static float MaxDatabaseQueueModifier
        {
            get
            {
                if (!initilized)
                {
                    Initialize();
                }
                return m_max_database_queue_modifier;
            }
        }




        /* Internal settings */

        private static int max_downloaders;
        private static int max_processors;
        private static int start_thread;
        private static int max_thread;
        private static int threads_before_write;
        private static int logical_processors;

        private static int max_processing_queue = 100;
        private static float max_database_queue_modifier = 1.5f;

        /* Default settings */

        private static int default_max_downloaders = 1;
        private static int default_max_processors = 1;
        private static int default_start_thread = 0;
        private static int default_max_thread = 25000000;
        private static int default_threads_before_write = 5000;


        /// <summary>
        /// Used to initilize the original values all threads will use
        /// </summary>
        /// <param name="ini"></param>
        public static void Initialize(IniFile ini)
        {
            var maxDownloaders = ini.Read("MaxDownloaders");
            var maxProcessors = ini.Read("MaxProcessors");
            var startThread = ini.Read("StartThread");
            var maxThread = ini.Read("MaxThread");
            var threadsBeforeWrite = ini.Read("ThreadsBeforeWrite");

            max_downloaders = TryGetValue(maxDownloaders, default_max_downloaders);
            max_processors = TryGetValue(maxProcessors, default_max_processors);
            start_thread = TryGetValue(startThread, default_start_thread);
            max_thread = TryGetValue(maxThread, default_max_thread);
            threads_before_write = TryGetValue(threadsBeforeWrite, default_threads_before_write);
        }

        /// <summary>
        /// Used to initlize the calling threads values
        /// </summary>
        public static void Initialize()
        {
            m_max_downloaders = max_downloaders;
            m_max_processors = max_processors;
            m_start_thread = start_thread;
            m_max_thread = max_thread;
            m_threads_before_write = threads_before_write;

            m_max_processing_queue = max_processing_queue;
            m_max_database_queue_modifier = max_database_queue_modifier;

            initilized = true;
        }

        private static int TryGetValue(string value, int def)
        {
            int output = 0;
            if (int.TryParse(value, out output))
            {
                return output;
            }
            return def;
        }

    }
}
