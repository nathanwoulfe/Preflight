using HtmlAgilityPack;
using Preflight.Helpers;
using Preflight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using Newtonsoft.Json;
using Umbraco.Core;

namespace Preflight.Extensions
{
    public static class StringExtensions
    {
        private static readonly char[] Vowels = { 'a', 'e', 'i', 'o', 'u', 'y' };
        private static readonly char[] WordDelimiters = { '.', '!', '?', ':', ';' };
        private static readonly string[] Endings = { "es", "ed" };

        private static readonly string CacheKey = "Preflight_SafeBrowsing_";

        /// <summary>
        /// the formula is: RE = 206.835 – (1.015 x ASL) – (84.6 x ASW) 
        /// where RE = the readability ease
        /// and ASL = average sentence length
        /// and ASW = average syllables per word
        ///
        /// aiming for a score of 60-70
        ///
        /// /li, /ol, closing headings should be treated as sentences
        /// strip all HTML tags
        /// split on delimiters to count sentences
        /// split on spaces to count words                   
        /// count worth length 
        /// split on vowels to count syllables and adjust for endings etc
        ///
        /// three or fewer letters are single syllable
        /// </summary>
        /// <param name="text">The string to parse</param>
        /// <returns></returns>
        public static ReadabilityResponseModel GetReadability(this string text)
        {
            var settings = SettingsHelper.GetSettings();
            var longWordLength = Convert.ToInt32(settings.First(s => s.Alias == KnownSettingAlias.LongWordSyllables).Value);
            var readabilityMin = Convert.ToInt32(settings.First(s => s.Alias == KnownSettingAlias.ReadabilityMin).Value);
            var readabilityMax = Convert.ToInt32(settings.First(s => s.Alias == KnownSettingAlias.ReadabilityMax).Value);

            var whitelist = ((string)settings.First(s => s.Alias == KnownSettingAlias.Whitelist).Value).Split(',');
            var blacklist = ((string)settings.First(s => s.Alias == KnownSettingAlias.Blacklist).Value).Split(',');

            // Words longer than three syllables
            var longWords = new List<string>();
            var blacklisted = new List<string>();

            text = Regex.Replace(text, MagicStrings.ClosingHtmlTags, ".");
            text = text.Replace(MagicStrings.NewLine, " ");
            text = Regex.Replace(text, MagicStrings.CharsToRemove, "").Replace("&amp;", "&");
            text = Regex.Replace(text, MagicStrings.DuplicateSpaces, " ");

            var sentences = text.Split(WordDelimiters).Where(s => !string.IsNullOrEmpty(s) && s.Length > 3).ToList();

            // calc words/sentence
            double totalWords = 0;
            double totalSyllables = 0;
            foreach (var sentence in sentences)
            {
                var words = sentence.Trim().Split(' ').Where(s => !string.IsNullOrEmpty(s) && !whitelist.Contains(s, StringComparer.OrdinalIgnoreCase)).ToList();
                totalWords += words.Count;

                foreach (var word in words)
                {
                    var syllableCount = word.CountSyllables();
                    if (syllableCount >= longWordLength && !longWords.Contains(word, StringComparer.OrdinalIgnoreCase))
                    {
                        longWords.Add(word);
                    }

                    if (blacklist.Contains(word, StringComparer.OrdinalIgnoreCase))
                    {
                        blacklisted.Add(word);
                    }

                    totalSyllables += syllableCount;
                }
            }

            var asl = totalWords / sentences.Count;
            var asw = totalSyllables / totalWords;

            var score = Math.Round(206.835 - (1.015 * asl) - (84.6 * asw), 0);

            return new ReadabilityResponseModel()
            {
                Score = score,
                AverageSyllables = Math.Round(asw, 1),
                SentenceLength = Math.Round(asl, 1),
                LongWords = longWords.OrderBy(l => l).ToList(),
                Blacklist = blacklisted.OrderBy(l => l).ToList(),
                Failed = score < readabilityMin || score > readabilityMax || blacklisted.Any()
            };
        }

