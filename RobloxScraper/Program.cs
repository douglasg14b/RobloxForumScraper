using RobloxScraper.DbModels;
using RobloxScraper.RobloxModels;
using RobloxScraper.Telemetry;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RobloxScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            SetupEncodingProvider();
            Config.Initialize(new IniFile());
            ForumsRepository respository = new ForumsRepository(new ForumsContext());

            TaskManager manager = new TaskManager(respository);
            DbManager dbManager = new DbManager(respository, manager);
            ConsoleOutput consoleOutput = new ConsoleOutput(manager, dbManager, respository);
            Task.Run(async () =>
            {
                await manager.Start();
            }).GetAwaiter().GetResult();
            

            /*TaskRunner.Init(respository, config);
            TaskRunner.Start();
            StatsManager updater = new StatsManager();
            DbManager dbManager = new DbManager(respository, config);*/

            Console.ReadKey(true);
        }

        static void SetupEncodingProvider()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}