using RobloxScraper.DbModels;
using RobloxScraper.RobloxModels;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace RobloxScraper
{
    class Program
    {
        static RobloxClient _client = new RobloxClient(new HttpClientHandler());

        static void Main(string[] args)
        {
            /*RobloxThread post = GetThread(221987920).GetAwaiter().GetResult();
            Thread thread = post.ToDbThread();

            using(var db = new ForumsContext())
            {
                db.Threads.Add(thread);
                var result = db.SaveChanges();
            }*/


            ForumsRepository respository = new ForumsRepository(new ForumsContext());

            TaskRunner.Init(respository);
            TaskRunner.Start();
            StatsManager updater = new StatsManager();
            DbManager dbManager = new DbManager(respository);

            Console.ReadKey(true);
        }


        private static async Task<RobloxThread> GetThread(int id)
        {
            string html = await _client.GetThread(id);

            RobloxThread thread = new RobloxThread(id);
            thread.AddPage(html);
            if(thread.PagesCount == 1)
            {
                return thread;
            }
            else
            {
                //Start at 1 since first page is already pulled
                for(int i = 1; i < thread.PagesCount; i++)
                {
                    string pageHtml = _client.GetThread(id, i, thread.GetNextPageParams()).GetAwaiter().GetResult();
                    thread.AddPage(pageHtml);
                }
            }
            return thread;
        }
    }
}