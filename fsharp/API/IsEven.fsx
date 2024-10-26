open System
open System.Net
open System.Net.Http
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks

type ApiResponse = {
    [<JsonPropertyName("ad")>] Ad: string
    [<JsonPropertyName("iseven")>] IsEven: bool
}

let fetchApiData (number : int) : Async<Result<ApiResponse,string>> =
    let url = $"https://api.isevenapi.xyz/api/iseven/{number}/"

    async {
        use client = new HttpClient()
        try
            let! response = client.GetAsync(url) |> Async.AwaitTask
            if response.StatusCode = HttpStatusCode.Unauthorized
            then
                return Error $"Unauthorized: {number} is out of range."
            else
                response.EnsureSuccessStatusCode () |> ignore
                let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                let options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
                let data = JsonSerializer.Deserialize<ApiResponse>(content, options)
                return Ok data
        with
        | :? HttpRequestException as ex ->
            return Error $"Request error: {ex.Message}"
        | ex ->
            return Error ex.Message
    }

// Only up to 1 million (exclusive) is supported.
// We go over slightly to experience errors too.
let number = Random().Next(1_100_000)

match fetchApiData number |> Async.RunSynchronously with
| Ok data ->
    let isEven = if data.IsEven then "IS" else "is NOT"
    printfn $"{number} {isEven} even.{Environment.NewLine}[Ad] {data.Ad}"
| Error e ->
    printfn $"Error: {e}"

0
