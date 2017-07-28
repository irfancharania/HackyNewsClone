namespace HackyNewsDomain

open System
open System.Text.RegularExpressions


type FeedItem = {
    title: string
    description: string
    link: Uri
    pubDate: DateTime
    comments: Uri
}

type FullContentItem = {
    content: string
    item: FeedItem
}

type RssFeed = {
    title: string
    description: string
    link: Uri
    items: FeedItem list
}

type UnparsableSites = seq<Regex>

//------------------------
type FailedFetchItem = {
    errorMessage: string
    item: FeedItem
}

type FetchedItemResult = Result<FullContentItem, FailedFetchItem>

type FetchedItem = 
| Unfetched of FeedItem 
| Fetched of FetchedItemResult


type GetRssFeed = Uri -> RssFeed

type TryFetchItem = FeedItem -> FetchedItemResult

type MaybeFetchItem = UnparsableSites     // dependency
                        -> FeedItem       // input
                        -> FetchedItem    // output

type TryFetchItems = 
    UnparsableSites             // dependency
        -> RssFeed              // input
        -> FetchedItem list     // output
