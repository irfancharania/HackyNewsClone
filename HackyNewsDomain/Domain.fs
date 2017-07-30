namespace HackyNewsDomain

open System
open System.Text.RegularExpressions
open FSharp.Data


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
    items: FeedItem seq
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

type FetchServiceUnavailable = {
    errorMessage: string
    feed: RssFeed
}

type TryFetchItems = 
    UnparsableSites                                     // dependency
        -> Result<RssFeed, FetchServiceUnavailable>     // input
        -> FetchedItem seq                              // output
