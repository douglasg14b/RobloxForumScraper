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

        static int max_workers = 3;
        static int max_downloaders = 15;


        static int max_forum_thread = 5000000;
        static int start_thread_modifier = 0;

        static int max_processing_queue = 100;
        static int max_database_queue = 250;

        static int max_downloads_per_thread = int.MaxValue;
        static ForumsRepository _repository;



        static TaskRunner()
        {
            Queue = new ConcurrentQueue<int>();
            ForumThreads = new ConcurrentBag<Thread>();
            DownloadTasks = new ConcurrentDictionary<string, Task>();
            ProcessingTasks = new ConcurrentDictionary<string, Task>();

            UnparsedThreads = new ConcurrentQueue<KeyValuePair<int, string>>();
            PartiallyParsedThreads = new ConcurrentQueue<KeyValuePair<RobloxThread, string>>();

            PageQueue = new ConcurrentQueue<RobloxThread>();

            downloadedStats = new ConcurrentDictionary<string, Stat>();
            pageDownloadStats = new ConcurrentDictionary<string, Stat>();
            processedStats = new ConcurrentDictionary<string, Stat>();
            pageProcessedStats = new ConcurrentDictionary<string, Stat>();
        }

        static bool alive = false;
        static bool downloadsActive = false;
        static bool processorsActive = false;

        /***********************
          ======  Stats  =====
         ***********************/
        public static ConcurrentDictionary<string, Stat> downloadedStats;
        public static ConcurrentDictionary<string, Stat> pageDownloadStats;
        public static ConcurrentDictionary<string, Stat> processedStats;
        public static ConcurrentDictionary<string, Stat> pageProcessedStats;

        public static int threadsDownloaded = 0;
        public static int threadsProcessed = 0;
        public static int emptyThreads = 0;

        /***********************
          ======  State  =====
         ***********************/

        //Fully parsed, waiting for database insertion
        public static ConcurrentBag<Thread> ForumThreads { get; private set; }

        //Raw html strings
        public static ConcurrentQueue<KeyValuePair<int, string>> UnparsedThreads { get; private set; }
        //Waiting on pages
        public static ConcurrentQueue<KeyValuePair<RobloxThread, string>> PartiallyParsedThreads { get; private set; } 

        //Queued up page pulls
        //{thread, pagehtml}
        public static ConcurrentQueue<RobloxThread> PageQueue { get; private set; }
        //Simple numbered queue
        public static ConcurrentQueue<int> Queue { get; private set; }

        public static ConcurrentDictionary<string, Task> DownloadTasks { get; private set; }
        public static ConcurrentDictionary<string, Task> ProcessingTasks { get; private set; }

        //ohgod just instantiate the damn class
        public static void Init(ForumsRepository repository, Config config)
        {
            _repository = repository;

            max_workers = config.MaxPocessors;
            max_downloaders = config.MaxDownloaders;
            max_forum_thread = config.MaxThread;
            start_thread_modifier = config.StartThread;
            max_database_queue += config.ThreadsBeforeWrite;
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

        }

        /**********************************************
          =============== Initilization ===============
         **********************************************/

        //Downloaders download thread pages
        private static void InitDownloaders()
        {
            for (int i = 0; i < max_downloaders; i++)
            {
                Task task = new Task(DoDownloadWork, $"download_task{i}", TaskCreationOptions.LongRunning);
                //Task task = new Task((object state) => { DoDownloadWork(state).GetAwaiter().GetResult(); }, $"download_task{i}", TaskCreationOptions.LongRunning);

                DownloadTasks.TryAdd($"download_task{i}", task);
                downloadedStats.TryAdd($"download_task{i}", new Stat());
                pageDownloadStats.TryAdd($"download_task{i}", new Stat());

                task.Start();
            }
        }

        //Workers parse and process the threads into waht will go into the database
        private static void InitWorkers()
        {
            for(int i = 0; i < max_workers; i++)
            {
                Task task = new Task(DoWork, $"worker_task{i}", TaskCreationOptions.LongRunning);
                //Task task = new Task((object state) => { DoWork(state).GetAwaiter().GetResult(); }, $"worker_task{i}", TaskCreationOptions.LongRunning);

                ProcessingTasks.TryAdd($"worker_task{i}", task);
                processedStats.TryAdd($"worker_task{i}", new Stat());
                pageProcessedStats.TryAdd($"worker_task{i}", new Stat());

                task.Start();
            }
        }

        private static void FillQueue(int startAt)
        {
            /*for(int i = 0; i < max_forum_thread; i++)
            {
                Queue.Enqueue(i);
            }*/

            if(startAt == 0)
            {
                startAt += start_thread_modifier;
            }
            for(int i = startAt + 1; i < max_forum_thread; i++)
            {
                Queue.Enqueue(i);
            }
        }

        /**********************************************
           =============== Updates ===============
         **********************************************/

        private static void IterateThreadsDownloaded(string key, long time)
        {
            System.Threading.Interlocked.Increment(ref threadsDownloaded);
            downloadedStats[key].Count++;
            downloadedStats[key].TimeTaken += time;
        }
        
        private static void IteratePagesDownloaded(string key, long time)
        {
            pageDownloadStats[key].Count++;
            pageDownloadStats[key].TimeTaken += time;
        }

        private static void IteratePagesProcessed(string key, long time)
        {
            pageProcessedStats[key].Count++;
            pageProcessedStats[key].TimeTaken += time;
        }

        private static void IterateThreadsProcessed(string key, long time)
        {
            System.Threading.Interlocked.Increment(ref threadsProcessed);
            processedStats[key].Count++;
            processedStats[key].TimeTaken += time;
        }

        private static void DownloaderComplete(string key)
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

        private static void ProcessorComplete(string key)
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

        private static void DoDownloadWork(object state)
        {
            int downloads = 0;
            Stopwatch stopwatch = new Stopwatch();
            while (alive && downloads < max_downloads_per_thread)
            {
                if(UnparsedThreads.Count > max_processing_queue)
                {
                    System.Threading.Thread.Sleep(10);
                    continue;
                }

                //Process page queue first
                if (!PageQueue.IsEmpty)
                {
                    stopwatch.Restart();
                    RobloxThread thread;
                    while (!PageQueue.TryDequeue(out thread)) { }
                    DownloadThreadPage(thread);
                    stopwatch.Stop();
                    IteratePagesDownloaded(state.ToString(), stopwatch.ElapsedMilliseconds);
                }

                stopwatch.Restart();
                int id;
                if (Queue.IsEmpty)
                {
                    DownloaderComplete(state.ToString());
                    break;
                }
                while (!Queue.TryDequeue(out id)) { }

                DownloadThread(id);
                downloads++;

                stopwatch.Stop();
                IterateThreadsDownloaded(state.ToString(), stopwatch.ElapsedMilliseconds);
            }
        }

        private static void DoWork(object state)
        {
            Stopwatch stopwatch = new Stopwatch();
            while (alive)
            {
                //Prioritize partial threads
                if (!PartiallyParsedThreads.IsEmpty)
                {
                    stopwatch.Restart();
                    KeyValuePair<RobloxThread, string> partialThread;
                    while (!PartiallyParsedThreads.TryDequeue(out partialThread)) { }
                    if (ProcessThreadPage(partialThread.Key, partialThread.Value))
                    {
                        Thread dbThread = partialThread.Key.ToDbThread();
                        ForumThreads.Add(dbThread);
                    }
                    stopwatch.Stop();
                    IteratePagesProcessed(state.ToString(), stopwatch.ElapsedMilliseconds);
                }

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
                try
                {
                    while (!UnparsedThreads.TryDequeue(out thread) && downloadsActive) { }
                }
                catch(Exception ex)
                {
                    Console.Write(ex);
                }
                

                ProcessThread(thread.Key, thread.Value);
                stopwatch.Stop();
                IterateThreadsProcessed(state.ToString(), stopwatch.ElapsedMilliseconds);
            }
        }

        private static void DownloadThread(int id)
        {
            string html = _client.GetThread(id).GetAwaiter().GetResult();
            UnparsedThreads.Enqueue(new KeyValuePair<int, string>(id, html));
            System.Threading.Interlocked.Increment(ref threadsDownloaded);
        }

        private static void DownloadThreadPage(RobloxThread thread)
        {
            string pageHtml = _client.GetThread(thread.ThreadId, thread.CurrentPage, thread.GetNextPageParams()).GetAwaiter().GetResult();
            PartiallyParsedThreads.Enqueue(new KeyValuePair<RobloxThread, string>(thread, pageHtml));
        }

        private static void ProcessThread(int id, string html)
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
            if (thread.PagesCount > 1 && thread.CurrentPage < thread.PagesCount)
            {
                PageQueue.Enqueue(thread);
                return;
                //Start at 1 since first page is already pulled
                /*for (int i = 1; i < thread.PagesCount; i++)
                {
                    string pageHtml = _client.GetThread(id, i, thread.GetNextPageParams()).GetAwaiter().GetResult();
                    if (String.IsNullOrEmpty(pageHtml))
                    {
                        thread.Errors += $"; Page {i} is error";
                        break;
                    }
                    thread.AddPage(pageHtml);
                }*/
            }

            Thread dbThread = thread.ToDbThread();
            ForumThreads.Add(dbThread);
            System.Threading.Interlocked.Increment(ref threadsProcessed);
            thread = null;
            return;
        }

        //Return value indicates if the thread is finished or not
        private static bool ProcessThreadPage(RobloxThread thread, string html)
        {
            if (String.IsNullOrEmpty(html))
            {
                thread.Errors += $"; Page {thread.CurrentPage} is error";
                return true;
            }
            thread.AddPage(html);
            if(thread.CurrentPage < thread.PagesCount)
            {
                PageQueue.Enqueue(thread);
                return false;
            }
            return true;
        }

        private static void PullThread(int id)
        {
            string html = _client.GetThread(id).GetAwaiter().GetResult();

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
