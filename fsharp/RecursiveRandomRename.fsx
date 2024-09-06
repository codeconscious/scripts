open System.IO
open System

type DirectoryItem =
    | DirectoryName of string
    | FileName of string

let rec getAllFiles dir pattern : seq<DirectoryItem> =
    let isNotHidden (path:string) =
        let attrs = File.GetAttributes(path)
        not <| attrs.HasFlag(FileAttributes.Hidden)

    seq {
        yield! Directory.EnumerateFiles(dir, pattern)
               |> Seq.where (fun f -> isNotHidden f)
               |> Seq.map FileName

        for d in Directory.EnumerateDirectories(dir) do
            yield DirectoryName d
            yield! getAllFiles d pattern
    }

let rename path newName =
    let renameFile (oldName:string) newName =
        try
            let dir = Path.GetDirectoryName(oldName)
            let ext = Path.GetExtension(oldName) // Includes initial period
            // let separator = Path.DirectorySeparatorChar
            let qualifiedNewName = Path.Combine(dir, $"{newName}{ext}")
            File.Move(oldName, qualifiedNewName)
            Ok $"Renamed file \"{oldName}\" to \"{qualifiedNewName}\"."
        with
            | :? FileNotFoundException -> Error $"File \"{oldName}\" was not found."
            | e -> Error $"Failure renaming \"{oldName}\" to \"{newName}\": {e.Message}"

    let renameDir (oldName:string) newName =
        try
            let dir = Path.GetDirectoryName(oldName)
            let qualifiedNewName = Path.Combine(dir, newName)
            Directory.Move(oldName, qualifiedNewName)
            Ok $"Renamed directory \"{oldName}\" to \"{qualifiedNewName}\"."
        with
            | :? FileNotFoundException -> Error $"Directory \"{oldName}\" was not found."
            | e -> Error $"Failure renaming \"{oldName}\" to \"{newName}\": {e.Message}"

    match path with
    | FileName f -> renameFile f newName
    | DirectoryName d -> renameDir d newName

let newGuid = Guid.NewGuid().ToString()

getAllFiles "/Users/jd/Downloads/generated_files/" "*"
|> Seq.map (fun f -> rename f newGuid)
|> Seq.iter (fun res -> match res with
                        | Ok s    -> printfn "[OK] %s" s
                        | Error e -> printfn "[ERROR] %s" e)
