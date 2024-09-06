open System.IO

type FileName = FileName of string
type DirectoryName = DirectoryName of string

type DirectoryItem =
    | SubDirectory of DirectoryName
    | File of FileName

let rec getAllFiles dir pattern =
    seq { yield! Directory.EnumerateFiles(dir, pattern)
          for d in Directory.EnumerateDirectories(dir) do
              yield d
              yield! getAllFiles d pattern }

getAllFiles "/Users/jd/Downloads/generated_files/" "*"
|> Seq.where (fun n -> Path.GetFileName(n)[0] <> '.')
|> Seq.iter (fun n -> printfn "%A" n)

let renameFile oldName newName =
    try
        Ok <| File.Move(oldName, newName)
    with
        | :? FileNotFoundException -> Error $"\"{oldName}\" was not found."
        | e -> Error $"Failure renaming \"{oldName}\" to \"{newName}\": {e.Message}"

let renameDir oldName newName =
    try
        Ok <| Directory.Move(oldName, newName)
    with
        | :? FileNotFoundException -> Error $"Directory \"{oldName}\" was not found."
        | e -> Error $"Failure renaming \"{oldName}\" to \"{newName}\": {e.Message}"
