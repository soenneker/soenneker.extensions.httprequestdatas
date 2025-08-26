using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker.Http;
using Soenneker.Extensions.String;

namespace Soenneker.Extensions.HttpRequestDatas;

/// <summary>
/// A collection of helpful HttpRequestData (Functions) extension methods
/// </summary>
public static class HttpRequestDataExtension
{
    private const int _maxHeaderChars = 8 * 1024;
    private const string _bearerPrefix = "Bearer ";

    public static bool TryGetBearer(this HttpRequestData req, out ReadOnlySpan<char> token, out string? authHeaderBacking)
    {
        token = default;
        authHeaderBacking = null;

        if (!req.Headers.TryGetValues("Authorization", out IEnumerable<string>? values) || values is null)
            return false;

        // Pick the first non-empty value; avoids LINQ allocations.
        foreach (string v in values)
        {
            if (v.HasContent())
            {
                authHeaderBacking = v;
                break;
            }
        }

        if (authHeaderBacking is null)
            return false;

        // Optional: guard against huge headers
        if (authHeaderBacking.Length > _maxHeaderChars)
            return false;

        ReadOnlySpan<char> span = authHeaderBacking.AsSpan().Trim();

        if (span.Length <= _bearerPrefix.Length || !span.StartsWith(_bearerPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        // Slice after "Bearer " and trim surrounding whitespace.
        token = span.Beyond(_bearerPrefix.Length).Trim();
        return !token.IsEmpty;
    }

    private static ReadOnlySpan<char> Beyond(this ReadOnlySpan<char> span, int count) =>
        (uint) count <= (uint) span.Length ? span[count..] : ReadOnlySpan<char>.Empty;
}