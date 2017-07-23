using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RobloxScraper.DbModels
{
    public class ForumGroup
    {
        public ForumGroup() { }
        public ForumGroup(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public ForumGroup(int id, string name, List<Forum> forums)
        {
            Id = id;
            Name = name;
            Forums = forums;
        }


        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        public List<Forum> Forums { get; set; }
    }
}
