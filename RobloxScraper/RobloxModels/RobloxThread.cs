using HtmlAgilityPack;
using RobloxScraper.DbModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RobloxScraper.RobloxModels
{
    public class RobloxThread
    {
        public RobloxThread(int id)
        {
            ThreadId = id;

            Pages = new List<RobloxPage>();
        }

        public bool IsEmpty { get; set; } = false;

        public int ThreadId { get; set; }
        public string Title { get; set; }
        public int PagesCount { get; set; }
        public int CurrentPage { get; set; } = 0;  //Last page that was added, rename?
        public RobloxForum Forum { get; set; }
        public RobloxForumGroup ForumGroup { get; set; }
        public List<RobloxPage> Pages { get; set; }


        public void AddPage(string html)
        {
            RobloxPage page = new RobloxPage(html);
            if(page.PageNumber == CurrentPage)
            {
                if(CurrentPage == 0)
                {
                    AddFirstPage(page);
                }
                else
                {
                    Pages.Add(page);
                    CurrentPage++;
                }
            }
            else
            {
                throw new Exception("Unexpected page number");
            }

        }



        //First page add, performsn some adiditon stuff
        public void AddFirstPage(RobloxPage page)
        {
            if (!page.IsEmpty)
            {
                ParseForum(page.Document);
                ParseTitle(page.Document);
                ParseNumberOfPages(page.Document);
                Pages.Add(page);
                CurrentPage++;
            }
            else
            {
                IsEmpty = true;
            }

        }

        //Gets the params of the current page to be used for the next pages request
        public RobloxRequestParams GetNextPageParams()
        {
            return Pages[CurrentPage - 1].Params;
        }

        public Thread ToDbThread()
        {

            /* Forums */
            ForumGroup forumGroup = ForumGroup.ToDbForum();
            Forum forum = Forum.ToDbForum();

            forum.ForumGroup = forumGroup;
            forumGroup.Forums = new List<Forum>() { forum };

            /* Posts */

            List<Post> posts = new List<Post>();
            foreach(RobloxPage page in Pages)
            {
                posts.AddRange(page.ToDbPosts());
            }

            Thread dbThread = new Thread(ThreadId, Title, forum, posts);
            foreach(Post post in posts)
            {
                post.Thread = dbThread;
            }
            return dbThread;
        }

        private void ParseForum(HtmlDocument document)
        {
            HtmlNode forumsNode = document.GetElementbyId("ctl00_cphRoblox_PostView1_ctl00_Whereami1");
            HtmlNode forumGroupNode = forumsNode.SelectSingleNode("div/nobr[2]/a");
            HtmlNode forumNode = forumsNode.SelectSingleNode("div/nobr[3]/a");

            string forumGroupUrl = forumGroupNode.Attributes["href"].Value;
            string forumUrl = forumNode.Attributes["href"].Value;

            MatchCollection forumGroupMatches = Regex.Matches(forumGroupUrl, "[0-9]+", RegexOptions.IgnoreCase);
            MatchCollection forumMatches = Regex.Matches(forumUrl, "[0-9]+", RegexOptions.IgnoreCase);

            string formGroupName = forumGroupNode.InnerText;
            string formName = forumNode.InnerText;

            Forum = new RobloxForum(int.Parse(forumGroupMatches[0].Value), formName);
            ForumGroup = new RobloxForumGroup(int.Parse(forumMatches[0].Value), formGroupName);
        }

        private void ParseTitle(HtmlDocument document)
        {
            Title = document.GetElementbyId("ctl00_cphRoblox_PostView1_ctl00_PostTitle").InnerText;
        }



        //Parses out the total number of pages in this post
        private void ParseNumberOfPages(HtmlDocument document)
        {
            HtmlNode pager = document.DocumentNode.SelectSingleNode("//*[@id='ctl00_cphRoblox_PostView1_ctl00_Pager']");
            string pageText = pager.SelectSingleNode("table/tr[1]/td[1]/span").InnerText;

            MatchCollection matches = Regex.Matches(pageText, "[0-9]+", RegexOptions.IgnoreCase);

            int current = int.Parse(matches[0].Value) - 1;
            if(current != 0)
            {
                throw new Exception($"Expected first page, got {current} instead");
            }
            CurrentPage = current;
            PagesCount = int.Parse(matches[1].Value);
        }
    }
}
