open System.IO

// Making F# faster and more interopable with task {…}

let readFilesTask (path1, path2) =
   task {
        let! bytes1 = File.ReadAllBytesAsync(path1)
        let! bytes2 = File.ReadAllBytesAsync(path2)
        return Array.append bytes1 bytes2
   }

// Making F# faster: Struct representations for partial active patterns

[<return: Struct>]
let (|Int|_|) (str: string) =
   match System.Int32.TryParse(str) with
   | true, int -> ValueSome(int)
   | _ -> ValueNone

match "455" with
| Int a -> "Is an int!"
| _ -> "Not an int..."

// Making F# more uniform: Formatting for binary numbers

printfn "%B" 444444
