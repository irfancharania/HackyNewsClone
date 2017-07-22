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
type FailedFetchItem = {
    errorMessage: string
    item: FeedItem
}

type FetchedItem = 
| Unfetched of FeedItem // result if url matches unparsable sites
| Fetched of FullContentItem 
| Failed of FailedFetchItem

type TryFetchFullContent = UnparsableSites -> FeedItem -> FetchedItem
