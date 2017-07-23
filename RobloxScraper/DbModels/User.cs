using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RobloxScraper.DbModels
{
    public class User
    {
        public User() { }
        public User(int id, string name)
        {
            Id = id;
            Name = name;
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
    }
}
