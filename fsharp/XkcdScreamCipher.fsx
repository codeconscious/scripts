(* F# IMPLEMENTATION OF XKCD'S SCREAM CIPHER (as seen at https://xkcd.com/3054/)
   • Replaces all letters in strings with A's containing various diacritics (e.g., "A̋", "A̧", "A̤").
   • Decodes such strings, returning them back to their original letters.
   • Non-supported characters (digits, punctuation, etc.) are not converted and are used as-is.
   • Multiple strings can be passed at once.
   • Requirement: .NET 9 runtime
   • Usage: dotnet fsi XkcdScreamCipher.fsx [--encode|--decode|--test] input [optionalInputs...]

   References and thanks:
   • My blog post about this: https://codeconscious.github.io/2025/02/23/xkcd-scream-cipher.html
   • Special thanks to FrostBird347 on GitHub, whose JavaScript implementation saved
     considerable time adn trouble gathering the necessary variants of `A`.
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

    // Abbreviated versions are currently unsupported due to an obscure bug involving "-d".
    // (See https://github.com/dotnet/fsharp/issues/10819 for more.)
    let supportedFlags = Map.ofList [
            "--encode", Encode
            "--decode", Decode
            "--test",   Test
        ]

    let private validateArgCount (args: string array) =
        let instructions =
            sprintf
                "Supply an operation flag and at least one string to convert.\nSupported flags: %s"
                (String.Join(", ", supportedFlags.Keys))

        match args.Length with
        | 0 -> Error $"No arguments were passed. {instructions}"
        | 1 -> Error $"Not enough arguments were passed. {instructions}"
        | _ -> Ok args

    let private validateFlag flag =
        if supportedFlags.ContainsKey flag
        then Ok supportedFlags[flag]
        else
            let flagSummary = String.Join(", ", supportedFlags.Keys)
            Error $"Unsupported flag \"%s{flag}\". You must use one of the following: {flagSummary}."

    let private validateInputs (inputs: string array) =
        let allToUpper (inputs: string array) =
            inputs
            |> Array.map (fun x -> x.ToUpperInvariant())

        if inputs.Length = 0
        then Error "No inputs to convert were passed."
        else Ok (inputs |> allToUpper)

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
        let convert ch =
            let asStr = ch.ToString()
            let encodingMap = encodingPairs |> Map.ofList

            match encodingMap.TryGetValue asStr with
            | true, found -> found
            | false, _ -> asStr

        unencodedText
        |> Seq.map convert
        |> fun x -> x |> String.concat String.Empty

    let decode (encodedText: string) =
        let decodingMap =
            // For detecting some additional characters that might be used instead of the expected ones.
            let extraDecodingPairs = [
                "A̸", "Z"
                "A̅", "T"
                "Ȧ", "B"
                "Á", "E"
                "Ả", "I"
                "Ạ", "K"
                "Ă", "L"
                "Ǎ", "M"
                "Â", "N"
                "Å", "O"
                "Ȃ", "R"
                "Ã", "S"
                "Ā", "T"
                "Ä", "U"
                "À", "V"
                "Ȁ", "W"
                "A̱", "D"
                "A̲", "D"
            ]

            encodingPairs
            |> List.map (fun (x, y) -> y, x) // Flip the pairs.
            |> List.append extraDecodingPairs
            |> Map.ofList

        let convert text =
            match decodingMap.TryGetValue text with
            | true, found -> found
            | false, _ -> text

        let stringInfo = StringInfo encodedText

        [| 0 .. stringInfo.LengthInTextElements - 1 |]
        |> Array.map (fun i -> stringInfo.SubstringByTextElements(i, 1))
        |> Array.map convert
        |> String.Concat

    // Confirms that provided input is correctly encoded and decoded to its original value.
    let test unencodedText =
        let encodedText = encode unencodedText
        let decodedText = decode encodedText
        let result =
            if unencodedText.Equals(decodedText, StringComparison.InvariantCultureIgnoreCase)
            then "OK"
            else "ERROR"

        $"{result}: {unencodedText} --> {encodedText} --> {decodedText}"

open ArgValidation
open Encoding

match validate fsi.CommandLineArgs with
| Error e ->
    printfn $"Error: %s{e}"
    1
| Ok args ->
    let operation =
        match args.Operation with
        | Encode -> encode
        | Decode -> decode
        | Test -> test

    args.Inputs
    |> Array.map operation
    |> Array.iter (fun text -> printfn $"%s{text}")
    0
