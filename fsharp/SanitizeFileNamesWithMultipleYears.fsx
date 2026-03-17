#!/usr/bin/env dotnet fsi

#r "nuget: Spectre.Console"

open System
open System.IO
open System.Text.RegularExpressions
open Spectre.Console

// Pattern: space, opening paren, 4 digits, slash, 4 digits, closing paren
let pattern = Regex(@" \((\d{4})/(\d{4})\)")

// Get all files in current directory, excluding hidden files and directories
let getFiles () =
    Directory.GetFiles(Directory.GetCurrentDirectory())
    |> Array.filter (fun f ->
        let filename = Path.GetFileName(f)
        not (File.GetAttributes(f).HasFlag(FileAttributes.Hidden)) &&
        not (Directory.Exists(f)) &&
        not (filename.StartsWith("."))
    )
    |> Array.map Path.GetFileName

// Extract first year from pattern match
let extractFirstYear (filename: string) : string option =
    let m = pattern.Match(filename)
    if m.Success then Some m.Groups.[1].Value else None

// Create new filename with first year in square brackets
let createNewFilename (filename: string) (firstYear: string) : string =
    pattern.Replace(filename, sprintf " [%s]" firstYear, 1)

// Prompt user for confirmation
let rec promptUser (filename: string) (newFilename: string) : bool option =
    AnsiConsole.Write("> Rename? (y/n/q): ")
    let response = Console.ReadLine().ToLower().Trim()

    match response with
    | "y" -> Some true
    | "n" -> Some false
    | "q" -> None
    | _ ->
        AnsiConsole.MarkupLine("[red]Invalid input. Please enter 'y', 'n', or 'q'.[/]")
        promptUser filename newFilename

// Process a single file
let processFile (dryRun: bool) (filename: string) : bool =
    match extractFirstYear filename with
    | Some firstYear ->
        let newFilename = createNewFilename filename firstYear

        AnsiConsole.MarkupLine($"[cyan]Current:[/]  {filename}")
        AnsiConsole.MarkupLine($"[cyan]Proposed:[/] {newFilename}")

        match promptUser filename newFilename with
        | Some true ->
            if dryRun then
                AnsiConsole.MarkupLine($"[yellow]\[DRY RUN][/] Would rename to: {newFilename}")
            else
                try
                    File.Move(filename, newFilename, false)
                    AnsiConsole.MarkupLine("[green]✓ Renamed[/]")
                with
                | ex ->
                    AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]")
                    false
            true
        | Some false ->
            AnsiConsole.MarkupLine("[yellow]✗ Skipped[/]")
            false
        | None ->
            false
    | None -> false

// Main script
[<EntryPoint>]
let main argv =
    let dryRun = argv |> Array.contains "--dry-run"

    let files = getFiles ()

    if files.Length = 0 then
        AnsiConsole.MarkupLine("[yellow]No files found in current directory.[/]")
        0
    else
        let mutable renamedCount = 0
        let mutable skippedCount = 0
        let mutable quit = false

        for filename in files do
            if not quit then
                AnsiConsole.MarkupLine("")
                let result = processFile dryRun filename

                if result then
                    renamedCount <- renamedCount + 1
                else if extractFirstYear filename |> Option.isSome then
                    // Only increment skipped if the file matched the pattern
                    if not (processFile dryRun filename) then
                        skippedCount <- skippedCount + 1

        AnsiConsole.MarkupLine("")
        AnsiConsole.MarkupLine("[bold]================================================== [/]")
        AnsiConsole.MarkupLine("[bold]Summary:[/]")
        AnsiConsole.MarkupLine($"[green]Renamed: {renamedCount}[/]")
        AnsiConsole.MarkupLine($"[yellow]Skipped: {skippedCount}[/]")
        if dryRun then
            AnsiConsole.MarkupLine("[yellow]\[DRY RUN][/]")

        0
