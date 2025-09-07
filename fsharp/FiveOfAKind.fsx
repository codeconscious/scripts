(* Simple game logic modeling practice one Sunday morning. *)
type DieValue =
    | One = 1
    | Two = 2
    | Three = 3
    | Four = 4
    | Five = 5
    | Six = 6

let dieValues = [| 1..6 |]

type RolledDice = int array

let rnd = System.Random()

let rollDice () =
    Array.init 5 (fun _ -> dieValues[rnd.Next dieValues.Length])

// type Roll private (array: int[]) =
//     static member Create(a: int, b: int, c: int, d: int, e: int) =
//         let values = [| a; b; c; d; e |]
//         if values.Length = 5 then
//             Roll values
//         else
//             failwith "Must have exactly 5 integers"

type RollScore =
    {
        Ones: int
        Twos: int
        Threes: int
        Fours: int
        Fives: int
        Sixes: int
        Chance: int
        ThreeOfAKind: int
        FourOfAKind: int
        FullHouse: int
        SmallStraight: int
        BigStraight: int
        FiveOfAKind: int
    }

module Rules =
    type FixedScores =
        | Zero = 0
        | FullHouse = 25
        | SmallStraight = 30
        | BigStraight = 40
        | FiveOfAKind = 50

    let countNumbers (values: int array) =
        values |> Array.countBy id |> Map.ofArray

    let chance (xs: int array) = xs |> Array.sum

    let threeOfAKind (rolledDice: int array) (counts: Map<int, int>) =
        counts
        |> Map.filter (fun _ y -> y >= 3)
        |> fun x ->
            if x.Count = 1
            then rolledDice |> Array.sum
            else int FixedScores.Zero

    let fourOfAKind (rolledDice: int array) (counts: Map<int, int>) =
        counts
        |> Map.filter (fun _ y -> y >= 4)
        |> fun x ->
            if x.Count = 1
            then rolledDice |> Array.sum
            else int FixedScores.Zero

    let fullHouse (map: Map<int, int>) =
        map
        |> Map.filter (fun _ count -> count > 2)
        |> Map.values
        |> Seq.sum
        |> fun sum ->
            if sum = 5
            then FixedScores.FullHouse
            else FixedScores.Zero
            |> int

    let private isConsecutive (is: int array) =
        is
        |> Array.pairwise
        |> Array.forall (fun (a, b) -> b = a + 1)

    let smallStraight (xs: Map<int, int>) =
        let anyValueAppearsAtLeast3Times = xs |> Map.values |> Seq.exists (fun i -> i >= 3)
        let score =
            if anyValueAppearsAtLeast3Times
            then FixedScores.Zero
            else
                let sortedValues = xs |> Map.keys |> Array.ofSeq |> Array.sort
                let group1 = sortedValues[..3]
                let group2 = sortedValues[1..]
                if isConsecutive group1 || isConsecutive group2
                then FixedScores.SmallStraight
                else FixedScores.Zero
        int score

    let bigStraight (xs: int array) =
        if xs |> Array.sort |> isConsecutive
        then FixedScores.BigStraight
        else FixedScores.Zero
        |> int

    let fiveOfAKind (xs: int array) =
        xs
        |> Array.tail
        |> Array.forall ((=) xs[0])
        |> fun result ->
            if result = true
            then FixedScores.FiveOfAKind
            else FixedScores.Zero
        |> int

open Rules

let rolledDice = [| 6;6;6;6;5 |] // rollDice()
printfn "%A" rolledDice

let countsMap = countNumbers rolledDice

let scoreFor x =
    let x' = int x
    match Map.containsKey x' countsMap with
    | true -> x' * countsMap[x']
    | false -> int FixedScores.Zero

let results =
    {
        Ones = scoreFor DieValue.One
        Twos = scoreFor DieValue.Two
        Threes = scoreFor DieValue.Three
        Fours = scoreFor DieValue.Four
        Fives = scoreFor DieValue.Five
        Sixes = scoreFor DieValue.Six
        Chance = chance rolledDice
        ThreeOfAKind = threeOfAKind rolledDice countsMap
        FourOfAKind = fourOfAKind rolledDice countsMap
        FullHouse = fullHouse countsMap
        SmallStraight = smallStraight countsMap
        BigStraight = bigStraight rolledDice
        FiveOfAKind = fiveOfAKind rolledDice
    }

printfn "%A" results
