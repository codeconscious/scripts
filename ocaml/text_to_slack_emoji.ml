let (<|) f x = f x

let space = ' '

module Styles = struct
    let letters = List.init 26 (fun i -> Char.chr (Char.code 'a' + i)) (* [ 'a' .. 'z' ] *)
    let numbers = List.init 10 (fun i -> Char.chr (Char.code '0' + i))

    type style = {
        name: string;
        supported_chars: char list;
        converter: char -> string
    }

    let styles =
        [
            { name = "cookie";
              supported_chars = letters @ [space; '!'; '?'; '&'; (* '•' *)];
              converter =
                    fun ch ->
                        match ch with
                        | c when c = space -> ":blank:"
                        | '!' -> ":cookie-exclaim:"
                        | '?' -> ":cookie-question:"
                        | '&' -> ":cookie-and:"
                        (* | '•' -> ":cookie-dot:" *)
                        | c -> Printf.sprintf ":cookie-{%c}:" c
            };
            { name = "bluebox";
              supported_chars = letters @ [space];
              converter =
                    fun ch ->
                        match ch with
                        | c when c = space -> ":blank: "
                        | 'o' -> ":alpha-0: "
                        | c -> Printf.sprintf ":alpha-{%c}:" c (* Hairspace *)
            };
            { name = "alphawhite";
              supported_chars = letters @ [space; '!'; '?'; '#'; '@'];
              converter =
                    fun ch ->
                        match ch with
                        | c when c = space -> ":blank:"
                        | '!' -> ":alphabet-white-exclamation:"
                        | '?' -> ":alphabet-white-question:"
                        | '#' -> ":alphabet-white-hash:"
                        | '@' -> ":alphabet-white-at:"
                        | c -> Printf.sprintf ":alphabet-white-{%c}:" c
            };
            { name = "alphayellow";
              supported_chars = letters @ [space; '!'; '?'; '#'; '@'; (* 'ñ' *)];
              converter =
                    fun ch ->
                        match ch with
                        | c when c = space -> ":blank:"
                        | '!' -> ":alphabet-yellow-exclamation:"
                        | '?' -> ":alphabet-yellow-question:"
                        | '#' -> ":alphabet-yellow-hash:"
                        | '@' -> ":alphabet-yellow-at:"
                        (* | 'ñ' -> ":alphabet-yellow-nñ:" *)
                        | c -> Printf.sprintf ":alphabet-yellow-{%c}:" c
            };
            { name = "alphasnow";
              supported_chars = letters @ [space];
              converter =
                    fun ch ->
                        match ch with
                        | c when c = space -> ":blank:"
                        | 'i' -> ":alpha_snow_i2:"
                        | c -> Printf.sprintf ":alpha_snow_{%c}:" c
            };
            { name = "tiles";
              supported_chars = letters @ [space; (* 'ñ' *)];
              converter =
                    fun ch ->
                        match ch with
                        | c when c = space -> ":letter_blank:"
                        (* | 'ñ' -> ":letterñ:" *)
                        | c -> Printf.sprintf ":letter_{%c}:" c
            };
            { name = "pokemon";
              supported_chars = letters @ [space];
              converter =
                    fun ch ->
                        match ch with
                        | c when c = space -> ":blank:"
                        | c -> Printf.sprintf ":pokemonfont-{%c}:" c
            };
            { name = "merah";
              supported_chars = letters @ [space];
              converter =
                    fun ch ->
                        match ch with
                        | c when c = space -> ":blank:"
                        (* The "merah" set is incomplete and inconsistently named. *)
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
                        | c -> Printf.sprintf ":merah{%c}:" c
            };
            { name = "magazine";
              supported_chars = letters @ [space];
              converter =
                    fun ch ->
                        match ch with
                        | c when c = space -> ":blank:"
                        | c -> Printf.sprintf ":magazine_{%c}:" c
            };
            { name = "custom";
              supported_chars = letters @ numbers @ [space; '?'; '!'];
              converter =
                    fun ch ->
                        match ch with
                        | c when c = space -> ":blank:"
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
                        | _ -> String.make 1 ch (* Should not be reached. *)
            };
            { name = "numbers";
              supported_chars = numbers @ [space];
              converter =
                    fun ch ->
                        match ch with
                        | c when c = space -> ":blank:"
                        | c -> Printf.sprintf ":num{%c}:" c
            };
            { name = "squarenumbers";
              supported_chars = numbers @ [space];
              converter =
                    fun ch ->
                        match ch with
                        | c when c = space -> ":blank:"
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

    let styleNames = styles |> List.map (fun s -> s.name)

end

module ArgValidation = struct
    (* open Result.Let_syntax *)
    open Styles

    type user_args = { style: string; text: string }

    let validate =
        let supportedStyleNames =
            String.concat " " (styles |> List.map (fun s -> s.name)) in


        let rawArgs =
            Sys.argv
            |> Array.to_list
            |> List.tl (* The head contains the script filename. *) in

        let argCount (rawArgs:string list) =
            let errorText =
                String.concat
                    "\n"
                    ["Pass in (1) a style name and (2) a string containing only supported characters for that style.";
                     Printf.sprintf "Supported styles: %s" supportedStyleNames] in

            match List.length rawArgs with
            | l when l = 2 ->
                Ok {
                    style = rawArgs |> List.hd |> String.lowercase_ascii;
                    text = List.nth rawArgs 1 |> String.lowercase_ascii
                }
            | _ -> Error errorText in

        let styleName args =
            let errorText =
                String.concat
                    "\n"
                    [Printf.sprintf "Style \"%s\" not found." args.style;
                     Printf.sprintf "Supported styles: %s" supportedStyleNames] in

            match styleNames |> List.mem args.style with
            | true -> Ok args
            | false -> Error errorText in

        let inputLength args =
            match String.length args.text with
            | 0 -> Error "You must enter text to be converted."
            | _ -> Ok args in

        let inputChars args =
            let style =
                styles
                |> List.filter (fun s -> s.name = args.style)
                |> List.hd in

            let ensure_visible_char ch =
                if ch = space then "<SPACE>" else String.make 1 ch in

            let invalidChars =
                args.text
                |> String.to_seq
                |> List.of_seq
                |> List.filter (fun ch -> not <| List.mem ch style.supported_chars)
                |> List.map ensure_visible_char in

            let error style =
              let supported_chars style =
                let chars = style.supported_chars |> List.map ensure_visible_char in
                String.concat " " chars in

              String.concat
                "\n"
                [Printf.sprintf "Invalid characters found for style \"%s\": %s" (style.name) (String.concat " " invalidChars);
                 Printf.sprintf "Supported characters: %s" (supported_chars style)] in

            if List.length invalidChars = 0
            then Ok args
            else Error (error style) in

        let ( let* ) = Result.bind in
        let* args = argCount rawArgs in
        let* args' = styleName args in
        let* args'' = inputLength args' in
        let* args''' = inputChars args'' in
        Ok args'''
end

open ArgValidation
open Styles

let args = validate

let get_style args =
    styles
    |> List.filter (fun s -> s.name = args.style)
    |> List.hd

let convert_text args style =
  let text : string = args.text in
  let converter : char -> string = style.converter in
  text
  |> String.to_seq
  |> Seq.map converter
  |> List.of_seq
  |> fun lst -> String.concat "" lst

let () =
  match args with
  | Error e ->
      Printf.eprintf "%s\n" e
  | Ok a ->
      a
      |> get_style
      |> (convert_text a)
      |> fun s -> Printf.printf "%s\n" s
