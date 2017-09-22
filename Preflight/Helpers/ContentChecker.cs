using Newtonsoft.Json.Linq;
using Preflight.Extensions;
using Preflight.Models;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;

namespace Preflight.Helpers
{
    public class ContentChecker
    {
        /// <summary>
        /// Helper for formatting the property name in the response
        /// </summary>
        /// <param name="added">List of names of properties currently included in the response</param>
        /// <param name="name">Name of the current property</param>
        /// <returns></returns>
        private static string SetName(IList<string> added, string name)
        {
            return added.IndexOf(name) != -1 ? name + " (Editor " + (added.IndexOf(name) + 2) + ")" : name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public PreflightResponseModel Check(IContent content)
        {
            var props = content.Properties.Where(p => p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Grid || p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Rte);
            var added = new List<string>();
            var response = new PreflightResponseModel();

            foreach (var prop in props)
            {
                var name = prop.PropertyType.Name;
                string val;
                ReadabilityResponseModel readability;
                List<BrokenLinkModel> links;

                switch (prop.PropertyType.PropertyEditorAlias)
                {
                    case KnownPropertyAlias.Grid:
                    {
                        var asJson = JObject.Parse(prop.Value.ToString());
                        var rtes = asJson.SelectTokens(MagicStrings.RteJsonPath);

                        foreach (var rte in rtes)
                        {
                            var value = rte.SelectToken(MagicStrings.RteValueJsonPath);
                            if (value == null) continue;

                            val = value.ToString();
                            readability = val.GetReadability();
                            links = val.CheckLinks();

                            response.Properties.Add(new PreflightPropertyResponseModel()
                            {
                                Name = SetName(added, name),
                                Readability = readability,
                                Links = links,
                                Failed = readability.Failed || links.Any()
                            });

                            added.Add(name);
                        }
                    }
                        break;
                    case KnownPropertyAlias.Archetype:
                    {
                        var asJson = JObject.Parse(prop.Value.ToString());
                        var rtes = asJson.SelectTokens(MagicStrings.ArchetypeRteJsonPath);

                        foreach (var rte in rtes)
                        {
                            var value = rte.SelectToken(MagicStrings.RteValueJsonPath);
                            if (value == null) continue;

                            val = value.ToString();
                            readability = val.GetReadability();
                            links = val.CheckLinks();

                            response.Properties.Add(new PreflightPropertyResponseModel()
                            {
                                Name = added.IndexOf(name) != -1 ? name + " (Editor " + (added.IndexOf(name) + 2) + ")" : name,
                                Readability = readability,
                                Links = links,
                                Failed = readability.Failed || links.Any()
                            });

                            added.Add(name);
                        }
                    }
                        break;
                    default:
                        if (prop.Value == null) continue;

                        val = prop.Value.ToString();
                        readability = val.GetReadability();
                        links = val.CheckLinks();

                        response.Properties.Add(new PreflightPropertyResponseModel()
                        {
                            Name = name,
                            Readability = readability,
                            Links = links,
                            Failed = readability.Failed || links.Any()
                        });
                        break;
                }
            }

            response.Failed = response.Properties.Any(p => p.Failed);

            return response;
        }
    }
}
