using Preflight.Constants;
#if NET472
using Umbraco.Core.Migrations;
#else
using Umbraco.Cms.Infrastructure.Migrations;
#endif

namespace Preflight.Migrations
{
    public class PreflightMigrationPlan : MigrationPlan
    {
        public PreflightMigrationPlan() : base(KnownStrings.Name) => DefinePlan();

        public override string InitialState => "Preflight_ZeroZeroZero";

        protected void DefinePlan()
        {
            From(InitialState)
                .To<Preflight_TwoZeroZero>(nameof(Preflight_TwoZeroZero));
        }
    }
}
