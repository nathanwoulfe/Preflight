using HtmlAgilityPack;
using Newtonsoft.Json;
using Preflight.Constants;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Web;
using Umbraco.Core;

namespace Preflight.Services
{
    public class LinksService : ILinksService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public List<BrokenLinkModel> CheckSafeBrowsing(string text, string apiKey)
        {
            List<BrokenLinkModel> response = new List<BrokenLinkModel>();

            if (!apiKey.HasValue()) return response;

            var doc = new HtmlDocument();
            doc.LoadHtml(text);

            HtmlNodeCollection links = doc.DocumentNode.SelectNodes(KnownStrings.HrefXPath);

            if (links == null || !links.Any()) return response;

            string[] hrefs = links.Select(l => l.GetAttributeValue("href", string.Empty))
                .Where(l => l.StartsWith("http")).ToArray();

            // check for cached responses - avoids request when page is being resaved
            MemoryCache cache = MemoryCache.Default;
            List<BrokenLinkModel> fromCache = new List<BrokenLinkModel>();

            foreach (string href in hrefs)
            {
                var cacheItem = (BrokenLinkModel)cache.Get(KnownStrings.CacheKey + href);
                if (null == cacheItem) continue;

                fromCache.Add(cacheItem);
                hrefs = hrefs.Except(href.AsEnumerableOfOne()).ToArray();
            }


            SafeBrowsingResponseModel safeBrowsingResult = SafeBrowsingLookup(hrefs, apiKey);

            if (safeBrowsingResult.Matches.Any())
            {
                response.AddRange(safeBrowsingResult.Matches.Select(m => new BrokenLinkModel
                {
                    Href = m.Threat.Url,
                    Status = m.ThreatType,
                    Unsafe = true,
                    Text = links.First(l => l.GetAttributeValue("href", string.Empty) == m.Threat.Url)
                        .InnerText
                }));

                foreach (BrokenLinkModel item in response)
                {
                    cache.Add(KnownStrings.CacheKey + item.Href, item,
                        new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(3600) });
                }
            }

            // add cached results
            response.AddRange(fromCache);

            return response;
        }

        /// <summary>
        /// Return a list of broken links (href and link text)
        /// Checks internal links by node, fails relative internal links, checks external links
        /// </summary>
        /// <param name="text"></param>
        /// <param name="safeBrowsingResult"></param>
        public List<BrokenLinkModel> Check(string text, List<BrokenLinkModel> safeBrowsingResult)
        {
            List<BrokenLinkModel> response = new List<BrokenLinkModel>();

            var doc = new HtmlDocument();
            doc.LoadHtml(text);

            HtmlNodeCollection links = doc.DocumentNode.SelectNodes(KnownStrings.HrefXPath);

            if (links == null || !links.Any()) return response;

            // filter out safebrowsing results if any exist
            IEnumerable<HtmlNode> linksToCheck = links.ToList();
            if (safeBrowsingResult != null && safeBrowsingResult.Any())
            {
                linksToCheck = links.Where(l =>
                    safeBrowsingResult.All(m =>
                        m.Href != l.GetAttributeValue("href", string.Empty)));
            }

            if (!linksToCheck.Any()) return response;

            foreach (HtmlNode link in linksToCheck)
            {
                string href = link.GetAttributeValue("href", string.Empty);

                var responseItem = new BrokenLinkModel
                {
                    Href = href,
                    Text = link.InnerText
                };

                if (href.HasValue())
                {
                    if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
                    {
                        var uri = new Uri(href);

                        if (uri.Host == HttpContext.Current.Request.Url.Host)
                        {
                            responseItem.Status = "Invalid internal link";
                        }
                        else
                        {
                            HttpStatusCode status = GetHeaders(href);

                            if (status != HttpStatusCode.OK)
                            {
                                responseItem.Status = $"Broken/unavailable ({(int)status})";
                            }
                        }
                    }
                    else if (Uri.IsWellFormedUriString(href, UriKind.Relative))
                    {
                        responseItem.Status = "Invalid internal link";
                    }
                }
                else
                {
                    responseItem.Status = "Href not set";
                }

                // if status is null, the link is ok
                if (responseItem.Status.HasValue())
                {
                    response.Add(responseItem);
                }
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="apiKey"></param>
        private static SafeBrowsingResponseModel SafeBrowsingLookup(IEnumerable<string> urls, string apiKey)
        {
            try
            {
                string result;
                string url = KnownStrings.SafeBrowsingUrl + apiKey;

                ThreatEntry[] threatEntries = urls.Select(u => new ThreatEntry { Url = u }).ToArray();

                var requestModel = new SafeBrowsingRequestModel
                {
                    Client = new Client
                    {
                        ClientId = "preflight-test",
                        ClientVersion = "0.0.0"
                    },
                    ThreatInfo = new ThreatInfo
                    {
                        ThreatTypes = new[] { "MALWARE", "SOCIAL_ENGINEERING" },
                        PlatformTypes = new[] { "WINDOWS" },
                        ThreatEntryTypes = new[] { "URL" },
                        ThreatEntries = threatEntries
                    }
                };

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    result = client.UploadString(url, "POST", JsonConvert.SerializeObject(requestModel));
                }

                return JsonConvert.DeserializeObject<SafeBrowsingResponseModel>(result);
            }
            catch (Exception)
            {
                return new SafeBrowsingResponseModel();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static HttpStatusCode GetHeaders(string url)
        {
            HttpStatusCode result = default(HttpStatusCode);
            WebRequest request = WebRequest.Create(url);

            request.Method = WebRequestMethods.Http.Head;

            try
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response != null)
                    {
                        result = response.StatusCode;
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse resp)
                {
                    result = resp.StatusCode;
                }
                else
                {
                    result = HttpStatusCode.NotFound;
                }
            }

            return result;
        }
    }
}
