using RobloxScraper.DbModels;
using RobloxScraper.RobloxModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RobloxScraper
{
    public static class TaskRunner
    {
        static RobloxClient _client = new RobloxClient(new HttpClientHandler());
        static int max_threads = 5;
        static int max_forum_thread = 150;

        static volatile bool alive = false;

        static TaskRunner()
        {
            Queue = new ConcurrentQueue<int>();
            ForumThreads = new ConcurrentBag<Thread>();
            Tasks = new ConcurrentDictionary<string, Task>();
        }

        static ConcurrentBag<Thread> ForumThreads { get; set; }
        static ConcurrentQueue<int> Queue { get; set; }
        static ConcurrentDictionary<string, Task> Tasks { get; set; }

        public static void StartProcessThreads()
        {
            alive = true;
            FillQueue();

            Task task = new Task(DoWork,"task1");
            Tasks.TryAdd("task1", task);
            task.Start();
            Task task2 = new Task(DoWork, "task2");
            Tasks.TryAdd("task2", task);
            task2.Start();
            Task task3 = new Task(DoWork, "task3");
            Tasks.TryAdd("task3", task);
            task3.Start();
            Console.WriteLine("Task started");

            Task.Delay(15000).ContinueWith(t => 
            {
                Interrupt(1500);
            });

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

        private static async void DoWork(object state)
        {
            while (alive)
            {
                int id;
                if (Queue.IsEmpty)
                {
                    Console.WriteLine($"Queue is now empty");
                    break;
                }
                while(!Queue.TryDequeue(out id)) { }

                await PullThread(id);
            }
            Console.WriteLine($"Task {state.ToString()} has been interrupted");
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
            Console.WriteLine($"Pulled Thread: {dbThread.Id} ");
            return;
        }

        private static void Interrupt(int timeout)
        {
            alive = false;
            //Task.Delay(timeout).ContinueWith(t => Interrupt());
        }
    }
}
