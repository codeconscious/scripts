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

type columnWidths = { First: int; Second: int; Third: int }

let printGroup (group:(string * Entry array)) =
    let category, entries = group
    let padding = 1

    let getColumnWidth (entries:Entry array) (item:Entry -> int) =
        entries
        |> Array.map item
        |> Array.max
        |> (+) padding
    let columnWidths =
        let entryColumnWidth = getColumnWidth entries
        let name = entryColumnWidth <| fun e -> e.Event.Name.Length
        let date = entryColumnWidth <| fun e -> e.Event.Date.ToString().Length
        let dayNo = entryColumnWidth  <| fun e -> e.Event.DayNumber.ToString().Length
        { First = name
          Second = date
          Third = dayNo }

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
            columnWidths.First
            entry.Event.Name
            columnWidths.Second
            (entry.Event.Date.ToString dateFormat)
            columnWidths.Third
            (entry.Event.DayNumber |> thousands)
            (summarizeMilestone entry.Milestone)

    printfn $"\n{category.ToUpperInvariant()}"
    printfn "%s" (new String('-', category.Length))
    entries |> Array.iter (fun d -> d |> printEntry columnWidths)

match entryGroups with
| Ok g -> g |> Array.iter printGroup
| Error e -> printfn $"ERROR: {e}"
