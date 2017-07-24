using RobloxScraper.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RobloxScraper.DbModels
{
    public class ForumsRepository
    {
        private ForumsContext _context;

        public ForumsRepository(ForumsContext context)
        {
            _context = context;
        }

        public void InsertThreads(IEnumerable<Thread> threads)
        {

            _context.Threads.AddRange(threads);
            _context.SaveChanges();
        }

        public int GetHighestThreadId()
        {
            int? max = _context.Threads.Max(t => t.Id);
            return max ?? 0;
        }
    }
}
