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

let fromDomain (fetchedItem:FetchedItemResult) = 
    match fetchedItem with 
    | Ok(x) -> {    title = x.item.title;
                    description = x.item.description;
                    link = x.item.link;
                    pubDate = x.item.pubDate;
                    comments = x.item.comments;
                    content = x.content;
                    error = "";
                }
    | Error(x) -> { title = x.item.title;
                    description = x.item.description;
                    link = x.item.link;
                    pubDate = x.item.pubDate;
                    comments = x.item.comments;
                    content = "";
                    error = x.message;
                }

