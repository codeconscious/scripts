open System.IO
open System

#r "nuget: CodeConscious.Startwatch, 1.0.0"

let instructions = """
Recursive Path Normalizer for files and directories
• An F# script that recursively normalizes either (1) the name of a given file
  or (2) the names of all directories and files within a given directory.
  Each item is renamed, ignored (if its name is already normalized), and
  renaming errors are tracked and reported.
• Requirement: .NET 9 runtime
• Usage: dotnet fsi RecursivePathNormalization.fsx YOUR_FILE_OR_DIRECTORY_PATH
• Item renaming cannot be reversed, so consider backing up your files first.
• URL: https://github.com/codeconscious/scripts/blob/main/fsharp/RecursivePathNormalizer.fsx"""

type Startwatch = Startwatch.Library.Watch

type Path =
    | File of string
    | Directory of string

module ArgValidation =
    type ValidationResult =
        | NoArgs
        | TooManyArgs
        | BadPath of string
        | ValidPath of Path

    let rawArgs = fsi.CommandLineArgs |> Array.toList |> List.tail // The head contains the script filename.

    let validate (args: string list) =
        match args with
        | t when t.Length = 0 -> NoArgs
        | t when t.Length > 1 -> TooManyArgs
        | t ->
            let arg = t.Head
            if Directory.Exists arg then ValidPath (Directory arg)
            elif File.Exists arg then ValidPath (File arg)
            else BadPath arg

    let runValidation =
        match validate rawArgs with
        | NoArgs -> Error "No file or directory path was passed."
        | TooManyArgs -> Error "Too many arguments were passed. Enter a single file or directory path."
        | BadPath p -> Error $"Path \"{p}\" was not found."
        | ValidPath p -> Ok p

module Renaming =
    type RenameResult =
        | Renamed of string
        | Ignored of string
        | Failed of string

    let rec iterateFiles path : Path seq =
        match path with
        | File _ -> seq { yield path }
        | Directory d ->
            seq {
                // Handle the files in the current directory.
                let files = Directory.EnumerateFiles(d, "*")

                yield! files |> Seq.map (fun f -> File f)

                // Recursively handle any subdirectories and their files.
                let subDirs = Directory.EnumerateDirectories d |> Seq.map (fun x -> Directory x)
                for subDir in subDirs do
                    yield! iterateFiles subDir
                    yield subDir
            }

    let rename pathItem =
        let renameFile (oldName: string) (newName: string) =
            try
                File.Move(oldName, newName)
                Renamed $"File \"{oldName}\" → \"{newName}\""
            with
                | :? FileNotFoundException -> Failed $"File \"{oldName}\" was not found."
                | e -> Failed $"Could not rename file \"{oldName}\": {e.Message}"

        let renameDir (oldName: string) (newName: string) =
            try
                Directory.Move(oldName, newName)
                Renamed $"Directory \"{oldName}\" → \"{newName}\""
            with
                | :? DirectoryNotFoundException -> Failed $"Directory \"{oldName}\" was not found."
                | e -> Failed $"Could not rename directory \"{oldName}\": {e.Message}"

        let oldName = match pathItem with File f -> f | Directory d -> d
        let normalizationForm = Text.NormalizationForm.FormC

        if oldName.IsNormalized normalizationForm
        then Ignored oldName
        else
            let newName = oldName.Normalize(normalizationForm)
            if oldName.Equals(newName, StringComparison.InvariantCulture)
            then Ignored oldName
            else
                match pathItem with
                | File f -> renameFile f newName
                | Directory d -> renameDir d newName

module Printing =
    open Renaming

    let printColor color text =
        match color with
        | None -> printfn $"{text}"
        | Some c ->
            Console.ForegroundColor <- c
            printfn $"{text}"
            Console.ResetColor()

    let summarize (watch: Startwatch) (results: RenameResult array) =
        let total = results.Length
        let renamedCount = results |> Array.filter _.IsRenamed |> _.Length
        let ignoredCount = results |> Array.filter _.IsIgnored |> _.Length
        let failed = results |> Array.filter _.IsFailed
        let failedCount = failed.Length
        let fileTense i = if i = 1 then "file" else "files"
        let formatNumber (i: int) = i.ToString("N0")

        // Print the names of the paths with errors.
        failed |> Array.iter (fun f -> $"[Error] {f}" |> printColor (Some ConsoleColor.Red))

        printColor None $"Processed {formatNumber total} {fileTense total} in {watch.ElapsedFriendly}."
        printColor None $"• Renamed: {formatNumber renamedCount}"
        printColor None $"• Ignored: {formatNumber ignoredCount}"
        let failedColor = if failedCount > 0 then (Some ConsoleColor.Red) else None
        printColor failedColor $"• Failed:  {formatNumber failedCount}"

open ArgValidation
open Renaming
open Printing

let processPath path =
    let watch = Startwatch()

    path
    |> iterateFiles
    |> Seq.map rename
    |> Array.ofSeq
    |> summarize watch

match runValidation with
| Ok path ->
    processPath path
    0
| Error e ->
    $"ERROR: {e}" |> printColor (Some ConsoleColor.Red)
    instructions |> printColor None
    1
