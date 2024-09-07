open System.IO
open System

type DirectoryItem =
    | DirectoryName of string
    | FileName of string

let rec allDirectoryItems dir pattern : seq<DirectoryItem> =
    let isNotHidden (path: string) =
        match path with
        | p when p[0] = '.'
            -> false
        | p ->
            let attrs = File.GetAttributes(p)
            not <| attrs.HasFlag(FileAttributes.Hidden)

    let items = seq {
        yield! Directory.EnumerateFiles(dir, pattern)
               |> Seq.where (fun f -> isNotHidden f)
               |> Seq.map FileName

        for d in Directory.EnumerateDirectories(dir) do
            yield DirectoryName d
            yield! allDirectoryItems d pattern
    }

    // Reversing ensures files are renamed before their enclosing directory,
    // preventing renaming errors.
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
    | FileName f -> renameFile f
    | DirectoryName d -> renameDir d

allDirectoryItems "/Users/jd/Downloads/generated_files/" "*"
|> Seq.map (fun itemInDir -> rename itemInDir)
|> Seq.iter (function
             | Ok s    -> printfn "[OK] %s" s
             | Error e -> printfn "[ERROR] %s" e)
