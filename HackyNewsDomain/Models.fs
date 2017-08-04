module HackyNewsDomain.Models
open HackyNewsDomain.Domain


type ResultOrError<'TResult, 'TErr> (result:Result<'TResult, 'TErr>) =
    member this.Success():bool =
        match result with
        | Ok(x) -> true
        | Error(x) -> false

    member this.GetResult():'TResult =
        match result with
        | Ok(x) -> x
        | Error(x) -> failwith "Called GetResult on error result, dumbass"

    member this.GetError():'TErr = 
        match result with
        | Ok(x) -> failwith "Called GetError on success result"
        | Error(x) -> x


type Item(item:FetchedItem) =  
    member this.GetItem():ResultOrError<FullContentItem, FailedFetchItem> =
        match item with
        | Unfetched(item) -> Error ({item = item; errorMessage = "Not Fetched"})
        | Fetched(result) -> result
        |> ResultOrError


type Feed(settings:Data.Settings) =
    member this.GetItems() =
        Data.getData settings
        |> Result.map (Seq.map (fun x -> new Item(x)))
        |> ResultOrError
