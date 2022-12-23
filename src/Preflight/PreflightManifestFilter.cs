using Umbraco.Cms.Core.Manifest;

namespace Preflight;

/// <summary>
/// Adds the backoffice files.
/// </summary>
internal sealed class PreflightManifestFilter : IManifestFilter
{
    /// <inheritdoc/>
    public void Filter(List<PackageManifest> manifests) => manifests.Add(new PackageManifest
    {
        PackageName = KnownStrings.Name,
        Scripts = new[]
        {
            "/App_Plugins/Preflight/Backoffice/js/preflight.min.js",
        },
        Stylesheets = new[]
        {
            "/App_Plugins/Preflight/Backoffice/css/preflight.min.css",
        },
        BundleOptions = BundleOptions.None,
    });
}
