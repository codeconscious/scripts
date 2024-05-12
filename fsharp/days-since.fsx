open System.IO
let readFile fileName =
    try
        fileName
        |> File.ReadAllText
        |> Ok
    with
        | :? FileNotFoundException -> Error $"\"{fileName}\" was not found."
        | e -> Error $"Unexpectedly could not read \"{fileName}\": {e.Message}"

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
      Date : DateOnly
      DayNumber : int }

type Milestone =
    { Date : DateOnly
      DaysUntil : int
      DaysOf : int }

type EntryWithMilestone =
    { Entry : Entry
      Milestone : Milestone }

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

let calc (pairs:(string * string) array) =
    let milestoneInterval = 1000
    let today = DateOnly.FromDateTime(DateTime.Now)

    let createEntry (pair:(string * string)) =
        let parsedDate = DateOnly.Parse(snd pair)
        let since = today.DayNumber - parsedDate.DayNumber + 1
        { Description = fst pair; Date = parsedDate; DayNumber = since }

    let createMilestone interval (entry:Entry) =
        // DayCount = daysSince + _interval - (daysSince % _interval);
        // DaysUntil = DayCount - daysSince;
        // Date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(DaysUntil));
        let daysSince = today.DayNumber - entry.Date.DayNumber + 1;
        let daysOf = daysSince + interval - (daysSince % interval)
        let daysUntil = daysOf - daysSince
        let date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(daysUntil))
        let milestone = { Date = date; DaysUntil = daysUntil; DaysOf = daysOf }
        { Entry = entry; Milestone = milestone }

    pairs
    |> Array.map createEntry
    |> Array.map (fun x -> x |> createMilestone milestoneInterval)

let pairs =
    result {
        let! text = readFile fileName
        let lines = text |> splitLines
        let pairs = lines |> splitToPairs
        let withValidDates = pairs |> Array.filter (fun x ->
            let isValid, _ = DateOnly.TryParse(snd x)
            isValid)
        printfn $"{withValidDates.Length} withValidDate(s) found"
        let entriesWithMilestones = withValidDates |> calc
        // let pairs =
        //     withValidDates
        //     |> Array.map (fun p -> {Description = fst p; Date = DateOnly.Parse(snd p)})
        //     |> Array.sortBy (fun p -> p.Date)
        printfn $"{entriesWithMilestones.Length} entriesWithMilestone(s) found"
        return entriesWithMilestones
    }

match pairs with
| Ok p -> p |> Array.iter (fun item -> printfn $"{item.Entry.Date.ToString()}: {item.Entry.Description}")
| Error e -> printfn $"ERROR: {e}"
