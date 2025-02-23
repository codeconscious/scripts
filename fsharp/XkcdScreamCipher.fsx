(* F# implementation of xkcd's Scream Cipher
   • Replaces all letters in strings with A's containing various diacritics (e.g., "A̋", "A̧", "A̤").
   • Decodes such strings to restore encoded letters.
   • Non-supported characters (digits, punctuation, etc.) are not converted.
   • Multiple strings can be passed.
   • Requirement: .NET 9 runtime

   References and thanks:
   • Source comic for the cipher: https://xkcd.com/3054/
   • Special thanks to FrostBird347 on GitHub, whose own JavaScript implementation
     saved me considerable time gathering the necessary variants of the letter "A"!
     (Source: https://gist.github.com/FrostBird347/e7c017d096b3b50a75f5dcd5b4d08b99)
*)

open System
open System.Globalization

module ArgValidation =
    type private ResultBuilder() =
        member this.Bind(m, f) =
            match m with
            | Error e -> Error e
            | Ok a -> f a

        member this.Return(x) =
            Ok x

    type Operation = Encode | Decode | Test
    type ValidatedArgs = { Operation: Operation; Inputs: string array }

    let private result = ResultBuilder()

    let private validateArgCount (args: string array) =
        match args.Length with
        | 0 -> Error "No arguments were passed. You must pass (1) an operation flag and (2) at least one input string."
        | 1 -> Error "Not enough arguments were passed. You must pass (1) an operation flag and (2) at least one input string."
        | _ -> Ok args

    let private validateFlag flag =
        // Abbreviated versions are currently unsupported due to an obscure .NET half-bug involving "-d".
        // (See https://github.com/dotnet/fsharp/issues/10819 for more.)
        let supportedFlags = Map.ofList [
            "--encode", Encode
            "--decode", Decode
            "--test",   Test
        ]

        if supportedFlags.ContainsKey flag
        then Ok supportedFlags[flag]
        else
            let supportedFlagSummary = String.Join(", ", supportedFlags.Keys)
            Error $"Unsupported flag \"%s{flag}\". You must use one of the following: {supportedFlagSummary}."

    let private validateInputs (inputs: string array) =
        if inputs.Length = 0
        then Error "No inputs to convert were passed."
        else Ok inputs

    let validate (rawArgs: string array) =
        result {
            let! args = validateArgCount rawArgs[1..] // The head contains the script filename.
            let flag, inputs = args[0], args[1..]
            let! operation = validateFlag flag
            let! inputs' = validateInputs inputs
            return { Operation = operation; Inputs = inputs' }
        }

module Encoding =
    let private encodingPairs = [
            ("A", "A"); ("B", "Ȧ"); ("C", "A̧"); ("D", "A̠"); ("E", "Á"); ("F", "A̮")
            ("G", "A̋"); ("H", "A̰"); ("I", "Ả"); ("J", "A̓"); ("K", "Ạ"); ("L", "Ă")
            ("M", "Ǎ"); ("N", "Â"); ("O", "Å"); ("P", "A̯"); ("Q", "A̤"); ("R", "Ȃ")
            ("S", "Ã"); ("T", "Ā"); ("U", "Ä"); ("V", "À"); ("W", "Ȁ"); ("X", "A̽")
            ("Y", "A̦"); ("Z", "A̷")
        ]

    let encode (unencodedText: string) =
        let encodingMap = encodingPairs |> Map.ofList

        let convert (ch: char) =
            let asStr = ch.ToString()
            match encodingMap.TryGetValue asStr with
            | true, found -> found
            | false, _ -> asStr

        unencodedText
        |> _.ToUpperInvariant()
        |> Seq.map convert
        |> fun x -> x |> String.concat String.Empty

    let decode (encodedText: string) =
        // For detecting some additional characters that might be used instead of the expected ones.
        let extraDecodingPairs = [
            ("A̸", "Z")
            ("A̅", "T")
            ("Ȧ", "B")
            ("Á", "E")
            ("Ả", "I")
            ("Ạ", "K")
            ("Ă", "L")
            ("Ǎ", "M")
            ("Â", "N")
            ("Å", "O")
            ("Ȃ", "R")
            ("Ã", "S")
            ("Ā", "T")
            ("Ä", "U")
            ("À", "V")
            ("Ȁ", "W")
            ("A̱", "D")
            ("A̲", "D")
        ]

        let decodingMap =
            let reversedEncodingPairs = encodingPairs |> List.map (fun (x, y) -> y, x)
            Map.ofList <| reversedEncodingPairs @ extraDecodingPairs

        let stringInfo = StringInfo encodedText

        [| 0 .. stringInfo.LengthInTextElements - 1 |]
        |> Array.map (fun i -> stringInfo.SubstringByTextElements(i, 1))
        |> Array.map (fun substring ->
            match decodingMap.TryGetValue substring with
            | true, found -> found
            | false, _ -> substring)
        |> String.Concat

    // Confirms that provided input is correctly encoded and decoded to its original value.
    // The check is case-insensitive.
    let test (unencodedText: string) =
        let encodedText = encode unencodedText
        let decodedText = decode encodedText
        let result =
            if unencodedText.Equals(decodedText, StringComparison.InvariantCultureIgnoreCase)
            then "OK"
            else "ERROR"

        $"{result}: {unencodedText} --> {encodedText} --> {decodedText}"

module Printing =
    // Be careful using color because we don't know the user's terminal color scheme,
    // so it's possible to output text that is invisible to them or otherwise hard to read.
    let private printLineColor color msg =
        match color with
        | Some c ->
            Console.ForegroundColor <- c
            printfn $"%s{msg}"
            Console.ResetColor()
        | None -> printfn $"%s{msg}"

    let printLine msg =
        printLineColor None msg

    let printError msg =
        printLineColor (Some ConsoleColor.Red) msg

open ArgValidation
open Encoding
open Printing

match validate fsi.CommandLineArgs with
| Error e ->
    printError $"Error: %s{e}"
    1
| Ok args ->
    let operation =
        match args.Operation with
        | Encode -> encode
        | Decode -> decode
        | Test -> test

    args.Inputs
    |> Array.map operation
    |> Array.iter printLine
    0
