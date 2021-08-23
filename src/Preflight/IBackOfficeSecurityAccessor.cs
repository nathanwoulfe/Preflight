#if NET472
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Composing;

namespace Preflight.Security
{
    public interface IBackOfficeSecurityAccessor
    {
        IBackOfficeSecurity BackOfficeSecurity { get; }
    }

    public class BackOfficeSecurityAccessor : IBackOfficeSecurityAccessor
    {
        public IBackOfficeSecurity BackOfficeSecurity => new BackOfficeSecurity();
    }

    public interface IBackOfficeSecurity
    {
        IUser CurrentUser { get; }
    }

    public class BackOfficeSecurity : IBackOfficeSecurity
    {
        // TODO => Is this the best place to get the current user?
        public IUser CurrentUser => Current.UmbracoContext?.Security?.CurrentUser;
    }
}
#endif