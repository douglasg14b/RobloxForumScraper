using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RobloxScraper.Extensions
{
    public static class DbSetExtensions
    {
        public static void AddOrUpdate<T>(this DbSet<T> dbSet, T data) where T : class
        {
            var t = typeof(T);
            PropertyInfo keyField = null;
            foreach (var propt in t.GetProperties())
            {
                var keyAttr = propt.GetCustomAttribute<KeyAttribute>();
                if (keyAttr != null)
                {
                    keyField = propt;
                    break; // assume no composite keys
                }
            }
            if (keyField == null)
            {
                throw new Exception($"{t.FullName} does not have a KeyAttribute field. Unable to exec AddOrUpdate call.");
            }
            var keyVal = keyField.GetValue(data);
            var dbVal = dbSet.Find(keyVal);
            if (dbVal != null)
            {
                dbSet.Update(data);
                return;
            }
            dbSet.Add(data);
        }
    }
}
