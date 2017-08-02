using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RobloxScraper.Telemetry
{
    public static class TelemetryManager
    {
        static TelemetryManager()
        {
            startTime = DateTime.Now;

            overallDownloads = new Stat();
            overallProcessed = new Stat();

            overallDownloadedThreads = new Stat();
            overallDownloadedPages = new Stat();
            overallProcessedThreads = new Stat();
            overallProcessedPages = new Stat();
            databaseStats = new Stat();
            emptyThreads = new Stat();

            downloadedThreadStats = new ConcurrentDictionary<int, Stat>();
            downloadedPageStats = new ConcurrentDictionary<int, Stat>();
            processedThreadStats = new ConcurrentDictionary<int, Stat>();
            processedPageStats = new ConcurrentDictionary<int, Stat>();
        }

        public static DateTime startTime;

        public static Stat overallDownloads;
        public static Stat overallProcessed;

        public static Stat overallDownloadedThreads;
        public static Stat overallDownloadedPages;
        public static Stat overallProcessedThreads;
        public static Stat overallProcessedPages;

        public static Stat databaseStats;
        public static Stat emptyThreads;

        public static ConcurrentDictionary<int, Stat> downloadedThreadStats;
        public static ConcurrentDictionary<int, Stat> downloadedPageStats;

        public static ConcurrentDictionary<int, Stat> processedThreadStats;
        public static ConcurrentDictionary<int, Stat> processedPageStats;

        public static async void Incriment(TelemetryType type, int key, long time)
        {
            switch (type)
            {
                case TelemetryType.downloaded_threads:
                    IncrimentKey(downloadedThreadStats, overallDownloads, overallDownloadedThreads, key, time);
                    break;
                case TelemetryType.downloaded_pages:
                    IncrimentKey(downloadedPageStats, overallDownloads, overallDownloadedPages, key, time);
                    break;
                case TelemetryType.processed_threads:
                    IncrimentKey(processedThreadStats, overallProcessed, overallProcessedThreads, key, time);
                    break;
                case TelemetryType.processed_pages:
                    IncrimentKey(processedPageStats, overallProcessed, overallProcessedPages, key, time);
                    break;
            }
        }

        public static async void IncrimentDatabaseStats(long time)
        {
            databaseStats.Incriment(time);
        }

        public static async void IncrimentEmptyThreads()
        {
            emptyThreads.Incriment(0);
        }

        /// <summary>
        /// Incriments a specific stats in a dictionary by key
        /// </summary>
        /// <param name="dict">Dictionary containing stats for each key</param>
        /// <param name="overall">The overall stat used to record data for a type of telemetry</param>
        /// <param name="key"></param>
        /// <param name="time"></param>
        private static void IncrimentKey(ConcurrentDictionary<int, Stat> dict, Stat overall1, Stat overall2, int key, long time)
        {
            ExistOrCreate(dict, key);
            dict[key].Incriment(time);
            overall1.Incriment(time);
            overall2.Incriment(time);
        }

        /// <summary>
        /// Incriments a specific stats in a dictionary by key
        /// </summary>
        /// <param name="dict">Dictionary containing stats for each key</param>
        /// <param name="overall">The overall stat used to record data for a type of telemetry</param>
        /// <param name="key"></param>
        /// <param name="time"></param>
        private static void IncrimentKey(ConcurrentDictionary<int, Stat> dict, Stat overall, int key, long time)
        {
            ExistOrCreate(dict, key);
            dict[key].Incriment(time);
            overall.Incriment(time);
        }

        /// <summary>
        /// If the key does not exist, create it
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        private static void ExistOrCreate(ConcurrentDictionary<int, Stat> dict, int key)
        {
            if (!dict.ContainsKey(key))
            {
                dict.TryAdd(key, new Stat());
            }
        }

    }
}
