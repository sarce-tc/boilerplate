namespace Microservice.Client.Infrastructure.Http;

/// <summary>Bound from wwwroot/appsettings.json → "Api". Drives the typed client base address.</summary>
public sealed class ApiOptions
{
    public const string SectionName = "Api";

    /// <summary>Absolute base URL of the backend, e.g. https://localhost:7443/ </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>API version segment, e.g. "v1". Combined into the per-feature route prefix.</summary>
    public string Version { get; set; } = "v1";

    /// <summary>Builds "api/v1/{resource}" — the versioned route prefix features compose on.</summary>
    public string ResourcePath(string resource) => $"api/{Version}/{resource}";
}
