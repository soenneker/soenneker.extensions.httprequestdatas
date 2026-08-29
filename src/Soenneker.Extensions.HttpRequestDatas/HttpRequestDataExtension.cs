using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Soenneker.Extensions.HttpRequestDatas;

/// <summary>
/// A collection of helpful HttpRequestData (Functions) extension methods
/// </summary>
public static class HttpRequestDataExtension
{
    private const int _maxHeaderChars = 8 * 1024;

    // Use spans so we can stay in Span-land.
    private static ReadOnlySpan<char> BearerPrefix => "Bearer ".AsSpan();

    /// <summary>
    /// Reads a Bearer token from the request authorization header without allocating a replacement header string.
    /// </summary>
    /// <param name="req">The request data containing the authorization header.</param>
    /// <param name="token">Receives the token text when a valid Bearer header is found.</param>
    /// <param name="authHeaderBacking">Receives the backing authorization-header value used by the token span.</param>
    /// <returns>True when a valid Bearer token was found; otherwise false.</returns>
    public static bool TryGetBearer(this HttpRequestData req, out ReadOnlySpan<char> token, out string? authHeaderBacking)
    {
        token = default;
        authHeaderBacking = null;

        if (!req.Headers.TryGetValues("Authorization", out IEnumerable<string>? values) || values is null)
            return false;

        // Pick first non-empty value (no LINQ).
        foreach (string v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
            {
                authHeaderBacking = v;
                break;
            }
        }

        if (authHeaderBacking is null)
            return false;

        if ((uint)authHeaderBacking.Length > _maxHeaderChars)
            return false;

        ReadOnlySpan<char> span = authHeaderBacking.AsSpan();

        // Fast path: already starts with "Bearer " (common case).
        if (span.Length > BearerPrefix.Length && span.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            token = TrimToken(span.Slice(BearerPrefix.Length));
            return !token.IsEmpty;
        }

        // Slow path: tolerate leading whitespace or odd formatting.
        span = span.Trim();

        if (span.Length <= BearerPrefix.Length || !span.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        token = TrimToken(span.Slice(BearerPrefix.Length));
        return !token.IsEmpty;
    }

    /// <summary>
    /// Writes a 401 Unauthorized response with the supplied message.
    /// </summary>
    /// <param name="req">The request data containing the authorization header.</param>
    /// <param name="message">The text written in the unauthorized response.</param>
    /// <returns>An awaitable that completes after the unauthorized response has been written.</returns>
    public static ValueTask WriteUnauthorized(this HttpRequestData req, string? message)
    {
        HttpResponseData res = req.CreateResponse(HttpStatusCode.Unauthorized);

        System.Threading.Tasks.Task writeTask = res.WriteStringAsync(message ?? string.Empty);
        req.FunctionContext.GetInvocationResult()
           .Value = res;

        return new ValueTask(writeTask);
    }

    private static ReadOnlySpan<char> TrimToken(ReadOnlySpan<char> token)
    {
        // Avoid Trim() work if no surrounding whitespace.
        if (token.IsEmpty)
            return token;

        if (!char.IsWhiteSpace(token[0]) && !char.IsWhiteSpace(token[^1]))
            return token;

        return token.Trim();
    }
}