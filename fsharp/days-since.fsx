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

let calc (pairs:(string * string) array) =
    let milestoneInterval = 1000
    let today = DateOnly.FromDateTime(DateTime.Now)

    let createEntry (pair:(string * string)) =
        let parsedDate = DateOnly.Parse(snd pair)
        let dayNumber = today.DayNumber - parsedDate.DayNumber + 1
        { Description = fst pair; Date = parsedDate; DayNumber = dayNumber }

    let createMilestone interval (entry:Entry) =
        let daysSince = entry.DayNumber
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
        let entriesWithMilestones =
            withValidDates |> calc |> Array.sortBy (fun x -> x.Entry.Date)
        printfn $"{entriesWithMilestones.Length} entriesWithMilestone(s) found"
        return entriesWithMilestones
    }

let printData entryWithMilestone =
    let milestoneText milestone =
        sprintf $"{milestone.DaysOf} in {milestone.DaysUntil} days on {milestone.Date}"
    printfn $"{entryWithMilestone.Entry.Date.ToString()}: {entryWithMilestone.Entry.Description} | {entryWithMilestone.Entry.DayNumber} | {milestoneText entryWithMilestone.Milestone}"


match pairs with
| Ok p ->
    Array.iter printData p
| Error e -> printfn $"ERROR: {e}"
