using Umbraco.Cms.Infrastructure.Migrations;

namespace Preflight.Migrations;

public class PreflightMigrationPlan : MigrationPlan
{
    public PreflightMigrationPlan()
        : base(KnownStrings.Name) => DefinePlan();

    /// <inheritdoc/>
    public override string InitialState => "Preflight_ZeroZeroZero";

    /// <inheritdoc/>
    protected void DefinePlan() => _ = From(InitialState)
            .To<Preflight_TwoZeroZero>(nameof(Preflight_TwoZeroZero));
}
