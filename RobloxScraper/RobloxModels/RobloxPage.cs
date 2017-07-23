using HtmlAgilityPack;
using RobloxScraper.DbModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RobloxScraper.RobloxModels
{
    public class RobloxPage
    {
        public RobloxPage(string html)
        {
            Document = new HtmlDocument();
            Document.LoadHtml(html);

            if (PageExists())
            {
                IsEmpty = false;
                Posts = new List<RobloxPost>();
                ParseCurrentPageNumber();
                ParseRequestInputParams();
                ParsePosts();
                return;
            }
            IsEmpty = true;
        }

        public HtmlDocument Document { get; private set; }
        public RobloxRequestParams Params { get; set; }
        public int PageNumber { get; set; }
        public bool IsEmpty { get; set; }
        public List<RobloxPost> Posts { get; set; }

        internal List<Post> ToDbPosts()
        {
            List<Post> posts = new List<Post>();
            foreach (RobloxPost post in Posts)
            {
                posts.Add(post.ToDbPost());
            }
            return posts;
        }

        internal List<Post> ToDbPosts(RobloxThread thread)
        {
            List<Post> posts = new List<Post>();
            foreach(RobloxPost post in Posts)
            {
                posts.Add(post.ToDbPost(thread));
            }
            return posts;
        }

        private bool PageExists()
        {
            HtmlNode node = Document.GetElementbyId("ctl00_cphRoblox_Message1_ctl00_MessageTitle");
            if(node != null)
            {
                string message = node.InnerText;
                if (message.ToLower().Contains("error"))
                {
                    return false;
                }
            }
            return true;
        }

        //Parses out the total number of pages in this post
        private void ParseCurrentPageNumber()
        {
            HtmlNode pager = Document.DocumentNode.SelectSingleNode("//*[@id='ctl00_cphRoblox_PostView1_ctl00_Pager']");
            string pageText = pager.SelectSingleNode("table/tr[1]/td[1]/span").InnerText;

            MatchCollection matches = Regex.Matches(pageText, "[0-9]+", RegexOptions.IgnoreCase);

            PageNumber = int.Parse(matches[0].Value) - 1;
        }

        //Parses the __VIEWSTATE, __VIEWSTATEGENERATOR, __EVENTVALIDATION, and __EVENTARGUMENT
        private void ParseRequestInputParams()
        {
            string eventArgument = "";
            string viewState = Document.GetElementbyId("__VIEWSTATE").Attributes["value"].Value;
            string viewStateGenerator = Document.GetElementbyId("__VIEWSTATEGENERATOR").Attributes["value"].Value;
            string eventValidation = Document.GetElementbyId("__EVENTVALIDATION").Attributes["value"].Value;

            HtmlNode eventArgumentNode = Document.GetElementbyId("__EVENTTARGET");

            if (eventArgumentNode != null)
            {
                eventArgument = eventArgumentNode.Attributes["value"].Value;
            }

            Params = new RobloxRequestParams(eventArgument, viewState, viewStateGenerator, eventValidation);
        }

        private void ParsePosts()
        {
            HtmlNodeCollection postNodes = Document.DocumentNode.SelectNodes("//tr[@class='forum-post']");
            foreach (HtmlNode node in postNodes)
            {
                RobloxPost post = new RobloxPost(node);
                Posts.Add(post);
            }
        }
    }
}
