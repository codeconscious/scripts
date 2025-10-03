// Source: https://www.youtube.com/watch?v=24yLOYMPKEM
let doSomething a b =
    let plusOne = a + 1
    let result = plusOne * b
    result

let doSomething2 a b =
    (fun plusOne ->
    (fun result ->
        result
    )(plusOne * b))(a + 1)

let ``let`` value body = body value
let doSomething3 a b =
    ``let`` (a + 1) (fun plusOne ->
    ``let`` (plusOne * b)) (fun result ->
    result)

let doSomething3' a b =
    ``let`` (a + 1) <| fun plusOne ->
    ``let`` (plusOne * b) <| fun result ->
    result

let doSomething3'' a b =
    a + 1 |> fun plusOne ->
    plusOne * b |> fun result ->
    result

// Equivalent:
let a = 100
a

100 |> fun a -> a
100 |> id
