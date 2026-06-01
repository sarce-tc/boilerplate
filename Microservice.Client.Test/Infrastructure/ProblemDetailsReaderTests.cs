using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microservice.Client.Infrastructure.Http;
using Microservice.Client.Shared.Results;
using Xunit;

namespace Microservice.Client.Test.Infrastructure;

public class ProblemDetailsReaderTests
{
    private static HttpResponseMessage ProblemResponse(HttpStatusCode status, object body)
    {
        var response = new HttpResponseMessage(status) { Content = JsonContent.Create(body) };
        response.Headers.TryAddWithoutValidation("X-Correlation-Id", "corr-123");
        return response;
    }

    [Fact]
    public async Task Maps_409_to_Conflict_and_lifts_detail_and_correlation()
    {
        using var response = ProblemResponse(HttpStatusCode.Conflict, new
        {
            title = "conflict",
            detail = "El producto está inactivo",
            status = 409,
            correlationId = "corr-from-body"
        });

        var error = await ProblemDetailsReader.ReadAsync(response);

        error.Kind.Should().Be(ErrorKind.Conflict);
        error.Message.Should().Be("El producto está inactivo");
        error.CorrelationId.Should().Be("corr-from-body");
    }

    [Fact]
    public async Task Maps_400_to_Validation_with_field_errors()
    {
        using var response = ProblemResponse(HttpStatusCode.BadRequest, new
        {
            title = "validation_error",
            status = 400,
            errors = new Dictionary<string, string[]> { ["Sku"] = ["A product with this SKU already exists"] }
        });

        var error = await ProblemDetailsReader.ReadAsync(response);

        error.Kind.Should().Be(ErrorKind.Validation);
        error.FieldErrors.Should().ContainKey("sku");
        error.FieldErrors!["sku"].Should().ContainSingle();
    }

    [Fact]
    public async Task Maps_429_to_RateLimited()
    {
        using var response = ProblemResponse(HttpStatusCode.TooManyRequests, new { title = "rate_limited", status = 429 });

        var error = await ProblemDetailsReader.ReadAsync(response);

        error.Kind.Should().Be(ErrorKind.RateLimited);
    }

    [Fact]
    public async Task Falls_back_to_header_correlation_when_body_is_not_json()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("upstream exploded")
        };
        response.Headers.TryAddWithoutValidation("X-Correlation-Id", "corr-hdr");

        var error = await ProblemDetailsReader.ReadAsync(response);

        error.Kind.Should().Be(ErrorKind.Unexpected);
        error.CorrelationId.Should().Be("corr-hdr");
    }
}
