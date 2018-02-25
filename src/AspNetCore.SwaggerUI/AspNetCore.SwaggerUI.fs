namespace AspNetCore.SwaggerUI

open System
open System.Threading.Tasks
open System.Runtime.CompilerServices

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.FileProviders

[<AutoOpen>]
module Implementation =
  let redirect (specPath : string) (context : HttpContext) (next : Func<Task>) =
    if context.Request.Path.ToString().Trim('/') |> String.IsNullOrEmpty
        && context.Request.Query.Count = 0 then
      context.Response.StatusCode <- 302
      context.Response.Headers.Add("Location", StringValues("?url=" + specPath))
      Task.CompletedTask
    else
      next.Invoke()

[<Extension>]
type ApplicationBuilder =
  [<Extension>]
  static member UseSwaggerUI(app: IApplicationBuilder) =
    let currentAssembly = typeof<ApplicationBuilder>.Assembly

    app
      .UseFileServer(FileServerOptions(
                      FileProvider = new EmbeddedFileProvider(currentAssembly, "AspNetCore.SwaggerUI.resources")
                    ))
    |> ignore
  
  [<Extension>]
  static member UseSwaggerUI(app: IApplicationBuilder, defaultOpenApiDefinition: string) =
    app
      .Use(redirect defaultOpenApiDefinition)
      .UseSwaggerUI()
    |> ignore
