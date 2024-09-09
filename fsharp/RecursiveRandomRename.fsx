open System.IO
open System

type DirectoryItem =
    | Directory of string
    | File of string
    | HiddenDirectory of string
    | HiddenFile of string

type RenameResult =
    | Renamed of string
    | Ignored of string
    | Failed of string

let rec allDirectoryItems dir pattern isChildOfHidden : seq<DirectoryItem> =
    let isHidden isDir path : bool =
        let name = if isDir then DirectoryInfo(path).Name else Path.GetFileName(path)
        match name with
        | p when p[0] = '.' -> true
        | _ ->
            let attrs = File.GetAttributes path
            attrs.HasFlag FileAttributes.Hidden

    seq {
        // Handle the files in this directory.
        let thisDirIsHidden = isChildOfHidden || dir |> isHidden true
        let files = Directory.EnumerateFiles(dir, pattern)
        yield! match thisDirIsHidden with
               | true  -> files |> Seq.map (fun p -> HiddenFile p)
               | false -> files |> Seq.map (fun p -> if p |> isHidden false then HiddenFile p else File p)

        // Recursively handle any subdirectories and their files.
        for subDir in Directory.EnumerateDirectories(dir) do
            let subDirIsHidden = isChildOfHidden || subDir |> isHidden true
            yield! allDirectoryItems subDir pattern subDirIsHidden
            yield if subDirIsHidden then HiddenDirectory subDir else Directory subDir
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
    | File f -> renameFile f
    | Directory d -> renameDir d
    | HiddenFile f -> Ignored <| sprintf $"Hidden file \"{f}\""
    | HiddenDirectory d -> Ignored <| sprintf $"Hidden directory \"{d}\""

let print =
    let inColor (color: ConsoleColor option) msg =
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

allDirectoryItems "/Users/jd/Downloads/generated_files/" "*" false
|> Seq.map (fun itemInDir -> rename itemInDir)
|> Seq.iter (fun result -> print result)
