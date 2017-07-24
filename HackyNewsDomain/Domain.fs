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

type UnparsableSites = Regex list

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

type CanFetchContent = 
    UnparsableSites     // dependency
        -> FeedItem     // input
        -> bool         // output

type TryFetchItem = FeedItem -> FetchedItemResult

type TryFetchItems = 
    UnparsableSites             // dependency
        -> RssFeed              // input
        -> FetchedItem list     // output

type GetRssFeedWorkFlow =  Uri -> FetchedItem list
