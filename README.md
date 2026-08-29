[![](https://img.shields.io/nuget/v/soenneker.extensions.httprequestdatas.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.httprequestdatas/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.httprequestdatas/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.httprequestdatas/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.extensions.httprequestdatas.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.httprequestdatas/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.httprequestdatas/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.httprequestdatas/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.HttpRequestDatas
A collection of helpful HttpRequestData (Functions) extension methods.

## Installation

```bash
dotnet add package Soenneker.Extensions.HttpRequestDatas
```

## Quick start

```csharp
using Soenneker.Extensions.HttpRequestDatas;

// Given an existing HttpRequestData named req:
var result = req.TryGetBearer(token, authHeaderBacking);
```

## Common operations

- `TryGetBearer()` - Extracts a non-empty bearer token from the first usable `Authorization` header. It ignores scheme casing and surrounding whitespace, and returns `false` for missing, malformed, or oversized headers.
- `WriteUnauthorized()` - Creates a `401 Unauthorized` response, writes the optional message, and assigns it as the Azure Functions invocation result.
