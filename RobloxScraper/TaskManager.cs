using RobloxScraper.DbModels;
using RobloxScraper.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using RobloxScraper.RobloxModels;
using System.Diagnostics;
using System.Net.Http;
using RobloxScraper.Extensions;
using RobloxScraper.Telemetry;
using AngleSharp.Parser.Html;
using AngleSharp.Dom.Html;

namespace RobloxScraper
{
    public class TaskManager
    {
        /// <summary>
        /// When false all tasks will stop processing and run to completion
        /// </summary>
        public bool active;
        private bool downloadersActive;
        private bool workersActive;
        public Exception exception;


        private ForumsRepository repository;
        RobloxClient client;

        public ConcurrentDictionary<int, Task> Tasks { get; set; }
        public ConcurrentDictionary<int, Task> Downloaders { get; set; }
        public ConcurrentDictionary<int, Task> Workers { get; set; }

        /// <summary>
        /// Fully processed threads
        /// </summary>
        public ConcurrentQueue<DbModels.Thread> DatabaseQueue { get; set; }

        /// <summary>
        /// Unprocessed threads
        /// </summary>
        public ConcurrentQueue<UnparsedThread> ProcessingQueue { get; set; }

        /// <summary>
        /// Unprocessed pages queue, threads waiting on pages
        /// </summary>
        public ConcurrentQueue<UnparsedPage> PageProcessingQueue { get; set; }

        /// <summary>
        /// Undownlaoded pages queue
        /// </summary>
        public ConcurrentQueue<RobloxThread> PageDownloadingQueue { get; set; }

        /// <summary>
        /// Thread IDs to be downloaded
        /// </summary>
        public ConcurrentQueue<int> ThreadQueue { get; set; }

        public TaskManager(ForumsRepository repository)
        {
            client = new RobloxClient(new HttpClientHandler() { MaxConnectionsPerServer = 100 });
            this.repository = repository;
            InitVariables();
            active = false;
        }

        private void InitVariables()
        {
            Tasks = new ConcurrentDictionary<int, Task>();
            Downloaders = new ConcurrentDictionary<int, Task>();
            Workers = new ConcurrentDictionary<int, Task>();
            DatabaseQueue = new ConcurrentQueue<DbModels.Thread>();
            ProcessingQueue = new ConcurrentQueue<UnparsedThread>();
            PageProcessingQueue = new ConcurrentQueue<UnparsedPage>();
            PageDownloadingQueue = new ConcurrentQueue<RobloxThread>();
            ThreadQueue = new ConcurrentQueue<int>();
        }

        /*******************************
           ===== Startup/Shutdown =====
         *******************************/

        public async Task Start()
        {
            active = true;
            downloadersActive = true;
            workersActive = true;

            if (Config.PullEmptyThreads)
            {
                List<int> emptyThreads = await repository.GetEmptyThreadIdsAsync(Config.StartThread);
                await FillQueue(emptyThreads);
            }
            else
            {
                int mostRecent = await repository.GetHighestThreadIdAsync();
                await FillQueue(mostRecent);
            }
    
            await InitilizeTasks();

            await StartTasks();
        }

        /// <summary>
        /// Cleanly stop all tasks
        /// </summary>
        public void StopAll()
        {
            active = false;
            System.Threading.Tasks.Task.WaitAll(Tasks.Values.ToArray());
        }

        /// <summary>
        /// A pipe dream
        /// </summary>
        public void Pause()
        {
            throw new NotImplementedException();
        }

        private async Task FillQueue(List<int> emptyThreads)
        {
            await Task.Run(() =>
            {
                foreach(int id in emptyThreads)
                {
                    ThreadQueue.Enqueue(id);
                }               
            });
        }

        private async Task FillQueue(int mostrecent)
        {
            //CPU bound operation for large numbers, offload to task to maintain UI responsiveness
            await Task.Run(() =>
            {
                int start = mostrecent;
                if (mostrecent == 0)
                {
                    start += Config.StartThread;
                }
                else if(mostrecent < Config.StartThread)
                {
                    start = Config.StartThread;
                }

                for (int i = start + 1; i < Config.MaxThread; i++)
                {
                    ThreadQueue.Enqueue(i);
                }
            });
        }

        private async Task InitilizeTasks()
        {
            int max = Config.MaxDownloaders;
            for (int i = 0; i < Config.MaxDownloaders; i++)
            {
                TaskState state = new TaskState();
                Task task = new Task<Task>(DoDownloadWork, state);
                //Task task = DoDownloadWork(state);
                state.Id = task.Id;

                Downloaders.TryAdd(i, task);
                Tasks.TryAdd(task.Id, task);
            }

            for (int i = 0; i < Config.MaxProcessors; i++)
            {
                TaskState state = new TaskState();
                Task task = new Task<Task>( DoWork, state, TaskCreationOptions.LongRunning);
                state.Id = task.Id;

                Workers.TryAdd(i, task);
                Tasks.TryAdd(task.Id, task);
            }
        }

        private async Task StartTasks()
        {
            foreach(Task task in Downloaders.Values)
            {
                task.Start();
            }

            foreach (Task task in Workers.Values)
            {
                task.Start();
            }
        }


        /**************************
          ===== Worker Loops =====
         **************************/

