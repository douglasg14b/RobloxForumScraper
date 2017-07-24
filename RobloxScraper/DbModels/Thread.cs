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
        public Thread(int id, string title, string errors, Forum forum, List<Post> posts)
        {
            Id = id;
            Title = title;
            Errors = errors;
            Forum = forum;
            Posts = posts;
            IsEmpty = false;
        }


        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("title")]
        public string Title { get; set; }
        [Column("errors")]
        public string Errors { get; set; }

        [ForeignKey("ForumId")]
        public Forum Forum { get; set; }
        [Column("forum")]
        public int? ForumId { get; set; }

        [Column("is_empty")]
        public bool IsEmpty { get; set; } = true;

        public List<Post> Posts { get; set; }
    }
}
