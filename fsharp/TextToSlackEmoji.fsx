module Styles =
    let letters = [ 'a' .. 'z' ]
    let numbers = [ '0' .. '9' ]
    let [<Literal>] Space = ' '

    type Style = {
        Name: string
        SupportedChars: char list
        Converter: char -> string
    }

    let styles =
        [
            {   Name = "cookie"
                SupportedChars = letters @ [Space; '!'; '?'; '&'; '•']
                Converter =
                    fun ch ->
                        match ch with
                        | Space -> ":blank:"
                        | '!' -> ":cookie-exclaim:"
                        | '?' -> ":cookie-question:"
                        | '&' -> ":cookie-and:"
                        | '•' -> ":cookie-dot:"
                        | _ -> $":cookie-{ch}:"
            };
            {   Name = "bluebox"
                SupportedChars = letters @ [Space]
                Converter =
                    fun ch ->
                        match ch with
                        | Space -> ":blank: "
                        | 'o' -> ":alpha-0: "
                        | _ -> $":alpha-{ch}: "
            };
            {   Name = "alphawhite"
                SupportedChars = letters @ [Space; '!'; '?'; '#'; '@']
                Converter =
                    fun ch ->
                        match ch with
                        | Space -> ":blank:"
                        | '!' -> ":alphabet-white-exclamation:"
                        | '?' -> ":alphabet-white-question:"
                        | '#' -> ":alphabet-white-hash:"
                        | '@' -> ":alphabet-white-at:"
                        | _ -> $":alphabet-white-{ch}:"
            };
            {   Name = "alphayellow"
                SupportedChars = letters @ [Space; '!'; '?'; '#'; '@'; 'ñ']
                Converter =
                    fun ch ->
                        match ch with
                        | Space -> ":blank:"
                        | '!' -> ":alphabet-yellow-exclamation:"
                        | '?' -> ":alphabet-yellow-question:"
                        | '#' -> ":alphabet-yellow-hash:"
                        | '@' -> ":alphabet-yellow-at:"
                        | 'ñ' -> ":alphabet-yellow-nñ:"
                        | _ -> $":alphabet-yellow-{ch}:"
            };
            {   Name = "alphasnow"
                SupportedChars = letters @ [Space]
                Converter =
                    fun ch ->
                        match ch with
                        | Space -> ":blank:"
                        | 'i' -> ":alpha_snow_i2:"
                        | _ -> $":alpha_snow_{ch}:"
            };
            {   Name = "tiles"
                SupportedChars = letters @ [Space; 'ñ']
                Converter =
                    fun ch ->
                        match ch with
                        | Space -> ":letter_blank:"
                        | 'ñ' -> ":letterñ:"
                        | _ -> $":letter_{ch}:"
            };
            {   Name = "pokemon"
                SupportedChars = letters @ [Space]
                Converter =
                    fun ch ->
                        match ch with
                        | Space -> ":blank:"
                        | _ -> $":pokemonfont-{ch}:"
            };
            {   Name = "merah"
                SupportedChars = letters @ [Space]
                Converter =
                    fun ch ->
                        match ch with
                        | Space -> ":blank:"
                        // The "merah" set is incomplete and inconsistently named.
                        | 'b' -> ":merahbb:"
                        | 'd' -> ":merahdd:"
                        | 'e' -> ":merahee:"
                        | 'f' -> ":ff:"
                        | 'h' -> ":merahhh:"
                        | 'k' -> ":merahkk:"
                        | 'l' -> ":merahll:"
                        | 'q' -> ":alpha-q:"
                        | 'r' -> ":merahrr:"
                        | 's' -> ":merahsss:"
                        | 'u' -> ":merahuuu:"
                        | 'x' -> ":alpha-x:"
                        | 'y' -> ":merahhyyy:"
                        | 'z' -> ":alpha-z:"
                        | _ -> $":merah{ch}:"
            };
            {   Name = "magazine"
                SupportedChars = letters @ [Space]
                Converter =
                    fun ch ->
                        match ch with
                        | Space -> ":blank:"
                        | _ -> $":magazine_{ch}:"
            };
            {   Name = "custom"
                SupportedChars = letters @ numbers @ [Space; '?'; '!'];
                Converter =
                    fun ch ->
                        match ch with
                        | Space -> ":blank:"
                        | 'a' -> ":m-a:"
                        | 'b' -> ":b4:"
                        | 'c' -> ":c2:"
                        | 'd' -> ":devo-d:"
                        | 'e' -> ":edge:"
                        | 'f' -> ":ff:"
                        | 'g' -> ":g-neon:"
                        | 'h' -> ":h:"
                        | 'i' -> ":info:"
                        | 'j' -> ":super-j:"
                        | 'k' -> ":m'kay:"
                        | 'l' -> ":labsslide-1:"
                        | 'm' -> ":m:"
                        | 'n' -> ":n64:"
                        | 'o' -> ":o:"
                        | 'p' -> ":p2:"
                        | 'q' -> ":qflash:"
                        | 'r' -> ":r:"
                        | 's' -> ":scon:"
                        | 't' -> ":kid-t:"
                        | 'u' -> ":m-u:"
                        | 'v' -> ":devo-v:"
                        | 'w' -> ":walphabet:"
                        | 'x' -> ":x:"
                        | 'y' -> ":y1:"
                        | 'z' -> ":zelle_onfire:"
                        | '0' -> ":0_bats:"
                        | '1' -> ":number-1-red:"
                        | '2' -> ":number2:"
                        | '3' -> ":number-3-flip:"
                        | '4' -> ":mana-4:"
                        | '5' -> ":round-red-5:"
                        | '6' -> ":mana-6:"
                        | '7' -> ":7-up:"
                        | '8' -> ":8flower:"
                        | '9' -> ":9lego:"
                        | '!' -> ":exclamation:"
                        | '?' -> ":question-icon:"
                        | _ -> string ch // Should not be reached.
            };
            {   Name = "numbers"
                SupportedChars = numbers @ [Space]
                Converter =
                    fun ch ->
                        match ch with
                        | Space -> ":blank:"
                        | _ -> $":num{ch}:"
            };
            {   Name = "squarenumbers"
                SupportedChars = numbers @ [Space]
                Converter =
                    fun ch ->
                        match ch with
                        | Space -> ":blank:"
                        | '0' -> ":zero:"
                        | '1' -> ":one:"
                        | '2' -> ":two:"
                        | '3' -> ":three:"
                        | '4' -> ":four:"
                        | '5' -> ":five:"
                        | '6' -> ":six:"
                        | '7' -> ":seven:"
                        | '8' -> ":eight:"
                        | _ -> "nine"
            };
        ]

    let styleNames = styles |> List.map _.Name

