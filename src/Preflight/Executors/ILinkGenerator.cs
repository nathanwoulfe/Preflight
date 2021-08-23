using System;
using System.Linq.Expressions;
#if NET472
using Umbraco.Web;
using Umbraco.Web.WebApi;
using System.Web.Routing;
using System.Web;
using System.Web.Mvc;
#else
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;
using CoreLinkGenerator = Microsoft.AspNetCore.Routing.LinkGenerator;
#endif

namespace Preflight.Executors
{
    public interface ILinkGenerator
    {
        string GetUmbracoApiServiceBaseUrl<T>(Expression<Func<T, object>> methodSelector) where T : UmbracoApiController;
    }

    public class LinkGenerator : ILinkGenerator
    {
#if NET472
        public string GetUmbracoApiServiceBaseUrl<T>(Expression<Func<T, object>> methodSelector) where T : UmbracoApiController
        {
            RequestContext requestContext = HttpContext.Current.Request.RequestContext;
            var urlHelper = new UrlHelper(requestContext);

            return urlHelper.GetUmbracoApiServiceBaseUrl(methodSelector);
        }
#else
        private readonly CoreLinkGenerator _coreLinkGenerator;

        public LinkGenerator(CoreLinkGenerator coreLinkGenerator)
        {
            _coreLinkGenerator = coreLinkGenerator;
        }

        public string GetUmbracoApiServiceBaseUrl<T>(Expression<Func<T, object>> methodSelector) where T : UmbracoApiController =>
            _coreLinkGenerator.GetUmbracoApiServiceBaseUrl(methodSelector);
#endif
    }
}