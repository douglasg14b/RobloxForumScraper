using AngleSharp.Dom;
using HtmlAgilityPack;
using RobloxScraper.DbModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RobloxScraper.RobloxModels
{
    public class RobloxPost
    {
        public RobloxPost(IElement postnode)
        {
            ParsePost(postnode);
        }

        public RobloxUser User { get; private set; }
        public string PostBody { get; private set; }
        public DateTime Timestamp { get; set; }

        public Post ToDbPost()
        {
            return new Post(Timestamp, PostBody, User.ToDbUser());
        }

        public Post ToDbPost(RobloxThread thread)
        {
            return new Post(Timestamp, PostBody, User.ToDbUser(), thread.ToDbThread());
        }


        private void ParsePost(IElement postnode)
        {
            IHtmlCollection<IElement> tables = postnode.QuerySelectorAll("table");
            IElement userNode = tables[0].QuerySelector("tr:nth-child(1) td a");


            IElement timestamp = tables[1].QuerySelector("tr:nth-child(1)");
            IElement body = tables[1].QuerySelector("tr:nth-child(2)");

            User = ParseUser(userNode);
            Timestamp = ParseTimestamp(timestamp.QuerySelector("td span").TextContent);
            PostBody = ParsePostBody(body.QuerySelector("td span").InnerHtml);
        }

        private RobloxUser ParseUser(IElement userNode)
        {
            string userLink = userNode.Attributes["href"].Value;
            MatchCollection matches = Regex.Matches(userLink, "[0-9]+", RegexOptions.IgnoreCase);
            int id = int.Parse(matches[0].Value);
            string name = userNode.TextContent;
            return new RobloxUser(name, id);
        }

        private DateTime ParseTimestamp(string value)
        {
            return DateTime.Parse(value);
        }

        private string ParsePostBody(string value)
        {
            value = value.Replace("<br>", "\r\n");
            return value;
        }


    }
}
