/// The application accepts an RSS feed and fetches
/// full content text for each item (unless item is blacklisted)
/// The content is cleansed using the Mercury Postlight web parser
namespace rec HackyNewsDomain.Domain

open System
open System.Text.RegularExpressions
open FSharp.Data


//------------------------
// Blacklist sites that match expression
type UnparsableSites = seq<Regex>

//------------------------
// Domain input

type FeedItem = {
    title: string
    description: string
    link: Uri
    pubDate: DateTime
    comments: Uri
}

type RssFeed = {
    title: string
    description: string
    link: Uri
    items: FeedItem seq
}

//------------------------
// Domain output

type FeedItemWithContent = {
    content: string
    item: FeedItem
}

//------------------------
// Public workflow

type FetchRssFeedItems = UnparsableSites                                        // dependency
                            -> Uri                                              // input
                            -> Result<seq<FetchedItemResult>, ServiceErrors>    // output


//------------------------

type GetRssFeed = Uri                                       // input
                    -> Result<RssFeed, ServiceErrors>       // output

type TryFetchItems = UnparsableSites                // dependency
                        -> RssFeed                  // input
                        -> seq<FetchedItemResult>   // output

type TryFetchItemContent = UnparsableSites          // dependency
                            -> FeedItem             // input
                            -> FetchedItemResult    // output

type FetchedItemResult = Result<FeedItemWithContent, FetchItemError>

//------------------------
// Expected errors

type FetchItemError = {item:FeedItem; message:string}

type ServiceErrors = 
| FailedToGetRssCase of string
| FetchServiceNotAvailableCase of string
