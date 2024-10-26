let numbers1 = [|4; 3; 1; 6|]
let numbers2 = [|4; 3; 8; 5|]

let isArraySpecial (nums: int array) =
    nums
    |> Array.pairwise
    |> Array.forall (fun (x, y) -> (x % 2) <> (y % 2))

printfn "{%A} is '{%A}'" numbers1 (isArraySpecial numbers1)
printfn "{%A} is '{%A}'" numbers2 (isArraySpecial numbers2)
