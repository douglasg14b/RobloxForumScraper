using Microsoft.EntityFrameworkCore;
using RobloxScraper.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobloxScraper.Extensions;

namespace RobloxScraper.DbModels
{
    public class ForumsRepository
    {
        public string status = String.Empty;
        private ForumsContext _context;

        public ForumsRepository(ForumsContext context)
        {
            _context = context;
        }

        public void InsertOrUpdateThreads(List<Thread> threads)
        {

            for(int i = 0; i < threads.Count; i++)
            {
                status = $"Adding {i} of {threads.Count}";
                if (_context.Threads.AsNoTracking().Where(t => t.Id == threads[i].Id).FirstOrDefault() != null)
                {
                    _context.Threads.Update(threads[i]);
                }
                else
                {
                    _context.Threads.Add(threads[i]);
                }
            }
            status = $"Saving";
            _context.SaveChanges();
            DetachAllEntities();
            status = String.Empty;
        }

        public void UpdateThreads(List<Thread> threads)
        {
            for(int i = 0; i < threads.Count; i++)
            {
                //_context.Entry(threads[i]).State = EntityState.Modified;
                _context.Threads.Update(threads[i]);
                status = $"Adding {i} of {threads.Count}";              
            }

            status = $"Saving";
            _context.SaveChanges();
            DetachAllEntities();
            status = String.Empty;
        }

        public void InsertThreads(IEnumerable<Thread> threads)
        {
            _context.Threads.AddRange(threads);
            _context.SaveChanges();
        }

        public async Task<int> GetHighestThreadIdAsync()
        {
            int? max = await _context.Threads.AsNoTracking().MaxAsync(t => t.Id);
            return max ?? 0;
        }

        public async Task<List<int>> GetEmptyThreadIdsAsync()
        {
            return await _context.Threads.AsNoTracking().Where(t => t.IsEmpty == true).Select(t => t.Id).ToListAsync();
        }

        public async Task<List<int>> GetEmptyThreadIdsAsync(int startAt)
        {
            return await _context.Threads.AsNoTracking().Where(t => t.IsEmpty == true && t.Id >= startAt).Select(t => t.Id).ToListAsync();
        }
        public int GetHighestThreadId()
        {
            int? max = _context.Threads.AsNoTracking().Max(t => t.Id);
            return max ?? 0;
        }

        public ForumGroup GetForumGroupById(int id)
        {
            return _context.ForumGroups.AsNoTracking().Where(f => f.Id == id).FirstOrDefault();
        }

        public Thread GetThreadById(int id)
        {
            return _context.Threads.AsNoTracking().Where(t => t.Id == id).FirstOrDefault();
        }

        public Forum GetForumById(int id)
        {
            return _context.Forums.AsNoTracking().Where(f => f.Id == id).FirstOrDefault();
        }

        public User GetUserById(int id)
        {
            return _context.Users.AsNoTracking().Where(f => f.Id == id).FirstOrDefault();
        }

        public List<int> GetUsersIdsByIds(List<int> ids)
        {
            return _context.Users.AsNoTracking().Where(u => ids.Contains(u.Id)).Select(u => u.Id).ToList();
        }

        private void DetachAllEntities()
        {
            status = $"Detaching";
            var entries = _context.ChangeTracker.Entries().ToList();
            for(int i = 0; i < entries.Count; i++)
            {
                status = $"Detaching {i} of {entries.Count}";
                _context.Entry(entries[i].Entity).State = EntityState.Detached;
            }
        }
    }
}
