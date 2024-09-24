open System.IO
open System

module ArgValidation =
    type Args =
        { Directory: string;
          IncludedExtensions: string array }

    type ResultBuilder() =
        member this.Bind(m, f) =
            match m with
            | Error e -> Error e
            | Ok a -> f a

        member this.Return(x) =
            Ok x

    let result = new ResultBuilder()

    let validateArgs =
        let rawArgs =
            fsi.CommandLineArgs
            |> Array.toList
            |> List.tail // The head contains the script filename.

        let checkCount (args: string list) =
            let instructions = """
Usage: dotnet fsi RecursiveRandomRename.fsx <DIRECTORY_PATH>
           • Updates all non-hidden files

       dotnet fsi RecursiveRandomRename.fsx <DIRECTORY_PATH> <EXT1,EXT2,EXT3,...>
           • Renames only non-hidden files with the specific extensions
           • The initial period is optional (i.e., '.m4a' == 'm4a')
           Sample: dotnet fsi RecursiveRandomRename.fsx /users/me/Downloads/testing m4a,mp3,ogg

Renaming cannot be undone, so be sure to back up your files first!
            """

            let appendInstructions text = $"{text}{Environment.NewLine}{instructions}"
            match args.Length with
            | l when l = 1 -> Ok { Directory = args[0]; IncludedExtensions = [||] }
            | l when l = 2 -> Ok { Directory = args[0]; IncludedExtensions = args[1].Split(',') }
            | l when l > 2 -> Error ("Too many arguments."    |> appendInstructions)
            | _ ->            Error ("No arguments supplied." |> appendInstructions)

        let checkDirectory args =
            if args.Directory |> Directory.Exists
            then Ok args
            else Error $"Directory \"{args.Directory}\" was not found."

        let checkExts args =
            let checkedExts =
                args.IncludedExtensions
                |> Seq.map (fun e -> if e[0] = '.' then e else $".{e}")
                |> Array.ofSeq

            Ok { args with IncludedExtensions = checkedExts }

        result {
            let! args = checkCount rawArgs
            let! args' = checkDirectory args
            let! args'' = checkExts args'
            return args''
        }

module Renaming =
    type DirectoryItem =
        | File of string
        | Directory of string
        | HiddenFile of string
        | ExcludedFile of string
        | HiddenDirectory of string

    type RenameResult =
        | Renamed of string
        | Ignored of string
        | Failed of string

    let rec allDirectoryItems dir includedExts isChildOfHidden : DirectoryItem seq =
        let checkHidden isDir path : bool =
            let name = if isDir then DirectoryInfo(path).Name else Path.GetFileName(path)
            match name with
            | p when p[0] = '.' -> true
            | _ ->
                let attrs = File.GetAttributes path
                attrs.HasFlag FileAttributes.Hidden

        seq {
            // Handle the files in this directory.
            let isThisDirHidden = isChildOfHidden || dir |> checkHidden true
            let files = Directory.EnumerateFiles(dir, "*")
            yield! match isThisDirHidden with
                   // | true  -> files |> Seq.map (fun p -> HiddenFile p) // Displays skipped files in hidden directories.
                   | true  -> Seq.empty // Does not display skipped files in hidden directories.
                   | false ->
                        files
                        |> Seq.map (fun f ->
                            if f |> checkHidden false then HiddenFile f
                            elif includedExts |> Array.contains (Path.GetExtension(f)) then File f
                            elif includedExts.Length = 0 then File f
                            else ExcludedFile f)

            // Recursively handle any subdirectories and their files.
            for subDir in Directory.EnumerateDirectories(dir) do
                let isSubDirHidden = isChildOfHidden || subDir |> checkHidden true
                yield! allDirectoryItems subDir includedExts isSubDirHidden
                yield if isSubDirHidden then HiddenDirectory subDir else Directory subDir
        }

    let rename path =
        let randomName () = Guid.NewGuid().ToString()

        let renameFile (oldName: string) =
            try
                let dir = Path.GetDirectoryName(oldName)
                let ext = Path.GetExtension(oldName) // Includes initial period
                let newName = $"{randomName()}{ext}"
                let newPath = Path.Combine(dir, newName)
                File.Move(oldName, newPath)
                Renamed $"File \"{oldName}\" → \"{newName}\""
            with
                | :? FileNotFoundException -> Failed $"File \"{oldName}\" was not found."
                | e -> Failed $"Failure renaming file \"{oldName}\": {e.Message}"

        let renameDir (oldName: string) =
            try
                let dir = Path.GetDirectoryName(oldName)
                let newName = randomName()
                let newPath = Path.Combine(dir, newName)
                Directory.Move(oldName, newPath)
                Renamed $"Directory \"{oldName}\" → \"{newName}\""
            with
                | :? FileNotFoundException -> Failed $"Directory \"{oldName}\" was not found."
                | e -> Failed $"Failure renaming directory \"{oldName}\": {e.Message}"

        match path with
        | File f            -> renameFile f
        | Directory d       -> renameDir d
        | HiddenFile f      -> Ignored <| sprintf $"Hidden file \"{f}\""
        | ExcludedFile f    -> Ignored <| sprintf $"Excluded file \"{f}\""
        | HiddenDirectory d -> Ignored <| sprintf $"Hidden directory \"{d}\" (and any files within)"

open ArgValidation
open Renaming

let printResult =
    let inColor color msg =
        match color with
        | Some c ->
            Console.ForegroundColor <- c
            printfn $"{msg}"
            Console.ResetColor()
        | None -> printfn $"{msg}"

    function
    | Renamed msg -> $"[Renamed] {msg}" |> inColor None
    | Ignored msg -> $"[Ignored] {msg}" |> inColor (Some ConsoleColor.DarkGray)
    | Failed msg  -> $"[Error] {msg}"   |> inColor (Some ConsoleColor.Red)

match validateArgs with
| Ok args ->
    allDirectoryItems args.Directory args.IncludedExtensions false
    |> Seq.map rename
    |> Seq.iter printResult
    0
| Error e ->
    printfn $"ERROR: {e}"
    1
