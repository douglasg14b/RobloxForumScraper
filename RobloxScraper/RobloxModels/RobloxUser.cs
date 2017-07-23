using RobloxScraper.DbModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobloxScraper.RobloxModels
{
    public class RobloxUser
    {
        public RobloxUser(string name, int id)
        {
            Name = name;
            Id = id;
        }

        public User ToDbUser()
        {
            return new User(Id, Name);
        }

        public string Name { get; set; }
        public int Id { get; set; }
    }
}
