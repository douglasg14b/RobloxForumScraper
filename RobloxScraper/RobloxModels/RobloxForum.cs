using System;
using System.Collections.Generic;
using System.Text;
using RobloxScraper.DbModels;

namespace RobloxScraper.RobloxModels
{
    public class RobloxForum
    {
        public RobloxForum(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        internal Forum ToDbForum(RobloxForumGroup forumGroup)
        {
            List<RobloxForum> forums = new List<RobloxForum>() { this };
            return new Forum(Id, Name, forumGroup.ToDbForum(forums));
        }

        internal Forum ToDbForum()
        {
            return new Forum(Id, Name);
        }
    }
}
