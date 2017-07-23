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
        public RobloxPost(HtmlNode postnode)
        {
            PostNode = postnode;
            ParsePost();
        }

        public HtmlNode PostNode { get; private set; }

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


        private void ParsePost()
        {
            HtmlNode userTable = PostNode.SelectSingleNode("td[1]/table");
            HtmlNode userNode = userTable.SelectSingleNode("tr[1]/td/a");


            HtmlNode bodyTable = PostNode.SelectSingleNode("td[2]/table");
            HtmlNode timestamp = bodyTable.SelectSingleNode("tr[1]");
            HtmlNode body = bodyTable.SelectSingleNode("tr[2]");

            User = ParseUser(userNode);
            Timestamp = ParseTimestamp(timestamp.SelectSingleNode("td/span").InnerText);
            PostBody = ParsePostBody(body.SelectSingleNode("td/span").InnerHtml);

        }

        private RobloxUser ParseUser(HtmlNode userNode)
        {
            string userLink = userNode.Attributes["href"].Value;
            MatchCollection matches = Regex.Matches(userLink, "[0-9]+", RegexOptions.IgnoreCase);
            int id = int.Parse(matches[0].Value);
            string name = userNode.InnerText;
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
