using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Security;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Preflight.Models;
using Preflight.Services.Interfaces;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Preflight.Services
{
    public class LinksService : ILinksService
    {
        /// <summary>
        /// Return a list of broken links (href and link text)
        /// Checks internal links by node, fails relative internal links, checks external links
        /// </summary>
        /// <param name="text"></param>
        /// <param name="checkLinks"></param>
        /// <param name="checkSafeBrowsing"></param>
        public List<BrokenLinkModel> Check(string text, bool checkLinks = true, bool checkSafeBrowsing = true, string apiKey = null)
        {
            List<BrokenLinkModel> response = new List<BrokenLinkModel>();

            if (checkLinks == false && checkSafeBrowsing == false) return response;

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(text);

                HtmlNodeCollection links = doc.DocumentNode.SelectNodes(Constants.HrefXPath);
                SafeBrowsingResponseModel safeBrowsingResult = null;

                if (links != null && links.Any())
                {
                    if (checkSafeBrowsing)
                    {
                        string[] hrefs = links.Select(l => l.GetAttributeValue("href", string.Empty))
                            .Where(l => l.StartsWith("http")).ToArray();

                        // check for cached responses - avoids request when page is being resaved
                        MemoryCache cache = MemoryCache.Default;
                        List<BrokenLinkModel> fromCache = new List<BrokenLinkModel>();

                        foreach (string href in hrefs)
                        {
                            var cacheItem = (BrokenLinkModel) cache.Get(Constants.CacheKey + href);
                            if (null == cacheItem) continue;

                            fromCache.Add(cacheItem);
                            hrefs = hrefs.Except(href.AsEnumerableOfOne()).ToArray();
                        }

                        if (!string.IsNullOrEmpty(apiKey))
                        {
                            safeBrowsingResult = SafeBrowsingLookup(hrefs, apiKey);

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
                                    cache.Add(Constants.CacheKey + item.Href, item,
                                        new CacheItemPolicy {AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(3600)});
                                }
                            }
                        }

                        // add cached results
                        response.AddRange(fromCache);
                    }

                    if (checkLinks)
                    {
                        // only check links marked as safe - check all if no result from lookup
                        IEnumerable<HtmlNode> linksToCheck = links.ToList();
                        if (safeBrowsingResult != null)
                        {
                            linksToCheck = !safeBrowsingResult.Matches.Any()
                                ? links.ToList()
                                : links.Where(l =>
                                    safeBrowsingResult.Matches.All(m =>
                                        m.Threat.Url != l.GetAttributeValue("href", string.Empty)));
                        }

                        if (linksToCheck.Any())
                        {
                            foreach (HtmlNode link in linksToCheck)
                            {
                                string href = link.GetAttributeValue("href", string.Empty);
                                string linkText = link.InnerText;

                                if (!string.IsNullOrEmpty(href) && Uri.IsWellFormedUriString(href, UriKind.Absolute))
                                {
                                    var uri = new Uri(href);

                                    if (uri.Host == HttpContext.Current.Request.Url.Host)
                                    {
                                        response.Add(new BrokenLinkModel()
                                        {
                                            Href = href,
                                            Text = linkText,
                                            Status = "Invalid internal link"
                                        });
                                    }
                                    else
                                    {
                                        HttpStatusCode status = GetHeaders(href);

                                        if (status != HttpStatusCode.OK)
                                        {
                                            response.Add(new BrokenLinkModel()
                                            {
                                                Href = href,
                                                Text = linkText,
                                                Status = "Broken/unavailable (" + (int) status + ")"
                                            });
                                        }
                                    }
                                }
                                else if (string.IsNullOrEmpty(href) || Uri.IsWellFormedUriString(href, UriKind.Relative))
                                {
                                    // is a relative link - fail it.
                                    response.Add(new BrokenLinkModel()
                                    {
                                        Status = string.IsNullOrEmpty(href) ? "Href not set" : "Invalid internal link",
                                        Href = href,
                                        Text = linkText
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(GetType(), ex.Message, ex);
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
                string url = Constants.SafeBrowsingUrl + apiKey;

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
