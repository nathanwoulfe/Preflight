using Umbraco.Cms.Core.Packaging;

namespace Preflight.Migrations;

public sealed class PreflightMigrationPlan : PackageMigrationPlan
{
    public PreflightMigrationPlan()
        : base(KnownStrings.Name)
    {
    }

    /// <inheritdoc/>
    public override string InitialState => "Preflight_ZeroZeroZero";

    /// <inheritdoc/>
    protected override void DefinePlan() =>
        From(InitialState)
            .To<Preflight_TwoZeroZero>(nameof(Preflight_TwoZeroZero));
}
