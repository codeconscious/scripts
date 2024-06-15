(* FilesWithSignificantDates: An F# Script

   Summary: Passed a directory, lists files with their created and last-modified times.

   Requirements: .NET 8 runtime (Untested on previous versions, though it might work)

   Usage: dotnet fsi <directoryPath>
          Sample: dotnet fsi FilesWithSignificantDates.fsx \Users\me\Documents

   Note: This was created mainly for learning purposes and might not be very good. ^_^
*)

open System.IO

type ResultBuilder() =
    member this.Bind(m, f) =
        match m with
        | Error e -> Error e
        | Ok a -> f a

    member this.Return(x) =
        Ok x

let result = new ResultBuilder()

let processInputDir maybeDirPath =
    let tryDirInfo dirPath =
        match dirPath |> Directory.Exists with
        | true -> Ok <| DirectoryInfo(dirPath)
        | _ -> Error "The directory does not exist."

    let summarizeFile (fi:FileInfo) =
        let createdAt = fi.CreationTime
        let modifiedAt = fi.LastWriteTime
        sprintf "\"%s\",%A,%A" (fi.FullName) createdAt modifiedAt

    match tryDirInfo maybeDirPath with
    | Error e -> Error e
    | Ok dirInfo ->
        dirInfo.GetFiles("*.*", EnumerationOptions(RecurseSubdirectories = true))
        |> Array.map summarizeFile
        |> Ok

let inputDir =
    let args = fsi.CommandLineArgs |> Array.tail
    match args.Length with
    | 1 -> Ok args[0]
    | _ -> Error "Pass a single directory path as an argument."


match inputDir with
| Error e -> printfn $"{e}"
| Ok d ->
    match processInputDir d with
    | Error e -> printfn $"Error: {e}"
    | Ok lines ->
        lines
        |> Array.iter (fun l -> l |> printfn "%s")
