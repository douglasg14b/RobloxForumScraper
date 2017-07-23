using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RobloxScraper
{
    public class ConsoleUpdater
    {
        Timer timer;

        List<string> DownloadTasks { get; set; }
        List<string> ProcessTasks { get; set; }

        string pulled_line_value = "Threads Pulled: ";
        string processed_line_value = "Threads Processed: ";
        string empty_line_value = "Empty Threads: ";

        long totalDownloaded = 0;
        long totalProcessed = 0;
        long totalDownloadTime = 0;
        long totalProcessTime = 0;


        public ConsoleUpdater()
        {
            DownloadTasks = new List<string>(TaskRunner.DownloadTasks.Keys);
            ProcessTasks = new List<string>(TaskRunner.ProcessingTasks.Keys);
            timer = new Timer(UpdateConsole, null, 250, Timeout.Infinite);
        }

        private void UpdateConsole(object obj)
        {
            totalDownloaded = 0;
            totalProcessed = 0;
            totalDownloadTime = 0;
            totalProcessTime = 0;

            int index = UpdateDownloadTasksProgress();
            index = UpdateProcessTasksProgress(index+1);
            /*
            UpdateThreadsPulled();
            UpdateThreadsProcessed();
            UpdateEmptyThreads();
            */
            ResetCursorPosition(index);

            timer.Change(250, Timeout.Infinite);
        }

        private int UpdateDownloadTasksProgress()
        {
            int i = 0;
            for(; i < DownloadTasks.Count; i++)
            {
                long count = TaskRunner.downloadedStats[DownloadTasks[i]].Count;
                long time = TaskRunner.downloadedStats[DownloadTasks[i]].TimeTaken;
                float avg = TaskRunner.downloadedStats[DownloadTasks[i]].Average;

                string task = $"Task #{i}";
                string downloaded = $" Downloaded: {count}";
                string average = $" Avg ms: {avg}";
                string status;
                if (TaskRunner.DownloadTasks[DownloadTasks[i]].IsCompleted)
                {
                    status = "Completed";
                }
                else
                {
                    status = "Running";
                }

                task = task.PadRight(10);
                status = status.PadRight(10);
                downloaded = downloaded.PadRight(20);

                totalDownloaded += count;
                totalDownloadTime += time;

                Console.SetCursorPosition(0, i);
                Console.Write(task + status + downloaded + average);
            }
            return i;
        }

        private int UpdateProcessTasksProgress(int index)
        {
            int i = 0;
            for (; i < ProcessTasks.Count; i++)
            {
                long count = TaskRunner.downloadedStats[DownloadTasks[i]].Count;
                long time = TaskRunner.downloadedStats[DownloadTasks[i]].TimeTaken;
                float avg = TaskRunner.downloadedStats[DownloadTasks[i]].Average;

                string task = $"Task #{i}";
                string processed = $"Processed: {count}";
                string average = $" Avg ms: {avg}";
                string status;
                if (TaskRunner.ProcessingTasks[ProcessTasks[i]].IsCompleted)
                {
                    status = "Completed";
                }
                else
                {
                    status = "Running";
                }

                task = task.PadRight(10);
                status = status.PadRight(10);
                processed = processed.PadRight(20);

                totalProcessed += count;
                totalProcessTime += time;

                Console.SetCursorPosition(0, index + i);
                Console.Write(task + status + processed + average);


            }
            return index + i;
        }

        private void UpdateThreadsPulled()
        {
            
            Console.SetCursorPosition(0, 0);
            Console.Write(pulled_line_value + TaskRunner.threadsDownloaded);
        }

        private void UpdateThreadsProcessed()
        {
            Console.SetCursorPosition(0, 1);
            Console.Write(processed_line_value + TaskRunner.threadsProcessed);
        }

        private void UpdateEmptyThreads()
        {
            Console.SetCursorPosition(0, 2);
            Console.Write(empty_line_value + TaskRunner.emptyThreads);
        }

        private void ResetCursorPosition(int index)
        {
            Console.SetCursorPosition(0, index);
        }
    }
}
