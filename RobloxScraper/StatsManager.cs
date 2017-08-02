using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RobloxScraper
{
    public class StatsManager
    {
        DateTime startTime;
        Timer timer;
        Timer snapshot;

        List<string> DownloadTasks { get; set; }
        List<string> ProcessTasks { get; set; }

        long totalDownloaded = 0;
        long totalPagesDownloaded = 0;
        long totalProcessed = 0;
        long totalPagesProcessed = 0;
        long totalDownloadTime = 0;
        long totalPagesDownloadTime = 0;
        long totalProcessTime = 0;
        long totalPagesProcessedTime = 0;

        Stat downloadSnapshot;
        Stat processedSnapshot;


        public StatsManager()
        {
            startTime = DateTime.Now;
            DownloadTasks = new List<string>(TaskRunner.DownloadTasks.Keys);
            ProcessTasks = new List<string>(TaskRunner.ProcessingTasks.Keys);
            downloadSnapshot = new Stat(totalDownloaded, totalProcessed);
            processedSnapshot = new Stat(totalProcessed, totalProcessTime);

            timer = new Timer(UpdateConsole, null, 250, Timeout.Infinite);
            snapshot = new Timer(UpdateSnapshot, null, 5000, Timeout.Infinite);
        }

        private void UpdateSnapshot(object obj)
        {
            downloadSnapshot = new Stat(totalDownloaded, totalProcessed);
            processedSnapshot = new Stat(totalProcessed, totalProcessTime);
            snapshot.Change(5000, Timeout.Infinite);
        }

        private void UpdateConsole(object obj)
        {
            totalDownloaded = 0;
            totalPagesDownloaded = 0;
            totalProcessed = 0;
            totalPagesProcessed = 0;
            totalDownloadTime = 0;
            totalPagesDownloadTime = 0;
            totalProcessTime = 0;
            totalPagesProcessedTime = 0;

            string statsTable = GetStatsTable();
            string downloadTable = GetDownloadTable();
            string processedTable = GetProcessedTable();

            Console.SetCursorPosition(0, 0);
            Console.Write(statsTable);
            Console.WriteLine();
            Console.Write(downloadTable);
            Console.WriteLine();
            Console.Write(processedTable);

            timer.Change(250, Timeout.Infinite);
        }

        private string GetStatsTable()
        {
            var table = new ConsoleTable("Time Elapsed", "Latest Thread", "Thread Queue", "Processing Queue", "Database Queue");
            TimeSpan elapsed = DateTime.Now.Subtract(startTime);
            int latest;
            TaskRunner.Queue.TryPeek(out latest);
            table.AddRow(elapsed.ToString(@"hh\:mm\:ss"), latest, TaskRunner.Queue.Count, TaskRunner.UnparsedThreads.Count, TaskRunner.ForumThreads.Count);
            return table.ToStringAlternative();
        }

        private string GetDownloadTable()
        {
            var table = new ConsoleTable("Worker", "Status", "Downloaded", "Pages Downloaded", "Avg Time(ms)");
            AddDownloadTaskRows(table);
            AddDownloadTotalsRow(table);
            return table.ToMarkDownString();
        }

        private string GetProcessedTable()
        {
            var table = new ConsoleTable("Worker", "Status", "Processed", "Pages Processed ", "Avg Time(ms)");
            AddProcessedTaskRows(table);
            AddProcessedTotalsRow(table);
            AddEmptyTotalsRow(table);
            return table.ToMarkDownString();
        }

        private void AddDownloadTotalsRow(ConsoleTable table)
        {
            float time = float.NaN;
            if(totalDownloaded + totalPagesDownloaded > 0 && DownloadTasks.Count > 0)
            {
                time = (totalDownloadTime + totalPagesDownloadTime) / DownloadTasks.Count / (totalDownloaded + totalPagesDownloaded);
            }

            table.AddRow("---", "Total", totalDownloaded, totalPagesDownloaded, time);
        }

        private void AddProcessedTotalsRow(ConsoleTable table)
        {
            float time = float.NaN;
            if (totalProcessed + totalPagesProcessed > 0 && ProcessTasks.Count > 0)
            {
                time = (totalProcessTime + totalPagesProcessedTime) / ProcessTasks.Count / (totalProcessed + totalPagesProcessed);
            }

            table.AddRow("---", "Total", totalProcessed, totalPagesProcessed, time);
        }

        private void AddEmptyTotalsRow(ConsoleTable table)
        {
            table.AddRow("---", "Total Empty", TaskRunner.emptyThreads, "N/A", "N/A");
        }

        private void AddDownloadTaskRows(ConsoleTable table)
        {
            for (int i = 0; i < DownloadTasks.Count; i++)
            {
                long count = TaskRunner.downloadedStats[DownloadTasks[i]].Count;
                long pagesCount = TaskRunner.pageDownloadStats[DownloadTasks[i]].Count;
                long time = TaskRunner.downloadedStats[DownloadTasks[i]].Time;
                long pagesTime = TaskRunner.pageDownloadStats[DownloadTasks[i]].Time;

                float avg = new Stat(count + pagesCount, time + pagesTime).Average;


                string status = GetWorkerStatus(TaskRunner.DownloadTasks[DownloadTasks[i]]);
                int id = TaskRunner.DownloadTasks[DownloadTasks[i]].Id;

                table.AddRow($"#{i} ({id})", status, count, pagesCount, avg);

                totalDownloaded += count;
                totalPagesDownloaded += pagesCount;
                totalDownloadTime += time;
                totalPagesDownloadTime += pagesTime;
            }
        }

        private void AddProcessedTaskRows(ConsoleTable table)
        {
            for (int i = 0; i < ProcessTasks.Count; i++)
            {
                long count = TaskRunner.processedStats[ProcessTasks[i]].Count;
                long pagesCount = TaskRunner.pageProcessedStats[ProcessTasks[i]].Count;
                long time = TaskRunner.processedStats[ProcessTasks[i]].Time;
                long pagesTime = TaskRunner.pageProcessedStats[ProcessTasks[i]].Time;

                float avg = new Stat(count+pagesCount, time+pagesTime).Average;

                string status = GetWorkerStatus(TaskRunner.ProcessingTasks[ProcessTasks[i]]);
                if(status == "Faulted")
                {
                    Console.WriteLine(TaskRunner.ProcessingTasks[ProcessTasks[i]].Exception);
                }
                int id = TaskRunner.ProcessingTasks[ProcessTasks[i]].Id;

                table.AddRow($"#{i} ({id})", status, count, pagesCount, avg);

                totalProcessed += count;
                totalPagesProcessed += pagesCount;
                totalProcessTime += time;
                totalPagesProcessedTime += pagesTime;
            }
        }

        private string GetWorkerStatus(System.Threading.Tasks.Task task)
        {
            return task.Status.ToString();
        }

        private float GetSnapshotAvg(string type, long count, long time)
        {
            if(type == "download")
            {
                return new Stat(count - downloadSnapshot.Count, time - downloadSnapshot.Time).Average;
            }
            else
            {
                return new Stat(count - processedSnapshot.Count, time - processedSnapshot.Time).Average;
            }
        }

    }
}
