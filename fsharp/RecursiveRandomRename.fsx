open System.IO
open System

type DirectoryItem =
    | DirectoryName of string
    | FileName of string

let rec getAllFiles dir pattern : seq<DirectoryItem> =
    let isNotHidden (path:string) =
        let attrs = File.GetAttributes(path)
        not <| attrs.HasFlag(FileAttributes.Hidden)

    let items = seq {
        yield! Directory.EnumerateFiles(dir, pattern)
               |> Seq.where (fun f -> isNotHidden f)
               |> Seq.map FileName

        for d in Directory.EnumerateDirectories(dir) do
            yield DirectoryName d
            yield! getAllFiles d pattern
    }

    // Reversing snsures files are renamed before their enclosing directory.
    items |> Seq.rev

let rename path =
    let newGuid () = Guid.NewGuid().ToString()

    let renameFile (oldName: string) =
        try
            let dir = Path.GetDirectoryName(oldName)
            let ext = Path.GetExtension(oldName) // Includes initial period
            let newName = Path.Combine(dir, $"{newGuid()}{ext}")
            File.Move(oldName, newName)
            Ok $"Renamed file \"{oldName}\" to \"{newName}\"."
        with
            | :? FileNotFoundException -> Error $"File \"{oldName}\" was not found."
            | e -> Error $"Failure renaming \"{oldName}\": {e.Message}"

    let renameDir (oldName: string) =
        try
            let dir = Path.GetDirectoryName(oldName)
            let newName = Path.Combine(dir, newGuid())
            Directory.Move(oldName, newName)
            Ok $"Renamed directory \"{oldName}\" to \"{newName}\"."
        with
            | :? FileNotFoundException -> Error $"Directory \"{oldName}\" was not found."
            | e -> Error $"Failure renaming \"{oldName}\" to \"{newGuid}\": {e.Message}"

    match path with
    | FileName f -> renameFile f
    | DirectoryName d -> renameDir d

getAllFiles "/Users/jd/Downloads/generated_files/" "*"
|> Seq.map (fun itemInDir -> rename itemInDir)
|> Seq.iter (function
             | Ok s    -> printfn "[OK] %s" s
             | Error e -> printfn "[ERROR] %s" e)
