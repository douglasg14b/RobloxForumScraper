using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RobloxScraper.DbModels
{
    public class Post
    {
        public Post() { }
        public Post(DateTime timestamp, string body, User user)
        {
            Timestamp = timestamp;
            Body = body;
            User = user;
        }

        public Post(DateTime timestamp, string body, User user, Thread thread)
        {
            Timestamp = timestamp;
            Body = body;
            User = user;
            Thread = thread;
        }

        [Column("id")]
        public int Id { get; set; }
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
        [Column("body")]
        public string Body { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }

        [ForeignKey("ThreadId")]
        public Thread Thread { get; set; }
        [Column("thread_id")]
        public int ThreadId { get; set; }
    }
}
