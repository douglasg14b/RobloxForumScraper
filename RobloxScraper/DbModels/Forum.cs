using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RobloxScraper.DbModels
{
    public class Forum
    {
        public Forum() { }
        public Forum(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public Forum(int id, string name, ForumGroup forumgroup)
        {
            Id = id;
            Name = name;
            ForumGroup = forumgroup;
            ForumGroupId = forumgroup.Id;
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [ForeignKey("ForumGroupId")]
        public ForumGroup ForumGroup { get; set; }
        [Column("forum_group_id")]
        public int ForumGroupId { get; set; }
    }
}
