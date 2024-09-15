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

let number = Random().Next(1_100_000) // Only up to 1 million (exclusive) is supported.

let result = fetchApiData number |> Async.RunSynchronously
match result with
| Ok data -> printfn $"{number} = {data.IsEven}  |  {data.Ad}"
| Error e -> printfn $"Error: {e}"
0
