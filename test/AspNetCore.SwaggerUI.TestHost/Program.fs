module AspNetCore.SwaggerUI.TestHost.App

open System
open System.IO

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection

open Giraffe
open YamlDotNet.Serialization

open AspNetCore.SwaggerUI.TestHost.HttpHandlers

open AspNetCore.SwaggerUI

// ---------------------------------
// Web app
// ---------------------------------

let yamlToJson yaml =
    let deserializer = (DeserializerBuilder()).Build()
    let yamlObj = deserializer.Deserialize(new StringReader(yaml))

    let serializer = (SerializerBuilder()).JsonCompatible().Build()

    serializer.Serialize yamlObj


let openApi = File.ReadAllText "openapi.yml"

let webApp =
    choose [
        GET >=> route "/openapi"
            >=> setHttpHeader "Content-Type" "application/json"
            >=> setBodyFromString (yamlToJson openApi)
        subRoute "/api"
            (choose [
                GET >=> choose [
                    route "/hello" >=> handleGetHello
                ]
            ])
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureApp (app : IApplicationBuilder) =
    app.Map(PathString "/openapi/ui", fun s -> s.UseSwaggerUI("/openapi")) |> ignore

    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0