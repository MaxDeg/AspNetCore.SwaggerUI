// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r "./packages/build/FAKE/tools/FakeLib.dll"

open Fake
open System

// --------------------------------------------------------------------------------------
// Build variables
// --------------------------------------------------------------------------------------

let apiKey = getBuildParamOrDefault "nugetApiKey" ""

let buildDir  = "./build/"
let appReferences = !! "/**/*.fsproj"
let dotnetcliVersion = "2.1.4"
let mutable dotnetExePath = "dotnet"

// --------------------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------------------

let run' timeout cmd args dir =
    if execProcess (fun info ->
        info.FileName <- cmd
        if not (String.IsNullOrWhiteSpace dir) then
            info.WorkingDirectory <- dir
        info.Arguments <- args
    ) timeout |> not then
        failwithf "Error while running '%s' with args: %s" cmd args

let run = run' System.TimeSpan.MaxValue

let runDotnet workingDir args =
    let result =
        ExecProcess (fun info ->
            info.FileName <- dotnetExePath
            info.WorkingDirectory <- workingDir
            info.Arguments <- args) TimeSpan.MaxValue
    if result <> 0 then failwithf "dotnet %s failed" args

// --------------------------------------------------------------------------------------
// Targets
// --------------------------------------------------------------------------------------

Target "Clean" <| fun _ ->
  CleanDirs [buildDir]


Target "InstallDotNetCLI" <| fun _ ->
  dotnetExePath <- DotNetCli.InstallDotNetSDK dotnetcliVersion


Target "Restore" <| fun _ ->
  appReferences
  |> Seq.iter (fun p ->
    let dir = System.IO.Path.GetDirectoryName p
    runDotnet dir "restore"
  )


Target "Build" <| fun _ ->
  appReferences
  |> Seq.iter (fun p ->
    let dir = System.IO.Path.GetDirectoryName p
    runDotnet dir "build --no-restore"
  )


Target "Package" <| fun _ ->
  appReferences
  |> Seq.iter (fun p ->
    let dir = System.IO.Path.GetDirectoryName p
    runDotnet dir "pack -c Release --no-restore -o build"
  )

Target "Push" <| fun _ ->
  !! "src/**/*.nupkg"
  |> Seq.iter (fun p ->
    let dir = System.IO.Path.GetDirectoryName p

    sprintf "nuget push %s -k %s -s %s" p apiKey "https://api.nuget.org/v3/index.json"
    |> runDotnet dir
  )

Target "PublishAndPush" ignore

// --------------------------------------------------------------------------------------
// Build order
// --------------------------------------------------------------------------------------

"Clean"
  ==> "InstallDotNetCLI"
  ==> "Restore"
  ==> "Build"

"Clean"
  ==> "InstallDotNetCLI"
  ==> "Restore"
  ==> "Package"
  =?> ("Push", not <| String.IsNullOrWhiteSpace apiKey)
  ==> "PublishAndPush"

RunTargetOrDefault "Build"
