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
    { Category : string
      Description : string
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
    |> Array.map (fun p -> (p[0], p[1], p[2]))

let calc (pairs:(string * string * string) array) =
    let milestoneInterval = 1000
    let today = DateOnly.FromDateTime(DateTime.Now)

    let createEntry (pair:(string * string * string)) =
        let category, desc, dateText = pair
        let parsedDate = dateText |> DateOnly.Parse
        let dayNumber = today.DayNumber - parsedDate.DayNumber + 1
        { Category = category; Description = desc; Date = parsedDate; DayNumber = dayNumber }

    let createMilestone interval (entry:Entry) =
        let daysSince = entry.DayNumber
        let daysOf = daysSince + interval - (daysSince % interval)
        let daysUntil = daysOf - daysSince
        let date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(daysUntil))
        let milestone = { Date = date; DaysUntil = daysUntil; DaysOf = daysOf }
        { Entry = entry; Milestone = milestone }

    pairs
    |> Array.map createEntry
    |> Array.map (createMilestone milestoneInterval)

let pairs =
    result {
        let! text = readFile fileName
        let lines = text |> splitLines
        let pairs = lines |> splitToPairs
        let withValidDates = pairs |> Array.filter (fun x ->
            let _, _, dateText = x
            let isValid, _ = dateText |> DateOnly.TryParse
            isValid)
        let entriesWithMilestones =
            withValidDates
            |> calc
            |> Array.sortBy (fun x -> x.Entry.Date)
        return entriesWithMilestones
    }

let printCollection (data:EntryWithMilestone array) =
    let padding = 2
    let descColWidth = data |> Array.map (fun d -> d.Entry.Description.Length) |> Array.max |> (+) padding
    let dateColWidth = data |> Array.map (fun d -> d.Entry.Date.ToString().Length) |> Array.max |> (+) padding
    let dayNoColWidth = data |> Array.map (fun d -> d.Entry.DayNumber.ToString().Length) |> Array.max |> (+) padding
    let columnsWidths = {| First = descColWidth; Second = dateColWidth; Third = dayNoColWidth |}

    let printSingle columnWidths entryWithMilestone =
        let milestoneText milestone =
            sprintf $"{milestone.DaysOf} in {milestone.DaysUntil} days on {milestone.Date}"

        printfn "%-*s: %-*s | %*s | %s"
            columnsWidths.First
            entryWithMilestone.Entry.Description
            columnsWidths.Second
            (entryWithMilestone.Entry.Date.ToString())
            columnsWidths.Third
            (entryWithMilestone.Entry.DayNumber.ToString())
            (milestoneText entryWithMilestone.Milestone)

    data
    |> Array.iter (fun d -> d |> printSingle columnsWidths)


match pairs with
| Ok p -> p |> printCollection
| Error e -> printfn $"ERROR: {e}"
