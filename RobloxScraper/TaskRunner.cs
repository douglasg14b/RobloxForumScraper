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

        static int max_workers = 7;
        static int max_downloaders = 7;

        static int max_downloads_per_thread = int.MaxValue;
        static int max_forum_thread = 100000;
        static ForumsRepository _repository;



        static TaskRunner()
        {
            Queue = new ConcurrentQueue<int>();
            ForumThreads = new ConcurrentBag<Thread>();
            DownloadTasks = new ConcurrentDictionary<string, Task>();
            ProcessingTasks = new ConcurrentDictionary<string, Task>();

            UnparsedThreads = new ConcurrentQueue<KeyValuePair<int, string>>();

            downloadedStats = new ConcurrentDictionary<string, Stat>();
            processedStats = new ConcurrentDictionary<string, Stat>();
        }

        static bool alive = false;
        static bool downloadsActive = false;
        static bool processorsActive = false;

        /***********************
          ======  Stats  =====
         ***********************/
        public static ConcurrentDictionary<string, Stat> downloadedStats;
        public static ConcurrentDictionary<string, Stat> processedStats;

        public static int threadsDownloaded = 0;
        public static int threadsProcessed = 0;
        public static int emptyThreads = 0;

        /***********************
          ======  State  =====
         ***********************/

        public static ConcurrentQueue<KeyValuePair<int, string>> UnparsedThreads { get; private set; }

        public static ConcurrentBag<Thread> ForumThreads { get; private set; }
        public static ConcurrentQueue<int> Queue { get; private set; }

        public static ConcurrentDictionary<string, Task> DownloadTasks { get; private set; }
        public static ConcurrentDictionary<string, Task> ProcessingTasks { get; private set; }

        //ohgod just instantiate the damn class
        public static void Init(ForumsRepository repository)
        {
            _repository = repository;
        }

        public static void Start()
        {
            alive = true;
            downloadsActive = true;
            processorsActive = true;
            int startAt = _repository.GetHighestThreadId();
            FillQueue(startAt);
            InitDownloaders();
            InitWorkers();

            Task.Delay(60000).ContinueWith(t => 
            {
                //Interrupt(1500);
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
                downloadedStats.TryAdd($"download_task{i}", new Stat());

                task.Start();
            }
        }

        private static void InitWorkers()
        {
            for(int i = 0; i < max_workers; i++)
            {
                Task task = new Task((object state) => { DoWork(state).GetAwaiter().GetResult(); }, $"worker_task{i}", TaskCreationOptions.LongRunning);

                ProcessingTasks.TryAdd($"worker_task{i}", task);
                processedStats.TryAdd($"worker_task{i}", new Stat());

                task.Start();
            }
        }

        private static void FillQueue(int startat)
        {
            /*for(int i = 0; i < max_forum_thread; i++)
            {
                Queue.Enqueue(i);
            }*/

            for(int i = startat + 1; i < max_forum_thread; i++)
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

        private static async void DownloaderComplete(string key)
        {
            foreach(Task task in DownloadTasks.Values)
            {
                if (!task.IsCompleted)
                {
                    return;
                }
            }
            downloadsActive = false;
        }

        private static async void ProcessorComplete(string key)
        {
            foreach (Task task in ProcessingTasks.Values)
            {
                if (!task.IsCompleted)
                {
                    return;
                }
            }
            processorsActive = false;
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
                    DownloaderComplete(state.ToString());
                    break;
                }
                while (!Queue.TryDequeue(out id)) { }

                await DownloadThread(id);
                downloads++;

                stopwatch.Stop();
                IterateThreadsDownloaded(state.ToString(), stopwatch.ElapsedMilliseconds);
            }
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
                    if (!downloadsActive)
                    {
                        ProcessorComplete(state.ToString());
                        return;
                    }
                    continue;
                }
                while(!UnparsedThreads.TryDequeue(out thread) && downloadsActive) { }

                await ProcessThread(thread.Key, thread.Value);
                stopwatch.Stop();
                IterateThreadsProcessed(state.ToString(), stopwatch.ElapsedMilliseconds);
            }
        }

        private static async Task DownloadThread(int id)
        {
            string html = await _client.GetThread(id);
            UnparsedThreads.Enqueue(new KeyValuePair<int, string>(id, html));
            System.Threading.Interlocked.Increment(ref threadsDownloaded);
        }

        private static async Task ProcessThread(int id, string html)
        {
            if (String.IsNullOrEmpty(html))
            {
                ForumThreads.Add(new Thread() { Id = id });
                System.Threading.Interlocked.Increment(ref emptyThreads);
                return;
            }

            RobloxThread thread = new RobloxThread(id);
            thread.AddPage(html);
            if (thread.IsEmpty)
            {
                ForumThreads.Add(new Thread() { Id = id });
                System.Threading.Interlocked.Increment(ref emptyThreads);
                return;
            }
            if (thread.PagesCount > 1)
            {
                //Start at 1 since first page is already pulled
                for (int i = 1; i < thread.PagesCount; i++)
                {
                    string pageHtml = await _client.GetThread(id, i, thread.GetNextPageParams());
                    if (String.IsNullOrEmpty(pageHtml))
                    {
                        thread.Errors += $"; Page {i} is error";
                        break;
                    }
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
