(* Enquoten: An F# Script

   Summary: Reads a single text file and splits its lines to the maximum line length
            limit provided. The prefix is included in the line length calculation, and it
            must be shorter then the line length limit. The script will try to split lines
            at spaces, but if none are found, it will cut the line at the line length limit.

   Requirements: .NET 8 runtime (Untested on previous versions, though it might work)

   Usage: dotnet fsi <lineLengthLimit> <quotePrefix> <filePath>
          Sample: dotnet fsi 70 "> " file1.txt

   Note: This was created mainly for learning purposes and might not be very good. ^_^

   Last significant update: May 4, 2024, to add computation expressions and allow custom prefixes.
*)

open System
open System.IO

type Args<'T> =
    {
        /// The maximum length each line of text should be, including the prefix text.
        Limit: 'T
        /// A prefix (such as "> ") for each line.
        Prefix: string
        /// The name of the comma-separated input file.
        File: string
    }

module ArgValidation =
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

        let validateArgCount (args:string list) =
            match args.Length with
            | l when l = 3 ->
                Ok { Limit = args.Head; Prefix = args[1]; File = args[2] }
            | _ ->
                Error "You must supply a maximum line length, a quote prefix (such as '> '), and one file path."

        let validateLimit (args:Args<string>) =
            match args with
            | { Args.Limit = l; Prefix = p; Args.File = f } ->
                match (l |> System.Int32.TryParse) with
                | true, i when i >= 2 -> Ok { Limit = i; Prefix = p; File = f }
                | true, _ -> Error "Requested line length too short."
                | _ -> Error "Maximum line length must be numeric."

        let validatePrefix (args:Args<int>) =
            match args with
            | { Prefix = p; Limit = l } when p.Length < l ->
                Ok args
            | _ ->
                Error (sprintf "The prefix text length (%i) must be at least 1 character less than the line length limit (%i)."
                    args.Prefix.Length
                    args.Limit)

        let validateFileExists (args:Args<int>) =
            match args with
            | { Args.Limit = _; Args.File = f } ->
                match f |> File.Exists with
                | true -> Ok args
                | false -> Error $"The file \"{f}\" does not exist."

        result {
            let! args = validateArgCount rawArgs
            let! args' = validateLimit args
            let! args'' = validatePrefix args'
            let! args''' = validateFileExists args''
            return args'''
        }

module Io =
    let readFile filePath =
        try
            filePath
            |> File.ReadAllText
            |> Ok
        with
            | :? FileNotFoundException -> Error $"\"{filePath}\" was not found."
            | e -> Error $"Unexpectedly could not read \"{filePath}\": {e.Message}"

module Quoting =
    let processFileText (args:Args<int>) (text:string) =
        let enquoten prefix text =
            text |> sprintf "%s%s" prefix

        let splitLine fullLine (args:Args<int>) =
            let finalSpaceIndex (text:string) (indexCeiling:int) =
                let index = text.LastIndexOf(" ", indexCeiling) // Searches backwards
                match text.LastIndexOf(" ", indexCeiling) with
                | -1 -> indexCeiling - 1
                | _  -> index

            let rec loop acc (linePart:string) limit =
                match linePart.Length with
                | l when l <= limit ->
                    acc @ [linePart]
                | _ ->
                    let splitIndex = finalSpaceIndex linePart limit
                    let trimmed = linePart[..splitIndex]
                    let remaining = linePart[splitIndex+1..]
                    loop (acc @ [trimmed]) remaining limit
            loop [] fullLine <| args.Limit - args.Prefix.Length

        text.Split Environment.NewLine
        |> Array.toList
        |> List.map (fun l -> splitLine l args)
        |> List.collect (fun l -> l)
        |> List.map (fun l -> enquoten args.Prefix l)

open ArgValidation
open Io
open Quoting

let output =
    result {
        let! okArgs = validateArgs
        let! fileText = readFile okArgs.File
        return fileText |> processFileText okArgs
    }

match output with
| Ok lines -> lines |> List.iter (fun l -> l |> printfn "%s")
| Error e -> e |> printfn "%s"
