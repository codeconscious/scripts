module IO =
    open System.IO
    let readFile fileName =
        try
            fileName
            |> File.ReadAllText
            |> Ok
        with
            | :? FileNotFoundException -> Error $"\"{fileName}\" was not found."
            | e -> Error $"Unexpectedly could not read \"{fileName}\": {e.Message}"

module Main =
    open IO
    open System

    type ResultBuilder() =
        member this.Bind(m, f) =
            match m with
            | Error e -> Error e
            | Ok a -> f a

        member this.Return(x) =
            Ok x

    let result = new ResultBuilder()

    type Entry =
        { Description : string
          Date : DateOnly }

    let fileName =
        fsi.CommandLineArgs
        |> Array.toList
        |> List.tail
        |> List.head

    let splitLines (text:string) =
        text.Split('\n', StringSplitOptions.TrimEntries ||| StringSplitOptions.RemoveEmptyEntries)

    let splitToPairs (text:string[]) =
        text
        |> Array.map (fun t -> t.Split(',', StringSplitOptions.TrimEntries))
        |> Array.map (fun p -> (p[0], p[1]))

    // let convertToEntries (pairs:(string * string) array) =
    //     let isDateValid (maybeDate:string) =
    //         match maybeDate |> DateOnly.TryParse with
    //         | (true, d) -> Ok d
    //         | _ -> Error $"Invalid date: \"{maybeDate}\""

    //     pairs
    //     |> Array.partition (fun p -> (fst p) |> DateOnly.TryParse)

    let pairs =
        result {
            let! text = readFile fileName
            let lines = text |> splitLines
            let pairs = lines |> splitToPairs
            let withValidDates = pairs |> Array.filter (fun x ->
                let isValid, _ = DateOnly.TryParse(snd x)
                isValid)
            printfn $"{withValidDates.Length} withValidDate(s) found"
            let pairs =
                withValidDates
                |> Array.map (fun p -> {Description = fst p; Date = DateOnly.Parse(snd p)})
            printfn $"{pairs.Length} pair(s) found"
            return pairs
        }

    match pairs with
    | Ok p -> p |> Array.iter (fun item -> printfn $"{item.Date.ToString()}: {item.Description}")
    | Error e -> printfn $"ERROR: {e}"
