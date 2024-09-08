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
    let isHiddenDir (path: string) : bool =
        let itemName = DirectoryInfo(path).Name
        // printfn $"isHidden path: {path}"
        // printfn $"isHidden itemName: {itemName}"
        match itemName with
        | p when p[0] = '.' ->
            // printfn $"itemName1: {itemName}"
            true
        | _ ->
            // printfn $"itemName2: {itemName}"
            let attrs = File.GetAttributes(path)
            attrs.HasFlag(FileAttributes.Hidden)

    let isHiddenFile (path: string) : bool =
        let itemName = Path.GetFileName(path)
        match itemName with
        | p when p[0] = '.' ->
            // printfn $"itemName1: {itemName}"
            true
        | _ ->
            // printfn $"itemName2: {itemName}"
            let attrs = File.GetAttributes(path)
            attrs.HasFlag(FileAttributes.Hidden)

    seq {
        let currentDirIsHidden = isChildOfHidden || dir |> isHiddenDir
        // printfn $"<DEBUG> Dir: {dir}  |  Hidden: {currentDirIsHidden}  |  isChildOfHidden: {isChildOfHidden}"

        for d in Directory.EnumerateDirectories(dir) do
            let childDirIsHidden = isChildOfHidden || d |> isHiddenDir
            yield! allDirectoryItems d pattern childDirIsHidden
            yield if childDirIsHidden then HiddenDirectory d else Directory d
            // let a = if isHiddenDirBool then HiddenDirectory d else Directory d
            // printfn $"<DEBUG d> {d} is {a}"

        let files = Directory.EnumerateFiles(dir, pattern)
        yield! match currentDirIsHidden with
               | true  -> files |> Seq.map (fun p -> HiddenFile p)
               | false -> files |> Seq.map (fun p -> if p |> isHiddenFile then HiddenFile p else File p)
    }

let rename path =
    let newGuid () = Guid.NewGuid().ToString()

    let renameFile (oldName: string) =
        try
            let dir = Path.GetDirectoryName(oldName)
            let ext = Path.GetExtension(oldName) // Includes initial period
            let newName = $"{newGuid()}{ext}"
            let newPath = Path.Combine(dir, newName)
            // File.Move(oldName, newPath)
            Renamed $"File \"{oldName}\" → \"{newPath}\""
        with
            | :? FileNotFoundException -> Failed $"File \"{oldName}\" was not found."
            | e -> Failed $"Failure renaming file \"{oldName}\": {e.Message}"

    let renameDir (oldName: string) =
        try
            let dir = Path.GetDirectoryName(oldName)
            let newName = newGuid()
            let newPath = Path.Combine(dir, newName)
            // Directory.Move(oldName, newPath)
            Renamed $"Directory \"{oldName}\" → \"{newPath}\""
        with
            | :? FileNotFoundException -> Failed $"Directory \"{oldName}\" was not found."
            | e -> Failed $"Failure renaming directory \"{oldName}\": {e.Message}"

    match path with
    | File f -> renameFile f
    | Directory d -> renameDir d
    | HiddenFile f -> Ignored <| sprintf $"Hidden file \"{f}\""
    | HiddenDirectory d -> Ignored <| sprintf $"Hidden directory \"{d}\""

let print = function
    | Renamed msg ->
        printfn "[Renamed] %s" msg
    | Ignored msg ->
        Console.ForegroundColor <- ConsoleColor.DarkGray
        printfn "[Ignored] %s" msg
        Console.ResetColor()
    | Failed e ->
        Console.ForegroundColor <- ConsoleColor.Red
        printfn "[Error] %s" e
        Console.ResetColor()

allDirectoryItems "/Users/jd/Downloads/generated_files/" "*" false
// |> Seq.iter (fun di -> printfn $"{di}")
|> Seq.map (fun itemInDir -> rename itemInDir)
|> Seq.iter (fun result -> print result)
