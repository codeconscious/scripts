open System.IO

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
    let renameFile oldName newName =
        try
            Ok <| File.Move(oldName, newName)
        with
            | :? FileNotFoundException -> Error $"File \"{oldName}\" was not found."
            | e -> Error $"Failure renaming \"{oldName}\" to \"{newName}\": {e.Message}"

    let renameDir oldName newName =
        try
            Ok <| Directory.Move(oldName, newName)
        with
            | :? FileNotFoundException -> Error $"Directory \"{oldName}\" was not found."
            | e -> Error $"Failure renaming \"{oldName}\" to \"{newName}\": {e.Message}"

    match path with
    | FileName f -> renameFile f newName
    | DirectoryName d -> renameDir d newName

let summarize dirItem =
    match dirItem with
    | DirectoryName d -> $"- {d}"
    | FileName f -> $"  {f}"

getAllFiles "/Users/jd/Downloads/generated_files/" "*"
// |> Seq.where (fun p -> Path.GetFileName(p)[0] <> '.')
|> Seq.iter (fun p -> printfn "%s" (summarize p))
