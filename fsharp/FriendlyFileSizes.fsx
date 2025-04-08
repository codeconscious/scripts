open System.Globalization

let formatInt64 (i: int64) : string =
    i.ToString("#,##0", CultureInfo.InvariantCulture)

let formatFloat (f: float) : string =
    f.ToString("#,##0.00", CultureInfo.InvariantCulture)

let private formatBytes (bytes: int64) =
    let kilobyte = 1024L
    let megabyte = kilobyte * 1024L
    let gigabyte = megabyte * 1024L
    let terabyte = gigabyte * 1024L

    match bytes with
    | _ when bytes >= terabyte -> sprintf "%s TB" (float bytes / float terabyte |> formatFloat)
    | _ when bytes >= gigabyte -> sprintf "%s GB" (float bytes / float gigabyte |> formatFloat)
    | _ when bytes >= megabyte -> sprintf "%s MB" (float bytes / float megabyte |> formatFloat)
    | _ when bytes >= kilobyte -> sprintf "%s KB" (float bytes / float kilobyte |> formatFloat)
    | _ -> sprintf "%s bytes" (bytes |> formatInt64)

printfn "%s" <| formatBytes (int64 fsi.CommandLineArgs[1])
