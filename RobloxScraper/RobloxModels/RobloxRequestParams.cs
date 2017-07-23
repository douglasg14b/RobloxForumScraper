using System;
using System.Collections.Generic;
using System.Text;

namespace RobloxScraper.RobloxModels
{
    public class RobloxRequestParams
    {
        public RobloxRequestParams(string eventargument, string viewstate, string viewstategenerator, string eventvalidation)
        {
            EventArgument = eventargument;
            ViewState = viewstate;
            ViewStateGenerator = viewstategenerator;
            EventValidation = eventvalidation;
        }

        public string EventArgument { get; set; }
        public string ViewState { get; set; }
        public string ViewStateGenerator { get; set; }
        public string EventValidation { get; set; }
    }
}
