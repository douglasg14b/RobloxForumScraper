using RobloxScraper.RobloxModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobloxScraper.Processing
{
    /// <summary>
    /// Thread and raw html of a page of that thread
    /// </summary>
    public class UnparsedPage
    {
        public UnparsedPage(RobloxThread thread, string html)
        {
            Thread = thread;
            Html = html;
        }

        /// <summary>
        /// Existing Thread
        /// </summary>
        public RobloxThread Thread { get; set; }

        /// <summary>
        /// Page Html
        /// </summary>
        public string Html { get; set; }
    }
}
