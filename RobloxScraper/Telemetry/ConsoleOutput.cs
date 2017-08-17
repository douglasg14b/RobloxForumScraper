using BetterConsoleTables;
using RobloxScraper.DbModels;
using RobloxScraper.Processing;
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
        DbManager dbManager;
        ForumsRepository repository;
        public ConsoleOutput(TaskManager taskManager, DbManager dbManager, ForumsRepository repository)
        {
            this.taskManager = taskManager;
            this.dbManager = dbManager;
            this.repository = repository;
            timer = new Timer(UpdateConsole, null, 250, Timeout.Infinite);
        }

        private void UpdateConsole(object state)
        {
            if (taskManager.exception == null)
            {
                Console.SetCursorPosition(0, 0);
                ConsoleTables tables = new ConsoleTables();
                tables.AddTable(GetOverallStatsTable());
                tables.AddTable(GetDownloadStatsTable());
                tables.AddTable(GetPocessedStatsTable());

                string output = tables.ToString();

                Console.Write(tables);
                timer.Change(250, Timeout.Infinite);
            }
            else
            {
                WriteException();
            }
        }

        private void WriteException()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine();
            Console.WriteLine("An exception has occured while trying to download. Downloading and processing have been stopped.");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Message: ");
            Console.ResetColor();
            Console.WriteLine("        " + taskManager.exception.Message);
            if(taskManager.exception.InnerException != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Inner Message: ");
                Console.ResetColor();
                Console.WriteLine("        " + taskManager.exception.InnerException.Message);
            }
        }

        /**********************
          ==== Downloaded ====
         **********************/

        private Table GetDownloadStatsTable()
        {
            Table table = new Table(BetterConsoleTables.Config.Markdown(), "Worker", "Status", "Downloaded", "Pages Downloaded", "Avg Time(ms)");
            AddDownloadRows(table);
            AddDownloadedTotalsRow(table);
            return table;
        }

        private void AddDownloadRows(Table table)
        {
            foreach (int key in TelemetryManager.downloadedThreadStats.Keys)
            {
                TaskState state = (TaskState)taskManager.Tasks[key].AsyncState;
                State status;
                if (state == null)
                {
                    status = State.None;
                }
                else
                {
                    status = state.Status;
                }
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

                table.AddRow(key, status.ToString(), threadCount, pageCount, stat.AverageTime);               
            }
        }

        private void AddDownloadedTotalsRow(Table table)
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

        private Table GetPocessedStatsTable()
        {
            Table table = new Table(BetterConsoleTables.Config.Markdown(), "Worker", "Status", "Processed ", "Pages Processed ", "Avg Time(ms)");
            AddPocessedRows(table);
            AddProcessedTotalsRow(table);
            AddProcessedEmptyTotalRow(table);
            return table;
        }

        private void AddPocessedRows(Table table)
        {
            foreach (int key in TelemetryManager.processedThreadStats.Keys)
            {
                TaskState state = (TaskState)taskManager.Tasks[key].AsyncState;
                State status;
                if (state == null)
                {
                    status = State.None;
                }
                else
                {
                    status = state.Status;
                }
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

                table.AddRow(key, status.ToString(), threadCount, pageCount, stat.AverageTime);
            }
        }

        private void AddProcessedTotalsRow(Table table)
        {
            int workerCount = TelemetryManager.processedThreadStats.Count;
            Stat threads = TelemetryManager.overallProcessedThreads;
            Stat pages = TelemetryManager.overallProcessedPages;

            Stat totals = threads + pages;
            table.AddRow("---", "Total", threads.count, pages.count, totals.AverageTime / workerCount);
        }

        private void AddProcessedEmptyTotalRow(Table table)
        {
            Stat empty = TelemetryManager.emptyThreads;

            table.AddRow("---","Total Empty:", empty.count, "---", "---");
        }
        /**********************
          ==== Overall ====
         **********************/

        private Table GetOverallStatsTable()
        {
            Table table = new Table(BetterConsoleTables.Config.MySqlSimple(), "Time Elapsed", "Latest Thread", "Thread Queue", "Processing Queue", "Database Queue", "Db Status", "Test");

            TimeSpan elapsed = DateTime.Now.Subtract(TelemetryManager.startTime);
            taskManager.ThreadQueue.TryPeek(out int latestThread);
            int threadQueue = taskManager.ThreadQueue.Count;
            int processQueue = taskManager.ProcessingQueue.Count;
            int databaseQueue = taskManager.DatabaseQueue.Count;

            table.AddRow(elapsed, latestThread, threadQueue, processQueue, databaseQueue, dbManager.status.ToString(), repository.status);

            return table;
        }
    }
}
