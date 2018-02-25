# AspNetCore.SwaggerUI

Provide a middleware to expose a custom version of Swagger UI.

This is an .Net integration of the following swagger-ui (modified):
https://github.com/jensoleg/swagger-ui


## Installation

```
Install-Package AspNetCore.SwaggerUI
```

## Usage

2 extension methods on IApplicationBuilder are provided.

### `UseSwaggerUI()`

```csharp
public void Configure(IApplicationBuilder app)
{
  app.Map("/openapi/ui", s => s.UseSwaggerUI())
}
```

The Swagger UI would be accessible on:
http://localhost/openapi/ui?url=<your-openapi-definition-url>

### `UseSwaggerUI(string defaultOpenApiDefinition)`

```csharp
public void Configure(IApplicationBuilder app)
{
  app.Map("/openapi/ui", s => s.UseSwaggerUI("/openapi"))
}
```

This version is similar to the preceding one, with the possibility to provide a default openapi definition

If you browse to http://localhost/openapi/ui you will be redirected to http://localhost/openapi/ui?url=/openapi