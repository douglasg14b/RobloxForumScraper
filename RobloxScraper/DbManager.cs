using RobloxScraper.DbModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RobloxScraper
{
    public class DbManager
    {
        Timer timer;
        ForumsRepository repository;

        public DbManager(ForumsRepository repository)
        {
            this.repository = repository;
            timer = new Timer(Poll, null, 1000, Timeout.Infinite);
        }

        private void Poll(object state)
        {
            int requiredThreads = 1;
            if(TaskRunner.ForumThreads == null)
            {
                timer.Change(1000, Timeout.Infinite);
                return;
            }

            if(TaskRunner.ForumThreads.Count > requiredThreads)
            {
                List<DbModels.Thread> threads = new List<DbModels.Thread>();
                for(int i = 0; i < requiredThreads; i++)
                {
                    DbModels.Thread thread;
                    if(TaskRunner.ForumThreads.TryTake(out thread))
                    {
                        threads.Add(thread);
                    }
                }
                repository.InsertThreads(threads);
            }
            
            timer.Change(1000, Timeout.Infinite);
        }
    }
}
