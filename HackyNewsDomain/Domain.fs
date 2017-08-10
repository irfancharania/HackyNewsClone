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

// Service to fetch sanitized text
type FetchServiceInfo = {
    apiUrl: Uri
    apiKey: string
    testUrl: Uri
}

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
                            -> FetchServiceInfo                                 // dependency
                            -> Uri                                              // input
                            -> Result<seq<FetchedItemResult>, ServiceError>    // output


//------------------------

type GetRssFeed = Uri                                       // input
                    -> Result<RssFeed, ServiceError>       // output

type IsFetchServiceAvailable<'a> = FetchServiceInfo                         // input
                                    -> 'a                                   // passthrough
                                    -> Result<'a, ServiceError>            // output

type TryFetchItems = UnparsableSites                // dependency
                        -> FetchServiceInfo         // dependency
                        -> RssFeed                  // input
                        -> seq<FetchedItemResult>   // output

type TryFetchItemContent = UnparsableSites          // dependency
                            -> FetchServiceInfo     // dependency                        
                            -> FeedItem             // input
                            -> FetchedItemResult    // output

type FetchedItemResult = Result<FeedItemWithContent, FetchItemError>

//------------------------
// Expected errors

type FetchItemError = {item:FeedItem; message:string}

type ServiceError = 
| FailedToGetRssCase of string
| FetchServiceNotAvailableCase of string