        private async Task DoDownloadWork(object state)
        {
            TaskState taskState = (TaskState)state;
            Stopwatch stopwatch = new Stopwatch();
            while (active && downloadersActive)
            {
                taskState.Status = State.Running;
                if (!await CanDownload())
                {
                    taskState.Status = State.Paused;
                    await Task.Delay(500);
                    continue;
                }

                if (!PageDownloadingQueue.IsEmpty)
                {
                    RobloxThread thread;
                    if(!PageDownloadingQueue.TryDequeue(out thread))
                    {
                        //Another thread grabbed the item before we did, continue other work
                        continue;
                    }
                    try
                    {
                        await stopwatch.TimeAsync(
                            async () => await DownloadThreadPage(thread),
                            async (long time) => TelemetryManager.Incriment(TelemetryType.downloaded_pages, taskState.Id, time)
                        );
                    }
                    catch (Exception ex)
                    {
                        taskState.Status = State.Error;
                        active = false;
                        exception = ex;
                        break;
                    }

                    continue;
                }

                if (!ThreadQueue.IsEmpty)
                {
                    int id;
                    if (!ThreadQueue.TryDequeue(out id))
                    {
                        //Nothing left in queue?
                        //TODO: Handle end conditions
                        continue;
                    }

                    try
                    {
                        await stopwatch.TimeAsync(
                            async () => await DownloadThread(id),
                            async (long time) => TelemetryManager.Incriment(TelemetryType.downloaded_threads, taskState.Id, time)

                        );
                    }
                    catch(Exception ex)
                    {
                        taskState.Status = State.Error;
                        active = false;
                        exception = ex;
                        break;
                    }
                }
                else
                {                  
                    downloadersActive = false;
                    break;
                }
            }
            taskState.Status = State.Complete;
        }

        private async Task DoWork(object state)
        {
            TaskState taskState = (TaskState)state;
            Stopwatch stopwatch = new Stopwatch();
            while (active)
            {
                taskState.Status = State.Running;
                if (!await CanProcess() && downloadersActive)
                {
                    taskState.Status = State.Paused;
                    await Task.Delay(250);
                    continue;
                }

                if (!PageProcessingQueue.IsEmpty)
                {
                    UnparsedPage page;
                    if(!PageProcessingQueue.TryDequeue(out page))
                    {
                        //another thread grabbed the page
                        continue;
                    }
                    stopwatch.Time(() =>
                        {
                            return ProcessThreadPage(page.Thread, page.Html);
                        },
                        (long time) => TelemetryManager.Incriment(TelemetryType.processed_pages, taskState.Id, time)
                    );
                }

                if (!ProcessingQueue.IsEmpty)
                {
                    UnparsedThread thread;
                    if(!ProcessingQueue.TryDequeue(out thread))
                    {
                        continue;
                    }

                    stopwatch.Time(() =>
                    {
                        ProcessThread(thread.Id, thread.Html);
                    },
                        (long time) => TelemetryManager.Incriment(TelemetryType.processed_threads, taskState.Id, time)
                    );
                }
                else if (!downloadersActive) //Queue is empty, no more downloads are being performed, set active to false after 1s delay
                {
                    taskState.Status = State.Paused;
                    await Task.Delay(1000);
                    active = false;
                    break;
                }
            }
            taskState.Status = State.Complete;
        }

        /**************************
            ===== Downloading =====
         **************************/

        private async Task DownloadThread(int id)
        {
            string html = await client.GetThread(id);
            ProcessingQueue.Enqueue(new UnparsedThread(id, html));
        }

        private async Task DownloadThreadPage(RobloxThread thread)
        {
            string html = await client.GetThread(thread.ThreadId, thread.CurrentPage, thread.GetNextPageParams());
            PageProcessingQueue.Enqueue(new UnparsedPage(thread, html));
        }

        /**************************
            ===== Processing =====
         **************************/

        private void ProcessThread(int id, string html)
        {
            if (String.IsNullOrEmpty(html))
            {
                DatabaseQueue.Enqueue(new DbModels.Thread() { Id = id });
                TelemetryManager.IncrimentEmptyThreads();
                return;
            }

            RobloxThread thread = new RobloxThread(id);
            thread.AddPage(html);
            if (thread.IsEmpty)
            {
                DatabaseQueue.Enqueue(new DbModels.Thread() { Id = id });
                TelemetryManager.IncrimentEmptyThreads();
                return;
            }
            if (thread.PagesCount > 1 && thread.CurrentPage < thread.PagesCount)
            {
                PageDownloadingQueue.Enqueue(thread);
                return;
            }

            DbModels.Thread dbThread = thread.ToDbThread();
            DatabaseQueue.Enqueue(dbThread);
            thread = null; //TODO: Evaluate necessity
            return;
        }

        //Return value indicates if the thread is finished or not
        private bool ProcessThreadPage(RobloxThread thread, string html)
        {
            if (String.IsNullOrEmpty(html))
            {
                thread.Errors += $"; Page {thread.CurrentPage} is error";
                return true;
            }
            thread.AddPage(html);
            if (thread.CurrentPage < thread.PagesCount)
            {
                PageDownloadingQueue.Enqueue(thread);
                return false;
            }
            return true;
        }


        /**************************
            ===== Utility =====
         **************************/

        /// <summary>
        /// Determines if the downloaders are allowed to download based on the ProcessingQueue size
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CanDownload()
        {
            if(ProcessingQueue.Count >= Config.MaxProcessingQueue)
            {
                return false;
            }

            if(DatabaseQueue.Count >= Config.ThreadsBeforeWrite * Config.MaxDatabaseQueueModifier)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the process workers are allowed to process based on the DatabaseQueue size
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CanProcess()
        {
            return !ProcessingQueue.IsEmpty;
        }

    }
}
