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
    item: FeedItem list
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


type GetRssFeed = string -> RssFeed
type CanFetchContent = FeedItem -> bool
type TryFetchContent = UnparsableSites -> RssFeed -> FetchedItem list


type GetRssFeedWorkFlow = GetRssFeed -> FetchedItem list