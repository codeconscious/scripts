(* Simple game logic modeling practice one Sunday morning. *)
type DieValue =
    | One = 1
    | Two = 2
    | Three = 3
    | Four = 4
    | Five = 5
    | Six = 6

let dieValues = [| 1..6 |]

type RolledDice = {
    Dice: int array
    Counts: Map<int, int>
    Sum: int
}

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

    let private isConsecutive (numbers: int array) =
        numbers
        |> Array.pairwise
        |> Array.forall (fun (x, y) -> x + 1 = y)

    let chance (rolledDice: RolledDice) =
        rolledDice.Sum

    let threeOfAKind (rolledDice: RolledDice) =
        rolledDice.Counts
        |> Map.filter (fun _ counts -> counts >= 3)
        |> fun x ->
            if x.Count = 1
            then rolledDice.Sum
            else int FixedScores.Zero

    let fourOfAKind (rolledDice: RolledDice) =
        rolledDice.Counts
        |> Map.filter (fun _ counts -> counts >= 4)
        |> fun x ->
            if x.Count = 1
            then rolledDice.Sum
            else int FixedScores.Zero

    let fullHouse (rolledDice: RolledDice) =
        rolledDice.Counts
        |> Map.filter (fun _ counts -> counts > 2 && counts < 5) // Five-of-a-kind is excluded.
        |> Map.values
        |> Seq.sum
        |> fun sum ->
            if sum = 5
            then FixedScores.FullHouse
            else FixedScores.Zero
            |> int

    let smallStraight (rolledDice: RolledDice) =
        let hasValueAppearingAtLeast3Times =
            rolledDice.Counts
            |> Map.values
            |> Seq.exists (fun i -> i >= 3)
        let score =
            if hasValueAppearingAtLeast3Times
            then FixedScores.Zero
            else
                let sortedValues = Array.sort rolledDice.Dice
                let group1 = sortedValues[..3]
                let group2 = sortedValues[1..]
                if isConsecutive group1 || isConsecutive group2
                then FixedScores.SmallStraight
                else FixedScores.Zero
        int score

    let bigStraight (rolledDice: RolledDice)=
        if rolledDice.Dice |> Array.sort |> isConsecutive
        then FixedScores.BigStraight
        else FixedScores.Zero
        |> int

    let fiveOfAKind (rolledDice: RolledDice)=
        rolledDice.Dice
        |> Array.tail
        |> Array.forall ((=) rolledDice.Dice[0])
        |> fun result ->
            if result
            then FixedScores.FiveOfAKind
            else FixedScores.Zero
        |> int

open Rules

let rolledDice =
    rollDice()
    |> fun x -> {
        Dice = x
        Counts = x |> Array.countBy id |> Map.ofArray
        Sum = Array.sum x
    }

printfn "%A" rolledDice

let scoreFor x =
    let x' = int x
    match Map.containsKey x' rolledDice.Counts with
    | true -> x' * rolledDice.Counts[x']
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
        ThreeOfAKind = threeOfAKind rolledDice
        FourOfAKind = fourOfAKind rolledDice
        FullHouse = fullHouse rolledDice
        SmallStraight = smallStraight rolledDice
        BigStraight = bigStraight rolledDice
        FiveOfAKind = fiveOfAKind rolledDice
    }

printfn "%A" results
