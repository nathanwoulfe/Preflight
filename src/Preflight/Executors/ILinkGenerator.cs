using System;
using System.Linq.Expressions;
#if NETCOREAPP
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;
using CoreLinkGenerator = Microsoft.AspNetCore.Routing.LinkGenerator;
#else
using Umbraco.Web;
using Umbraco.Web.WebApi;
using System.Web.Routing;
using System.Web;
using System.Web.Mvc;
#endif

namespace Preflight.Executors
{
    public interface ILinkGenerator
    {
        string GetUmbracoApiServiceBaseUrl<T>(Expression<Func<T, object>> methodSelector) where T : UmbracoApiController;
    }

    public class LinkGenerator : ILinkGenerator
    {
#if NETCOREAPP
        private readonly CoreLinkGenerator _coreLinkGenerator;

        public LinkGenerator(CoreLinkGenerator coreLinkGenerator)
        {
            _coreLinkGenerator = coreLinkGenerator;
        }

        public string GetUmbracoApiServiceBaseUrl<T>(Expression<Func<T, object>> methodSelector) where T : UmbracoApiController =>
            _coreLinkGenerator.GetUmbracoApiServiceBaseUrl(methodSelector);
#else
        public string GetUmbracoApiServiceBaseUrl<T>(Expression<Func<T, object>> methodSelector) where T : UmbracoApiController
        {
            RequestContext requestContext = HttpContext.Current.Request.RequestContext;
            var urlHelper = new UrlHelper(requestContext);

            return urlHelper.GetUmbracoApiServiceBaseUrl(methodSelector);
        }
#endif
    }
}