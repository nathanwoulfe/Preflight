using System.Net;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Preflight.Extensions;
using Preflight.Plugins.LinkHealth.Models;
using Preflight.Services;
using Umbraco.Extensions;

namespace Preflight.Plugins.LinkHealth.Services;

public class SafeBrowsingService : ISafeBrowsingService
{
    private readonly ICacheManager _cacheManager;
    private const string SafeBrowsingUrl = "https://safebrowsing.googleapis.com/v4/threatMatches:find?key=";
    private const string CacheKey = "Preflight_SafeBrowsing_";

    public SafeBrowsingService(ICacheManager cacheManager) => _cacheManager = cacheManager;

    /// <summary>
    ///
    /// </summary>
    /// <param name="text"></param>
    /// <param name="apiKey"></param>
    /// <returns></returns>
    public List<BrokenLinkModel> Check(string text, string apiKey)
    {
        List<BrokenLinkModel> response = [];

        if (!apiKey.HasValue())
        {
            return response;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(text);

        HtmlNodeCollection links = doc.DocumentNode.SelectNodes(KnownStrings.HrefXPath);

        if (links == null || !links.Any())
        {
            return response;
        }

        string[] hrefs = links.Select(l => l.GetAttributeValue("href", string.Empty))
            .Where(l => l.StartsWith("http")).ToArray();

        // check for cached responses - avoids request when page is being resaved
        List<BrokenLinkModel> fromCache = [];

        foreach (string href in hrefs)
        {
            if (!_cacheManager.TryGet(CacheKey + href, out BrokenLinkModel cacheItem))
            {
                continue;
            }

            fromCache.Add(cacheItem);
            hrefs = hrefs.Except(new[] { href }).ToArray();
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
                    .InnerText,
            }));

            foreach (BrokenLinkModel item in response)
            {
                _cacheManager.Set(CacheKey + item.Href, item);
            }
        }

        // add cached results
        response.AddRange(fromCache);

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
            string url = SafeBrowsingUrl + apiKey;

            ThreatEntryModel[] threatEntries = urls.Select(u => new ThreatEntryModel { Url = u }).ToArray();

            var requestModel = new SafeBrowsingRequestModel
            {
                Client = new ClientModel
                {
                    ClientId = "preflight-test",
                    ClientVersion = "0.0.0",
                },
                ThreatInfo = new ThreatInfoModel
                {
                    ThreatTypes = new[] { "MALWARE", "SOCIAL_ENGINEERING" },
                    PlatformTypes = new[] { "ANY_PLATFORM" },
                    ThreatEntryTypes = new[] { "URL" },
                    ThreatEntries = threatEntries,
                },
            };

            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                result = client.UploadString(url, "POST", JsonConvert.SerializeObject(requestModel));
            }

            return JsonConvert.DeserializeObject<SafeBrowsingResponseModel>(result) ?? new();
        }
        catch (Exception)
        {
            return new SafeBrowsingResponseModel();
        }
    }
}
