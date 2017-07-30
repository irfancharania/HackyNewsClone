module HackyNewsDomain.Models
open HackyNewsDomain

type Item(item:FetchedItem) =  
    member this.GetItem():FeedItem =
        match item with
        | Unfetched(item) -> item
        | Fetched(result) -> match result with
                             | Ok(r) -> r.item 
                             | Error(r) -> r.item
    member this.HasContent():bool =
        match item with
        | Unfetched(item) -> false
        | Fetched(result) -> match result with
                             | Ok(r) -> true
                             | Error(r) -> false
    member this.GetContent():string =
        match item with
        | Unfetched(item) -> ""
        | Fetched(result) -> match result with
                             | Ok(r) -> r.content
                             | Error(r) -> "" 


type Feed(settings:Data.Settings) =
    member this.GetItems():Item seq =
        Data.getData settings
        |> Seq.map (fun x -> new Item(x))