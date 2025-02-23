(* F# implementation of xkcd's Scream Cipher
   • Replaces all letters in strings with A's containing various diacritics (e.g., "A̋", "A̧", "A̤").
   • Decodes such strings to restore encoded letters.
   • Non-supported characters (digits, punctuation, etc.) are not converted.
   • Multiple strings can be passed.
   • Requirement: .NET 9 runtime
   • Source comic for the cipher: https://xkcd.com/3054/
   • Special thanks to FrostBird347 on GitHub, whose own JavaScript implementation
     saved me considerable time gathering the necessary variants of the letter "A"!
     (Source: https://gist.github.com/FrostBird347/e7c017d096b3b50a75f5dcd5b4d08b99)
*)

open System
open System.Globalization

let encodingPairs = [
        ("A", "A"); ("B", "Ȧ"); ("C", "A̧"); ("D", "A̠"); ("E", "Á"); ("F", "A̮")
        ("G", "A̋"); ("H", "A̰"); ("I", "Ả"); ("J", "A̓"); ("K", "Ạ"); ("L", "Ă")
        ("M", "Ǎ"); ("N", "Â"); ("O", "Å"); ("P", "A̯"); ("Q", "A̤"); ("R", "Ȃ")
        ("S", "Ã"); ("T", "Ā"); ("U", "Ä"); ("V", "À"); ("W", "Ȁ"); ("X", "A̽")
        ("Y", "A̦"); ("Z", "A̷")
    ]

let encode (text: string) =
    let encodingMap = encodingPairs |> Map.ofList

    text
    |> _.ToUpperInvariant()
    |> Seq.map (fun ch ->
        match encodingMap.TryGetValue (ch.ToString()) with
        | true, found -> found
        | false, _ -> ch.ToString())
    |> fun x -> x |> String.concat String.Empty

let decode (text: string) =
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
        reversedEncodingPairs @ extraDecodingPairs |> Map.ofList

    let stringInfo = StringInfo text

    [| 0 .. stringInfo.LengthInTextElements - 1 |]
    |> Array.map (fun i -> stringInfo.SubstringByTextElements(i, 1))
    |> Array.map (fun substring ->
        match decodingMap.TryGetValue substring with
        | true, found -> found
        | false, _ -> substring)
    |> String.Concat

let args = fsi.CommandLineArgs |> Array.toList |> List.tail // The head contains the script filename.

args
|> List.map (fun x -> encode x)
|> List.iter (fun x -> printfn "%s" x)

args
|> List.map (fun x -> encode x)
|> List.map (fun x -> decode x)
|> List.iter (fun x -> printfn "%s" x)
