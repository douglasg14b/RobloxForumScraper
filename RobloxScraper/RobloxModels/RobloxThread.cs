﻿using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using RobloxScraper.DbModels;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public string Errors { get; set; } = "";
        public RobloxForum Forum { get; set; }
        public RobloxForumGroup ForumGroup { get; set; }
        public List<RobloxPage> Pages { get; set; }


        public void AddPage(string html)
        {
            html = html.Replace('\uFFFF', ' ');
            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.Parse(html);
            RobloxPage page = new RobloxPage(document, this);
            if (page.PageNumber == CurrentPage)
            {
                try
                {
                    if (CurrentPage == 0)
                    {
                        AddFirstPage(document, page);
                    }
                    else
                    {
                        Pages.Add(page);
                        CurrentPage++;
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message + " In Thread # " + ThreadId);
                    throw new Exception(ex.Message + " In Thread # " + ThreadId);
                }

            }
            else
            {
                throw new Exception("Unexpected page number");
            }
        }



        //First page add, performs some additional stuff
        public void AddFirstPage(IHtmlDocument document, RobloxPage page)
        {
            if (!page.IsEmpty)
            {
                ParseForum(document);
                ParseTitle(document);
                ParseNumberOfPages(document);
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
            forum.ForumGroupId = forumGroup.Id;
            forumGroup.Forums = new List<Forum>() { forum };

            /* Posts */

            List<Post> posts = new List<Post>();
            foreach(RobloxPage page in Pages)
            {
                posts.AddRange(page.ToDbPosts());
            }

            Thread dbThread = new Thread(ThreadId, Title, Errors, forum, posts);
            foreach(Post post in posts)
            {
                post.Thread = dbThread;
            }
            return dbThread;
        }

        private void ParseForum(IHtmlDocument document)
        {
            IHtmlCollection<IElement> forumsElements = document.GetElementById("ctl00_cphRoblox_PostView1_ctl00_Whereami1").QuerySelectorAll("nobr > a");

            string forumUrl = forumsElements[forumsElements.Length - 1].Attributes["href"].Value;
            string formName = forumsElements[forumsElements.Length - 1].TextContent;
            MatchCollection forumMatches = Regex.Matches(forumUrl, "[0-9]+", RegexOptions.IgnoreCase);

            string forumGroupUrl;         
            string formGroupName;
            int forumGroupId;
            MatchCollection forumGroupMatches;

            if (forumsElements.Length == 2)
            {
                formGroupName = "ROBLOX Forum";
                forumGroupId = 0;
            }
            else
            {
                forumGroupUrl = forumsElements[1].Attributes["href"].Value;
                forumGroupMatches = Regex.Matches(forumGroupUrl, "[0-9]+", RegexOptions.IgnoreCase);
                formGroupName = forumsElements[1].TextContent;
                forumGroupId = int.Parse(forumGroupMatches[0].Value);
            }

            Forum = new RobloxForum(forumGroupId, formName);
            ForumGroup = new RobloxForumGroup(int.Parse(forumMatches[0].Value), formGroupName);
        }

        private void ParseTitle(IHtmlDocument document)
        {
            Title = document.GetElementById("ctl00_cphRoblox_PostView1_ctl00_PostTitle").TextContent;
        }



        //Parses out the total number of pages in this post
        private void ParseNumberOfPages(IHtmlDocument document)
        {
            IElement pager = document.GetElementById("ctl00_cphRoblox_PostView1_ctl00_Pager").QuerySelector("table tr").QuerySelector("td").QuerySelector("span");               
            string pageText = pager.TextContent;

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
