using RobloxScraper.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RobloxScraper
{
    public class DbManager
    {
        int timeout = 250;

        Timer timer;
        ForumsRepository repository;
        TaskManager taskManager;
        int required_threads;
        public DbStatus status = DbStatus.Uninitilized;

        public DbManager(ForumsRepository repository, TaskManager taskManager)
        {
            status = DbStatus.Initilizing;
            required_threads = Config.ThreadsBeforeWrite;
            this.repository = repository;
            this.taskManager = taskManager;
            timer = new Timer(Poll, null, timeout, Timeout.Infinite);
        }

        private void Poll(object state)
        {
            status = DbStatus.Polling;
            if (taskManager.DatabaseQueue == null)
            {
                timer.Change(timeout, Timeout.Infinite);
                return;
            }

            if((taskManager.DatabaseQueue.Count > required_threads || taskManager.active == false) && taskManager.DatabaseQueue.Count > 0)
            {
                status = DbStatus.Cleaning;
                List<DbModels.Thread> threads = new List<DbModels.Thread>();
                for(int i = 0; i < required_threads; i++)
                {
                    DbModels.Thread thread;
                    if(taskManager.DatabaseQueue.TryDequeue(out thread))
                    {
                        threads.Add(thread);
                    }
                }
                RemoveExistingEntityReferences(threads);
                status = DbStatus.Writing;
                /*
                if (Config.PullEmptyThreads)
                {
                    repository.UpdateThreads(threads);
                }
                else
                {
                    repository.InsertThreads(threads);
                }*/
                repository.InsertOrUpdateThreads(threads);
            }

            status = DbStatus.Polling;
            timer.Change(timeout, Timeout.Infinite);
        }

        private void RemoveExistingEntityReferences(List<DbModels.Thread> threads)
        {
            if(threads[0].Forum != null && threads[0].Forum.Id == 8)
            {
                int i = 0;
            }
            List<int> forumGroups = GetUniqueForumGroups(threads);
            List<int?> forums = GetUniqueForums(threads);
            List<int> users = GetUniqueUsers(threads);
            GetAndRemoveExistingForumGroups(forumGroups, threads);
            GetAndRemoveExistingForums(forums, threads);
            GetAndRemoveExistingUsers(users, threads);
        }

        //Necessary to work around entity frameworks issue with inserting duplicate entities
        private void GetAndRemoveExistingForumGroups(List<int> forumGroups, List<DbModels.Thread> threads)
        {
            List<int> existing = new List<int>();
            foreach(int id in forumGroups)
            {
                ForumGroup group = repository.GetForumGroupById(id);
                if(group != null)
                {
                    existing.Add(group.Id);
                }
            }

            foreach(DbModels.Thread thread in threads)
            {
                if(thread.Forum != null)
                {
                    foreach(int id in existing)
                    {
                        if (thread.Forum.ForumGroupId == id)
                        {
                            thread.Forum.ForumGroup = null;
                            break;
                        }
                    }
                }
            }
        }

        //Necessary to work around entity frameworks issue with inserting duplicate entities
        private void GetAndRemoveExistingForums(List<int?> forums, List<DbModels.Thread> threads)
        {
            List<int> existing = new List<int>();
            foreach (int id in forums)
            {
                Forum forum = repository.GetForumById(id);
                if (forum != null)
                {
                    existing.Add(forum.Id);
                }
            }

            foreach (DbModels.Thread thread in threads)
            {
                if (thread.Forum != null)
                {
                    foreach (int id in existing)
                    {
                        if (thread.Forum.Id == id)
                        {
                            thread.Forum = null;
                            break;
                        }
                    }
                }
            }
        }

        //Necessary to work around entity frameworks issue with inserting duplicate entities
        private void GetAndRemoveExistingUsers(List<int> users, List<DbModels.Thread> threads)
        {
            if(users.Count == 0)
            {
                return;
            }

            status = DbStatus.CleaningUsers;

            List<int> existing = new List<int>();
            foreach (int id in users)
            {
                User user = repository.GetUserById(id);
                if (user != null)
                {
                    existing.Add(user.Id);
                }
            }

            //Ohgod so inefficient....
            foreach (DbModels.Thread thread in threads)
            {
                if(thread.Posts != null)
                {
                    foreach (Post post in thread.Posts)
                    {
                        foreach (int id in existing)
                        {
                            if (post.User.Id == id)
                            {
                                post.User = null;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private List<int> GetUniqueForumGroups(List<DbModels.Thread> threads)
        {
            return threads.Where(t => t.Forum != null).Select(t => t.Forum.ForumGroupId).Distinct().ToList();
        }

        private List<int?> GetUniqueForums(List<DbModels.Thread> threads)
        {
            return threads.Where(t => t.Forum != null).Select(t => t.ForumId).Distinct().ToList();
        }

        private List<int> GetUniqueUsers(List<DbModels.Thread> threads)
        {          
            return threads.Where(t => t.Posts != null).SelectMany(t => t.Posts.Select(p => p.UserId)).Distinct().ToList();
        }
    }

    public enum DbStatus
    {
        Uninitilized = 0,
        Initilizing = 1,
        Polling = 2,
        Cleaning = 3,
        CleaningUsers = 4,
        Writing = 5
    }
}
