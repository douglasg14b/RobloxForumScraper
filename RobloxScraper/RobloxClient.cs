using RobloxScraper.RobloxModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RobloxScraper
{
    class RobloxClient : HttpClient
    {
        public RobloxClient(HttpClientHandler handler)
            :base(handler){}

        private string baseUrl = "https://forum.roblox.com/Forum/ShowPost.aspx";


        public async Task<string> GetThread(int id)
        {
            Uri uri = BuildPageUri(id);
            HttpRequestMessage request = CreateRequest(HttpMethod.Post, uri);
            HttpResponseMessage response = await SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                if(response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    return String.Empty;
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetThread(int id, int page, RobloxRequestParams parameters)
        {
            Uri uri = BuildPageUri(id);
            List<KeyValuePair<string, string>> formArgs = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("__EVENTTARGET", $"ctl00$cphRoblox$PostView1$ctl00$Pager$Page{page}"),
                new KeyValuePair<string, string>("__VIEWSTATE", parameters.ViewState),
                new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", parameters.ViewStateGenerator),
                new KeyValuePair<string, string>("__EVENTVALIDATION", parameters.EventValidation),
                new KeyValuePair<string, string>("__EVENTARGUMENT", parameters.EventArgument)
            };
            HttpRequestMessage request = CreateRequest(HttpMethod.Post, uri, formArgs);
            HttpResponseMessage response = await SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    return String.Empty;
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                }
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetThread(int id, int page, string viewState, string viewStateGenerator, string eventValidation, string eventArgument)
        {
            try
            {
                Uri uri = BuildPageUri(id);
                List<KeyValuePair<string, string>> formArgs = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("__EVENTTARGET", $"ctl00$cphRoblox$PostView1$ctl00$Pager$Page{page}"),
                    new KeyValuePair<string, string>("__VIEWSTATE", viewState),
                    new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", viewStateGenerator),
                    new KeyValuePair<string, string>("__EVENTVALIDATION", eventValidation),
                    new KeyValuePair<string, string>("__EVENTARGUMENT", eventArgument)
                };
                HttpRequestMessage request = CreateRequest(HttpMethod.Post, uri, formArgs);
                HttpResponseMessage response = await SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

        public Uri BuildPageUri(int id)
        {
            return new Uri(baseUrl + $"?PostID={id}");
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, Uri uri)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, uri);
            return request;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, Uri uri, List<KeyValuePair<string, string>> content)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, uri);
            request.Content = new FormUrlEncodedContent(content);
            return request;
        }
    }
}