module ArgValidation =
    open System
    open Styles

    type UserArgs = { Style: string; Text: string }

    type ResultBuilder() =
        member this.Bind(m, f) =
            match m with
            | Error e -> Error e
            | Ok a -> f a

        member this.Return(x) =
            Ok x

    let result = new ResultBuilder()

    let validate =
        let supportedStyleNames =
            String.Join(
                Space,
                styles |> List.map _.Name
            )

        let rawArgs =
            fsi.CommandLineArgs
            |> Array.toList
            |> List.tail // The head contains the script filename.

        let argCount (rawArgs:string list) =
            let errorText =
                System.String.Join(
                    Environment.NewLine,
                    ["Pass in (1) a style name and (2) a string containing only supported characters for that style.";
                    $"Supported styles: {supportedStyleNames}"]
                )

            match rawArgs.Length with
            | l when l = 2 ->
                Ok <|
                {
                    Style = rawArgs.Head.ToLowerInvariant();
                    Text = rawArgs[1].ToLowerInvariant()
                }
            | _ -> Error errorText

        let styleName args =
            let errorText =
                System.String.Join(
                    Environment.NewLine,
                    [$"Style \"{args.Style}\" not found.";
                    $"Supported styles: {supportedStyleNames}"]
                )

            match styleNames |> List.contains args.Style with
            | true -> Ok args
            | false -> Error errorText

        let inputLength args =
            match args.Text.Length with
            | 0 -> Error "You must enter text to be converted."
            | _ -> Ok args

        let inputChars args =
            let style =
                styles
                |> List.filter (fun s -> s.Name = args.Style)
                |> List.head

            let ensureVisibleChar ch =
                if ch = Space then "<SPACE>" else $"{ch}"

            let invalidChars =
                args.Text
                |> Seq.toList
                |> List.filter (fun ch -> not <| List.contains ch style.SupportedChars)
                |> List.map ensureVisibleChar

            let error style =
                let supportedChars style =
                    let chars =
                        style.SupportedChars
                        |> List.map ensureVisibleChar
                    String.Join(Space, chars)

                String.Join(
                    Environment.NewLine,
                    [$"Invalid characters found for style \"{style.Name}\": {String.Join(Space, invalidChars)}";
                    $"Supported characters: {style |> supportedChars}"]
                )

            if invalidChars.Length = 0
            then Ok args
            else Error <| error style

        result {
            let! args = argCount rawArgs
            let! args' = styleName args
            let! args'' = inputLength args'
            let! args''' = inputChars args''
            return args'''
        }

open ArgValidation
open Styles

let args = validate

let getStyle args =
    styles
    |> List.filter (fun s -> s.Name = args.Style)
    |> List.head

let convertText args style =
    let text = args.Text
    let converter = style.Converter

    text
    |> Seq.map converter
    |> Seq.map string
    |> String.concat System.String.Empty

match args with
| Error e ->
    e |> eprintfn "%s"
| Ok a ->
    a
    |> getStyle
    |> convertText a
    |> printfn "%s"
