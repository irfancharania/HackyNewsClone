module HackyNewsDomain.Dto

open System
open HackyNewsDomain.Domain


type FeedItemWithContentDto = {
    title: string
    description: string
    link: Uri
    pubDate: DateTime
    comments: Uri
    content: string
    error: string
}

type ResultDto = {
    items: seq<FeedItemWithContentDto>
    error: string
}

let fromDomainItem' (feedItem:FeedItem)  :FeedItemWithContentDto =
    { title = feedItem.title;
        description = feedItem.description;
        link = feedItem.link;
        pubDate = feedItem.pubDate;
        comments = feedItem.comments;
        content = "";
        error = "";
    }

let fromDomainItem (fetchedItem:FetchedItemResult) :FeedItemWithContentDto= 
    match fetchedItem with 
    | Ok(x) -> {    title = x.item.title;
                    description = x.item.description;
                    link = x.item.link;
                    pubDate = x.item.pubDate;
                    comments = x.item.comments;
                    content = x.content;
                    error = "";
                }
    | Error(x) -> {fromDomainItem' x.item with error = x.message }
    

let fromDomain (result: Result<seq<FetchedItemResult>, ServiceError> ) :ResultDto =
    match result with 
    | Ok (x) -> let seq = x |> Seq.map fromDomainItem
                {items = seq; error = ""}
    | Error(x) -> match x with 
                    | FetchRssErrorCase (y) -> {items = Seq.empty; error = y}
                    | FetchServiceNotAvailableErrorCase (y) -> 
                            let items' = y.items |> Seq.map fromDomainItem'
                            {items = items'; error = y.message}
                

    
