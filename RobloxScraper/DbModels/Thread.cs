using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RobloxScraper.DbModels
{
    public class Thread
    {
        public Thread() { }
        public Thread(int id, string title, Forum forum, List<Post> posts)
        {
            Id = id;
            Title = title;
            Forum = forum;
            Posts = posts;
        }


        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("title")]
        public string Title { get; set; }

        [ForeignKey("ForumId")]
        public Forum Forum { get; set; }
        [Column("forum")]
        public int ForumId { get; set; }

        public List<Post> Posts { get; set; }
    }
}
