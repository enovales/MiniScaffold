#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open System

let release = LoadReleaseNotes "RELEASE_NOTES.md"
let srcGlob = "*.csproj"
// let testsGlob = "tests/**/*.fsproj"


Target "Clean" (fun _ ->
    [ "obj" ;"dist"]
    |> CleanDirs

    // !! srcGlob
    // |> Seq.collect(fun p -> 
    //     ["bin";"obj"] 
    //     |> Seq.map(fun sp ->
    //          IO.Path.GetDirectoryName p @@ sp)
    //     )
    // |> CleanDirs

    )

Target "DotnetRestore" (fun _ ->
    !! srcGlob
    |> Seq.iter (fun proj ->
        DotNetCli.Restore (fun c ->
            { c with
                Project = proj
                //This makes sure that Proj2 references the correct version of Proj1
                AdditionalArgs = [sprintf "/p:PackageVersion=%s" release.NugetVersion]
            }) 
))



Target "DotnetPack" (fun _ ->
    !! srcGlob
    |> Seq.iter (fun proj ->
        DotNetCli.Pack (fun c ->
            { c with
                Project = proj
                Configuration = "Release"
                OutputPath = IO.Directory.GetCurrentDirectory() @@ "dist"
                AdditionalArgs = [sprintf "/p:PackageVersion=%s" release.NugetVersion]
            }) 
    )
)

Target "Publish" (fun _ ->
    Paket.Push(fun c ->
            { c with 
                PublishUrl = "https://www.nuget.org"
                WorkingDir = "dist"
            }
        )
)

"Clean"
  ==> "DotnetRestore"
  ==> "DotnetPack"
  ==> "Publish"

RunTargetOrDefault "DotnetPack"