open System
open System.Net.Http
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks

type ApiResponse = {
    [<JsonPropertyName("ad")>] Ad: string
    [<JsonPropertyName("iseven")>] IsEven: bool
}

let fetchApiData (url: string) : Async<Result<ApiResponse,string>> =
    async {
        use client = new HttpClient()
        try
            let! response = client.GetStringAsync(url) |> Async.AwaitTask
            let options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
            let data = JsonSerializer.Deserialize<ApiResponse>(response, options)
            return Ok data
        with
        | :? HttpRequestException as ex ->
            return Error $"Request error: {ex.Message}"
        | ex ->
            return Error $"An error occurred: {ex.Message}"
    } // |> Async.StartAsTask

let number = Random().Next(1_000_000)
// let number = 1000000
let url = $"https://api.isevenapi.xyz/api/iseven/{number}/"

let result = fetchApiData url |> Async.RunSynchronously
match result with
| Ok data -> printfn $"{number} = {data.IsEven}  |  {data.Ad}"
| Error e -> printfn $"Error: {e}"
0
