(* DaysSince: An F# Script

   Summary: Reads a single file containing event names and dates and displays
            how many days have elapsed since and how many more will elapse
            until the next 1,000-day milestone. Only single-byte text is fully
            supported.

            Each line of the file must contain 3 comma-separated values:
            1. Event category name (Birthday, Event, etc.)
            2. Event name
            3. Event date in YYYY-MM-DD or MM-DD-YYYY format
            Sample line: `Birthday, Shigeru Miyamoto, 1952/11/16`

            Parsing support is very simple and nested commas and such are unsupported.
            Whitespace around commas is ignored. Invalid lines are also ignored.

   Requirements: .NET 8 runtime (Untested on previous versions, though it might work)

   Usage: dotnet fsi <filePath>

   Note: This was created mainly for learning purposes and might not be very good. ^_^
*)

open System
open System.IO

type ResultBuilder() =
    member this.Bind(m, f) =
        match m with
        | Error e -> Error e
        | Ok a -> f a

    member this.Return(x) =
        Ok x

let result = new ResultBuilder()

type Event =
    { Category : string
      Name : string
      Date : DateOnly
      DayNumber : int }

type Milestone =
    { Date : DateOnly
      DaysUntil : int
      DayNumber : int }

type Entry = // TODO: Think of a better name, if possible.
    { Event : Event
      Milestone : Milestone }

let fileName =
    fsi.CommandLineArgs
    |> Array.tail
    |> Array.head

let entryGroups =
    let readFile fileName =
        try
            fileName
            |> File.ReadAllText
            |> Ok
        with
            | :? FileNotFoundException -> Error $"\"{fileName}\" was not found."
            | e -> Error $"File access failed: {e.Message}"

    let splitLines (text:string) =
        let options = StringSplitOptions.TrimEntries ||| StringSplitOptions.RemoveEmptyEntries
        text.Split(Environment.NewLine, options)

    let splitByComma (text:string[]) =
        text
        |> Array.map (fun t -> t.Split(',', StringSplitOptions.TrimEntries))

    let splitTriplets (text:(string array) array) =
        text
        |> Array.filter (fun g -> g.Length = 3)
        |> Array.map (fun g -> (g[0], g[1], g[2]))

    let createEntries (groups:(string * string * string) array) =
        let milestoneInterval = 1000
        let today = DateTime.Now |> DateOnly.FromDateTime

        let createEvent (triplet:(string * string * string)) =
            let category, name, dateText = triplet
            let parsedDate = dateText |> DateOnly.Parse
            let dayNumber = today.DayNumber - parsedDate.DayNumber + 1
            { Category = category
              Name = name
              Date = parsedDate; DayNumber = dayNumber }

        let createMilestone interval (event:Event) =
            let daysSince = event.DayNumber
            let daysOf = daysSince + interval - (daysSince % interval)
            let daysUntil = daysOf - daysSince
            let date =
                daysUntil
                |> DateTime.Now.Date.AddDays
                |> DateOnly.FromDateTime
            let milestone =
                { Date = date
                  DaysUntil = daysUntil
                  DayNumber = daysOf }
            { Event = event
              Milestone = milestone }

        groups
        |> Array.map createEvent
        |> Array.map (createMilestone milestoneInterval)

    result { // TODO: Decide whether to remove this computation expression.
        let! text = readFile fileName
        let lines = text |> splitLines
        let uncommentedLines =
            lines
            |> Array.filter (fun l -> not <| l.StartsWith('#'))
        let groups = uncommentedLines |> splitByComma
        let validCountGroups: (string * string * string) array = groups |> splitTriplets
        let validDateGroups = // TODO: Avoid double-parsing dates.
            validCountGroups
            |> Array.filter (fun x ->
                let _, _, dateText = x
                let isValid, _ = dateText |> DateOnly.TryParse
                isValid)
        let sortedEntries =
            validDateGroups
            |> createEntries
            |> Array.groupBy (fun g -> g.Event.Category)
            |> Array.map
                (fun (k, e) ->
                    k, e |> Array.sortBy (fun e -> e.Event.Date))
        return sortedEntries
    }

let printGroup (group:(string * Entry array)) =
    let category, data = group
    let padding = 1

    // TODO: DRY up the next 3 functions.
    let nameColWidth =
        data
        |> Array.map (fun d -> d.Event.Name.Length)
        |> Array.max
        |> (+) padding
    let dateColWidth =
        data
        |> Array.map (fun d -> d.Event.Date.ToString().Length)
        |> Array.max
        |> (+) padding
    let dayNoColWidth =
        data
        |> Array.map (fun d -> d.Event.DayNumber.ToString().Length)
        |> Array.max
        |> (+) padding
    let columnsWidths =
        {| First = nameColWidth
           Second = dateColWidth
           Third = dayNoColWidth |}

    let thousands (x:int) =
        System.String.Format("{0:#,##0}", x)

    let printEntry columnWidths entry =
        let dateFormat = "yyyy-MM-dd"

        let summarizeMilestone milestone =
            sprintf "%s in %s days on %s"
                (milestone.DayNumber |> thousands)
                (milestone.DaysUntil |> thousands)
                (milestone.Date.ToString dateFormat)

        printfn "%-*s | %-*s | %*s | %s"
            columnsWidths.First
            entry.Event.Name
            columnsWidths.Second
            (entry.Event.Date.ToString dateFormat)
            columnsWidths.Third
            (entry.Event.DayNumber |> thousands)
            (summarizeMilestone entry.Milestone)

    printfn $"\n{category.ToUpperInvariant()}"
    printfn "%s" (new String('-', category.Length))
    data |> Array.iter (fun d -> d |> printEntry columnsWidths)

match entryGroups with
| Ok g -> g |> Array.iter printGroup
| Error e -> printfn $"ERROR: {e}"