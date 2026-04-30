#!/usr/bin/env dotnet fsi

#r "nuget: Spectre.Console"
#r "nuget: CCFSharpUtils"
#r "nuget: FSharpPlus"
#r "nuget: FsToolkit.ErrorHandling"

open System
open System.IO
open System.Text.RegularExpressions
open Spectre.Console
open CCFSharpUtils
open FSharpPlus
open FSharpPlus.Operators

// Pattern: space, opening parenthesis, 4 digits, slash, 4 digits, closing parenthesis.
let pattern = Regex @" \((\d{4})/(\d{4})\)"

let getFiles () =
    Directory.GetFiles(Directory.GetCurrentDirectory())
    |> Array.filter (fun path ->
        let filename = Path.GetFileName path
        not (File.GetAttributes(path).HasFlag(FileAttributes.Hidden)) &&
        not (filename.StartsWith ".") &&
        not (Directory.Exists path)
    )
    |> Array.map Path.GetFileName

let extractFirstYear (fileName: string) : string option =
    let m = pattern.Match fileName
    if m.Success then Some m.Groups[1].Value else None

let generateNewFilename (fileName: string) (firstYear: string) : string =
    pattern.Replace(fileName, sprintf " [%s]" firstYear, 1)

// Prompt user for confirmation
let rec promptUser (fileName: string) (newFileName: string) : bool option =
    AnsiConsole.Write "> Rename? (y/n/q): "
    let response = Console.ReadLine().ToLower().Trim()

    match response with
    | "y" -> Some true
    | "n" -> Some false
    | "q" -> None
    | _ ->
        AnsiConsole.MarkupLine "[red]Invalid input. Please enter 'y', 'n', or 'q'.[/]"
        promptUser fileName newFileName

let processFile (dryRun: bool) (fileName: string) : bool =
    match extractFirstYear fileName with
    | Some firstYear ->
        let newFilename = generateNewFilename fileName firstYear

        AnsiConsole.MarkupLine $"[cyan]Current:[/]  {fileName}"
        AnsiConsole.MarkupLine $"[cyan]Proposed:[/] {newFilename}"

        match promptUser fileName newFilename with
        | Some true ->
            if dryRun then
                AnsiConsole.MarkupLine $"[yellow]\[DRY RUN][/] Would rename to: {newFilename}"
                true
            else
                try
                    File.Move(fileName, newFilename, false)
                    AnsiConsole.MarkupLine "[green]✓ Renamed[/]"
                    true
                with
                | ex ->
                    AnsiConsole.MarkupLine $"[red]✗ Error: {ex.Message}[/]"
                    false
        | Some false ->
            AnsiConsole.MarkupLine "[yellow]✗ Skipped[/]"
            false
        | None ->
            false
    | None ->
        false

let dryRun = fsi.CommandLineArgs |> Array.contains "--dry-run"

let files = getFiles ()

if Array.isEmpty files then
    AnsiConsole.MarkupLine "[yellow]No files found in current directory.[/]"
    0
else
    let mutable renamedCount = 0
    let mutable skippedCount = 0
    let mutable quit = false

    for filename in files do
        if not quit then
            AnsiConsole.MarkupLine String.Empty
            let result = processFile dryRun filename

            if result then
                renamedCount <- renamedCount + 1
            else if extractFirstYear filename |> Option.isSome then
                // Only increment skipped if the file matched the pattern
                if not (processFile dryRun filename) then
                    skippedCount <- skippedCount + 1

    AnsiConsole.MarkupLine String.Empty
    AnsiConsole.MarkupLine "[bold]================================================== [/]"
    AnsiConsole.MarkupLine "[bold]Summary:[/]"
    AnsiConsole.MarkupLine $"[green]Renamed: {renamedCount}[/]"
    AnsiConsole.MarkupLine $"[yellow]Skipped: {skippedCount}[/]"
    if dryRun then AnsiConsole.MarkupLine "[yellow]\[DRY RUN][/]"

    0
