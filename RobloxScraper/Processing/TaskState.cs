using System;
using System.Collections.Generic;
using System.Text;

namespace RobloxScraper.Processing
{
    public class TaskState
    {
        public TaskState()
        {
            Status = "Pending";
        }

        public TaskState(int id)
        {
            Id = id;
            Status = "Pending";
        }

        public string Status { get; set; }
        public int Id { get; set; }
    }
}