        /// <summary>
        /// Return a list of broken links (href and link text)
        /// Checks internal links by node, fails relative internal links, checks external links
        /// </summary>
        /// <param name="text"></param>
        public static List<BrokenLinkModel> CheckLinks(this string text)
        {

            var response = new List<BrokenLinkModel>();

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(text);

                var links = doc.DocumentNode.SelectNodes(MagicStrings.HrefXPath);

                if (links != null && links.Any())
                {
                    var hrefs = links.Select(l => l.GetAttributeValue("href", string.Empty)).Where(l => l.StartsWith("http"));

                    // check for cached responses - avoids request when page is being resaved
                    var cache = MemoryCache.Default;
                    var fromCache = new List<BrokenLinkModel>();

                    foreach (var href in hrefs)
                    {
                        var cacheItem = (BrokenLinkModel) cache.Get(CacheKey + href);
                        if (null == cacheItem) continue;

                        fromCache.Add(cacheItem);
                        hrefs = hrefs.Except(href.AsEnumerableOfOne());
                    }

                    var settings = SettingsHelper.GetSettings();
                    var apiKey = settings.First(s => s.Alias == KnownSettingAlias.GoogleApiKey).Value.ToString();
                    SafeBrowsingResponseModel safeBrowsingResult = null;

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
                                Text = links.First(l => l.GetAttributeValue("href", string.Empty) == m.Threat.Url).InnerText
                            }));

                            foreach (var item in response)
                            {
                                cache.Add(CacheKey + item.Href, item, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(3600) });
                            }
                        }
                    }

                    // add cached results
                    response.AddRange(fromCache);

                    // only check links marked as safe - check all if no result from lookup
                    var safeLinks = !safeBrowsingResult.Matches.Any() ?
                        links.ToList() :
                        links.Where(l => safeBrowsingResult.Matches.All(m => m.Threat.Url != l.GetAttributeValue("href", string.Empty)));

                    foreach (var link in safeLinks)
                    {
                        var href = link.GetAttributeValue("href", string.Empty);
                        var linkText = link.InnerText;

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
                                var status = GetHeaders(href);

                                if (status != HttpStatusCode.OK)
                                {
                                    response.Add(new BrokenLinkModel()
                                    {
                                        Href = href,
                                        Text = linkText,
                                        Status = "Broken/unavailable (" + (int)status + ")"
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
            catch (Exception ex)
            {
                var e = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urls"></param>
        private static SafeBrowsingResponseModel SafeBrowsingLookup(IEnumerable<string> urls, string apiKey)
        {
            try
            {
                var result = "";
                var url = "https://safebrowsing.googleapis.com/v4/threatMatches:find?key=" + apiKey;

                var threatEntries = urls.Select(u => new ThreatEntry {Url = u}).ToArray();

                var requestModel = new SafeBrowsingRequestModel
                {
                    Client = new Client
                    {
                        ClientId = "preflight-test",
                        ClientVersion = "0.0.0"
                    },
                    ThreatInfo = new ThreatInfo
                    {
                        ThreatTypes = new [] {"MALWARE", "SOCIAL_ENGINEERING" },
                        PlatformTypes = new [] { "WINDOWS" },
                        ThreatEntryTypes = new [] { "URL" },
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
            var result = default(HttpStatusCode);
            var request = WebRequest.Create(url);
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
                var resp = ex.Response as HttpWebResponse;
                if (resp != null)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private static int CountSyllables(this string word)
        {
            var currentWord = word;
            var numVowels = 0;
            var lastWasVowel = false;

            if (currentWord.Length <= 3)
            {
                return 1;
            }

            foreach (var wc in currentWord.ToLower())
            {

                var foundVowel = false;
                foreach (var v in Vowels)
                {
                    //don't count diphthongs
                    if (v == wc && lastWasVowel)
                    {
                        foundVowel = true;
                        break;
                    }

                    if (v != wc || lastWasVowel)
                    {
                        continue;
                    }

                    numVowels++;
                    foundVowel = true;
                    lastWasVowel = true;
                    break;
                }

                //if full cycle and no vowel found, set lastWasVowel to false;
                if (!foundVowel)
                {
                    lastWasVowel = false;
                }
            }

            var lastTwoChars = currentWord.Substring(currentWord.Length - 2).ToLower();
            var lastThreeChars = currentWord.Substring(currentWord.Length - 3).ToLower();

            if (Endings.IndexOf(lastTwoChars) != -1 && lastThreeChars != "ied" || lastTwoChars.First() != 'l' && lastTwoChars.Last() == 'e')
            {
                numVowels--;
            }

            return numVowels;
        }
    }
}
