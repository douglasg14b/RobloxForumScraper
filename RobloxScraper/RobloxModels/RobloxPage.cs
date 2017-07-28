using AngleSharp.Dom;
using AngleSharp.Dom.Html;
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
        int thread_id;
        public RobloxPage() { IsEmpty = true; }
        public RobloxPage(IHtmlDocument document, RobloxThread thread)
        {
            thread_id = thread.ThreadId;

            if (PageExists(document))
            {
                IsEmpty = false;
                Posts = new List<RobloxPost>();
                ParseCurrentPageNumber(document);
                ParseRequestInputParams(document);
                ParsePosts(document);
                return;
            }
            IsEmpty = true;
        }

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

        private bool PageExists(IHtmlDocument document)
        {
            IElement node = document.GetElementById("ctl00_cphRoblox_Message1_ctl00_MessageTitle");
            if(node != null)
            {
                string message = node.TextContent;
                if (message.ToLower().Contains("error"))
                {
                    return false;
                }
            }
            return true;
        }

        //Parses out the total number of pages in this post
        private void ParseCurrentPageNumber(IHtmlDocument document)
        {
            IElement pager = document.GetElementById("ctl00_cphRoblox_PostView1_ctl00_Pager");
            if (pager == null)
            {
                throw new Exception($"No pager for Page in Thread: {thread_id}");
            }
            string pageText = pager.QuerySelector("table tr:nth-child(1) td:nth-child(1) span").TextContent;

            MatchCollection matches = Regex.Matches(pageText, "[0-9]+", RegexOptions.IgnoreCase);

            PageNumber = int.Parse(matches[0].Value) - 1;
        }

        //Parses the __VIEWSTATE, __VIEWSTATEGENERATOR, __EVENTVALIDATION, and __EVENTARGUMENT
        private void ParseRequestInputParams(IHtmlDocument document)
        {
            string eventArgument = "";
            string viewState = document.GetElementById("__VIEWSTATE").Attributes["value"].Value;
            string viewStateGenerator = document.GetElementById("__VIEWSTATEGENERATOR").Attributes["value"].Value;
            string eventValidation = document.GetElementById("__EVENTVALIDATION").Attributes["value"].Value;

            IElement eventArgumentNode = document.GetElementById("__EVENTTARGET");

            if (eventArgumentNode != null)
            {
                eventArgument = eventArgumentNode.Attributes["value"].Value;
            }

            Params = new RobloxRequestParams(eventArgument, viewState, viewStateGenerator, eventValidation);
        }

        private void ParsePosts(IHtmlDocument document)
        {
            IHtmlCollection<IElement> posts = document.QuerySelectorAll("tr.forum-post");
            //HtmlNodeCollection postNodes = document.DocumentNode.SelectNodes("//tr[@class='forum-post']");
            foreach (IElement node in posts)
            {
                RobloxPost post = new RobloxPost(node);
                Posts.Add(post);
            }
        }
    }
}
