using RobloxScraper.DbModels;
using RobloxScraper.RobloxModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RobloxScraper
{
    public static class TaskRunner
    {
        static RobloxClient _client = new RobloxClient(new HttpClientHandler() { MaxConnectionsPerServer = 100 });

        static int max_threads = 10;
        static int max_workers = 5;
        static int max_downloaders = 10;
        static int max_downloads_per_thread = 200;
        static int max_forum_thread = 10000;



        static TaskRunner()
        {
            Queue = new ConcurrentQueue<int>();
            ForumThreads = new ConcurrentBag<Thread>();
            DownloadTasks = new ConcurrentDictionary<string, Task>();
            ProcessingTasks = new ConcurrentDictionary<string, Task>();

            UnparsedThreads = new ConcurrentQueue<KeyValuePair<int, string>>();

            downloadedStats = new ConcurrentDictionary<string, Stats>();
            processedStats = new ConcurrentDictionary<string, Stats>();
        }

        static bool alive = false;

        /***********************
          ======  Stats  =====
         ***********************/
        public static ConcurrentDictionary<string, Stats> downloadedStats;
        public static ConcurrentDictionary<string, Stats> processedStats;

        public static int threadsDownloaded = 0;
        public static int threadsProcessed = 0;
        public static int emptyThreads = 0;

        /***********************
          ======  State  =====
         ***********************/

        static ConcurrentQueue<KeyValuePair<int, string>> UnparsedThreads { get; set; }

        static ConcurrentBag<Thread> ForumThreads { get; set; }
        static ConcurrentQueue<int> Queue { get; set; }

        public static ConcurrentDictionary<string, Task> DownloadTasks { get; private set; }
        public static ConcurrentDictionary<string, Task> ProcessingTasks { get; private set; }

        public static void Start()
        {
            alive = true;
            FillQueue();
            InitDownloaders();
            InitWorkers();

            Task.Delay(60000).ContinueWith(t => 
            {
                Interrupt(1500);
            });

        }

        /**********************************************
          =============== Initilization ===============
         **********************************************/

        private static void InitDownloaders()
        {
            for (int i = 0; i < max_downloaders; i++)
            {
                Task task = new Task((object state) => { DoDownloadWork(state).GetAwaiter().GetResult(); }, $"download_task{i}", TaskCreationOptions.LongRunning);

                DownloadTasks.TryAdd($"download_task{i}", task);
                downloadedStats.TryAdd($"download_task{i}", new Stats());

                task.Start();
            }
        }

        private static void InitWorkers()
        {
            for(int i = 0; i < max_workers; i++)
            {
                Task task = new Task((object state) => { DoWork(state).GetAwaiter().GetResult(); }, $"worker_task{i}", TaskCreationOptions.LongRunning);

                ProcessingTasks.TryAdd($"worker_task{i}", task);
                processedStats.TryAdd($"worker_task{i}", new Stats());

                task.Start();
            }
        }

        private static void FillQueue()
        {
            /*for(int i = 0; i < max_forum_thread; i++)
            {
                Queue.Enqueue(i);
            }*/

            for(int i = 221847158; i < max_forum_thread + 221847158; i++)
            {
                Queue.Enqueue(i);
            }
        }

        /**********************************************
           =============== Updates ===============
         **********************************************/

        private static async void IterateThreadsDownloaded(string key, long time)
        {
            System.Threading.Interlocked.Increment(ref threadsDownloaded);
            downloadedStats[key].Count++;
            downloadedStats[key].TimeTaken += time;
        }

        private static async void IterateThreadsProcessed(string key, long time)
        {
            System.Threading.Interlocked.Increment(ref threadsProcessed);
            processedStats[key].Count++;
            processedStats[key].TimeTaken += time;
        }
        /**********************************************
           =============== Work ===============
         **********************************************/

        private static async Task DoDownloadWork(object state)
        {
            int downloads = 0;
            Stopwatch stopwatch = new Stopwatch();
            while (alive && downloads < max_downloads_per_thread)
            {
                stopwatch.Restart();
                int id;
                if (Queue.IsEmpty)
                {
                    break;
                }
                while (!Queue.TryDequeue(out id)) { }

                await DownloadThread(id);
                downloads++;

                stopwatch.Stop();
                IterateThreadsDownloaded(state.ToString(), stopwatch.ElapsedMilliseconds);
            }
            Console.WriteLine($"Download Task {state.ToString()} has been interrupted");
        }

        private static async Task DoWork(object state)
        {
            Stopwatch stopwatch = new Stopwatch();
            while (alive)
            {
                stopwatch.Restart();
                KeyValuePair<int, string> thread;
                if (UnparsedThreads.IsEmpty)
                {
                    //Console.WriteLine($"Queue is now empty");
                    //break;
                }
                while(!UnparsedThreads.TryDequeue(out thread)) { }

                await ProcessThread(thread.Key, thread.Value);
                stopwatch.Stop();
                IterateThreadsProcessed(state.ToString(), stopwatch.ElapsedMilliseconds);
            }
            Console.WriteLine($"Task {state.ToString()} has been interrupted");
        }

        private static async Task DownloadThread(int id)
        {
            string html = await _client.GetThread(id);
            UnparsedThreads.Enqueue(new KeyValuePair<int, string>(id, html));
            System.Threading.Interlocked.Increment(ref threadsDownloaded);
        }

        private static async Task ProcessThread(int id, string html)
        {
            RobloxThread thread = new RobloxThread(id);
            thread.AddPage(html);
            if (thread.IsEmpty)
            {
                System.Threading.Interlocked.Increment(ref emptyThreads);
                return;
            }
            if (thread.PagesCount > 1)
            {
                //Start at 1 since first page is already pulled
                for (int i = 1; i < thread.PagesCount; i++)
                {
                    string pageHtml = await _client.GetThread(id, i, thread.GetNextPageParams());
                    thread.AddPage(pageHtml);
                }
            }

            Thread dbThread = thread.ToDbThread();
            ForumThreads.Add(dbThread);
            System.Threading.Interlocked.Increment(ref threadsProcessed);
            return;
        }

        private static async Task PullThread(int id)
        {
            string html = await _client.GetThread(id);

            RobloxThread thread = new RobloxThread(id);
            thread.AddPage(html);
            if (thread.IsEmpty)
            {
                return;
            }
            if (thread.PagesCount > 1)
            {
                //Start at 1 since first page is already pulled
                for (int i = 1; i < thread.PagesCount; i++)
                {
                    string pageHtml = _client.GetThread(id, i, thread.GetNextPageParams()).GetAwaiter().GetResult();
                    thread.AddPage(pageHtml);
                }
            }

            Thread dbThread = thread.ToDbThread();
            ForumThreads.Add(dbThread);
            return;
        }

        private static void Interrupt(int timeout)
        {
            alive = false;
        }
    }
}
