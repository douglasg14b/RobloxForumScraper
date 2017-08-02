using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RobloxScraper.Telemetry
{
    public class ConsoleOutput
    {
        Timer timer;
        TaskManager taskManager;
        public ConsoleOutput(TaskManager taskManager)
        {
            this.taskManager = taskManager;
            timer = new Timer(UpdateConsole, null, 250, Timeout.Infinite);
        }

        private void UpdateConsole(object state)
        {
            Console.SetCursorPosition(0, 0);

            string statsTable = GetOverallStatsTable();
            string downloadTable = GetDownloadStatsTable();
            string processedTable = GetPocessedStatsTable();

            Console.Write(statsTable);
            Console.Write(downloadTable);
            Console.WriteLine("");
            Console.WriteLine("");
            Console.Write(processedTable);
            timer.Change(250, Timeout.Infinite);
        }

        /**********************
          ==== Downloaded ====
         **********************/

        private string GetDownloadStatsTable()
        {
            ConsoleTable table = new ConsoleTable("Worker", "Status", "Downloaded", "Pages Downloaded", "Avg Time(ms)");
            AddDownloadRows(table);
            AddDownloadedTotalsRow(table);
            return table.ToMarkDownString();
        }

        private void AddDownloadRows(ConsoleTable table)
        {
            foreach (int key in TelemetryManager.downloadedThreadStats.Keys)
            {
                string status = taskManager.Tasks[key].Status.ToString();
                long threadCount = TelemetryManager.downloadedThreadStats[key].count;
                long threadTime = TelemetryManager.downloadedThreadStats[key].time;
                long pageCount = 0;
                long pagetime = 0;
                if (TelemetryManager.downloadedPageStats.ContainsKey(key))
                {
                    pageCount = TelemetryManager.downloadedPageStats[key].count;
                    pagetime = TelemetryManager.downloadedPageStats[key].time;
                }

                Stat stat = new Stat(threadCount + pageCount, threadTime + pagetime);

                table.AddRow(key, status, threadCount, pageCount, stat.AverageTime);               
            }
        }

        private void AddDownloadedTotalsRow(ConsoleTable table)
        {
            int workerCount = TelemetryManager.downloadedThreadStats.Count;
            Stat threads = TelemetryManager.overallDownloadedThreads;
            Stat pages = TelemetryManager.overallDownloadedPages;

            Stat totals = threads + pages;
            table.AddRow("---", "Total", threads.count, pages.count, totals.AverageTime / workerCount);
        }


        /**********************
          ==== Processed ====
         **********************/

        private string GetPocessedStatsTable()
        {
            ConsoleTable table = new ConsoleTable("Worker", "Status", "Processed ", "Pages Processed ", "Avg Time(ms)");
            AddPocessedRows(table);
            AddProcessedTotalsRow(table);
            return table.ToMarkDownString();
        }

        private void AddPocessedRows(ConsoleTable table)
        {
            foreach (int key in TelemetryManager.processedThreadStats.Keys)
            {
                string status = taskManager.Tasks[key].Status.ToString();
                long threadCount = TelemetryManager.processedThreadStats[key].count;
                long threadTime = TelemetryManager.processedThreadStats[key].time;
                long pageCount = 0;
                long pagetime = 0;
                if (TelemetryManager.processedPageStats.ContainsKey(key))
                {
                    pageCount = TelemetryManager.processedPageStats[key].count;
                    pagetime = TelemetryManager.processedPageStats[key].time;
                }

                Stat stat = new Stat(threadCount + pageCount, threadTime + pagetime);

                table.AddRow(key, status, threadCount, pageCount, stat.AverageTime);
            }
        }

        private void AddProcessedTotalsRow(ConsoleTable table)
        {
            int workerCount = TelemetryManager.processedThreadStats.Count;
            Stat threads = TelemetryManager.overallProcessedThreads;
            Stat pages = TelemetryManager.overallProcessedPages;

            Stat totals = threads + pages;
            table.AddRow("---", "Total", threads.count, pages.count, totals.AverageTime / workerCount);
        }

        /**********************
          ==== Overall ====
         **********************/

        private string GetOverallStatsTable()
        {
            ConsoleTable table = new ConsoleTable("Time Elapsed", "Latest Thread", "Thread Queue", "Processing Queue", "Database Queue");

            TimeSpan elapsed = DateTime.Now.Subtract(TelemetryManager.startTime);
            taskManager.ThreadQueue.TryPeek(out int latestThread);
            int threadQueue = taskManager.ThreadQueue.Count;
            int processQueue = taskManager.ProcessingQueue.Count;
            int databaseQueue = taskManager.DatabaseQueue.Count;

            table.AddRow(elapsed, latestThread, threadQueue, processQueue, databaseQueue);

            return table.ToStringAlternative();
        }
    }
}
