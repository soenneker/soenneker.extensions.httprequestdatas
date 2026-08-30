[![](https://img.shields.io/nuget/v/soenneker.extensions.httprequestdatas.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.httprequestdatas/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.httprequestdatas/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.httprequestdatas/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.extensions.httprequestdatas.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.httprequestdatas/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.httprequestdatas/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.httprequestdatas/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.HttpRequestDatas
Bearer-token parsing and unauthorized-response helpers for .NET isolated Azure Functions.

## Installation

```bash
dotnet add package Soenneker.Extensions.HttpRequestDatas
```

## Read a bearer token

```csharp
using Soenneker.Extensions.HttpRequestDatas;

if (!request.TryGetBearer(out ReadOnlySpan<char> token, out string? authorizationHeader))
{
    await request.WriteUnauthorized("A bearer token is required");
    return;
}

bool valid = token.SequenceEqual(expectedToken);
```

`TryGetBearer()` reads the first nonblank `Authorization` header and accepts the `Bearer` scheme case-insensitively. Leading and trailing whitespace around the header and token are ignored. Empty tokens, tokens containing whitespace, other schemes, and headers larger than 8 KiB return `false`.

The returned token is a span over `authorizationHeader`; it does not allocate a second string. Keep the backing string in scope while using the span. If the token must survive an `await` or be stored, copy it first:

```csharp
if (request.TryGetBearer(out ReadOnlySpan<char> token, out string? authorizationHeader))
{
    string tokenText = token.ToString();
    await ValidateToken(tokenText, cancellationToken);
}
```

The method extracts the credential but does not validate its signature, issuer, audience, expiry, or permissions.

## Write an unauthorized result

```csharp
await request.WriteUnauthorized("Invalid credentials");
return;
```

`WriteUnauthorized()` creates a `401 Unauthorized` response, writes the supplied message as the body, and assigns the response to the current invocation result. A `null` message produces an empty body. It does not stop the function, so return immediately unless further processing is intentional.
