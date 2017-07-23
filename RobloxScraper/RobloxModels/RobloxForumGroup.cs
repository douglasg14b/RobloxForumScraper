using System;
using System.Collections.Generic;
using System.Text;
using RobloxScraper.DbModels;

namespace RobloxScraper.RobloxModels
{
    public class RobloxForumGroup
    {
        public RobloxForumGroup(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        internal ForumGroup ToDbForum()
        {
            return new ForumGroup(Id, Name);
        }

        internal ForumGroup ToDbForum(List<RobloxForum> forums)
        {
            List<Forum> dbForums = new List<Forum>();
            foreach(RobloxForum forum in forums)
            {
                dbForums.Add(forum.ToDbForum(this));
            }
            return new ForumGroup(Id, Name, dbForums);
        }
    }
}
