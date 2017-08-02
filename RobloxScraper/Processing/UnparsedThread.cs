using System;
using System.Collections.Generic;
using System.Text;

namespace RobloxScraper.Processing
{
    public class UnparsedThread
    {
        public UnparsedThread(int id, string html)
        {
            Id = id;
            Html = html;
        }

        /// <summary>
        /// Thread Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Thread Html
        /// </summary>
        public string Html { get; set; }
    }
}
