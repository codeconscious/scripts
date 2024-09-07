open System.IO
open System

type DirectoryItem =
    | Directory of string
    | HiddenDirectory of string
    | File of string
    | HiddenFile of string

let rec allDirectoryItems dir pattern : seq<DirectoryItem> =
    let isHidden (path: string) : bool =
        match path with
        | p when p[0] = '.' -> true
        | p ->
            let attrs = File.GetAttributes(p)
            attrs.HasFlag(FileAttributes.Hidden)

    let items = seq {
        yield! Directory.EnumerateFiles(dir, pattern)
               |> Seq.map (fun p -> match p |> isHidden with
                                    | true -> HiddenFile p
                                    | false -> File p)

        for dir in Directory.EnumerateDirectories(dir) do
            yield (if isHidden dir then HiddenDirectory dir else Directory dir)
            yield! allDirectoryItems dir pattern
    }

    // Reversing ensures files are renamed before their enclosing directory,
    // thus preventing renaming failures.
    items |> Seq.rev

let rename path =
    let newGuid () = Guid.NewGuid().ToString()

    let renameFile (oldName: string) =
        try
            let dir = Path.GetDirectoryName(oldName)
            let ext = Path.GetExtension(oldName) // Includes initial period
            let newName = $"{newGuid()}{ext}"
            let newPath = Path.Combine(dir, newName)
            File.Move(oldName, newPath)
            Ok $"Renamed file \"{oldName}\" to \"{newName}\"."
        with
            | :? FileNotFoundException -> Error $"File \"{oldName}\" was not found."
            | e -> Error $"Failure renaming \"{oldName}\": {e.Message}"

    let renameDir (oldName: string) =
        try
            let dir = Path.GetDirectoryName(oldName)
            let newName = newGuid()
            let newPath = Path.Combine(dir, newName)
            Directory.Move(oldName, newPath)
            Ok $"Renamed directory \"{oldName}\" to \"{newName}\"."
        with
            | :? FileNotFoundException -> Error $"Directory \"{oldName}\" was not found."
            | e -> Error $"Failure renaming \"{oldName}\": {e.Message}"

    match path with
    | File f -> renameFile f
    | HiddenFile f -> Ok <| sprintf $"Ignoring hidden file \"{f}\"."
    | Directory d -> renameDir d
    | HiddenDirectory d -> Ok <| sprintf $"Ignoring hidden directory \"{d}\"."

allDirectoryItems "/Users/jd/Downloads/generated_files/" "*"
|> Seq.map (fun itemInDir -> rename itemInDir)
|> Seq.iter (function
             | Ok s    -> printfn "[OK] %s" s
             | Error e -> printfn "[ERROR] %s" e)
