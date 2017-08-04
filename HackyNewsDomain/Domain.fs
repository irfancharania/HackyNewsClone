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

type IsFetchServiceAvailable = RssFeed -> Result<RssFeed, ServiceError>

type FetchRssFeedItems = UnparsableSites                                    // dependency
                            -> Uri                                          // input
                            -> Result<seq<FetchedItemResult>, ServiceError> // output

type GetRssFeed = Uri                                   // input
                    -> Result<RssFeed, ServiceError>    // output


type FetchedItemResult = Result<FeedItemWithContent, FetchItemError>

type TryFetchItems = UnparsableSites                // dependency
                        -> RssFeed                  // input
                        -> seq<FetchedItemResult>   // output

type TryFetchItemContent = UnparsableSites                  // dependency
                            -> FeedItem                     // input
                            -> FetchedItemResult            // output

//------------------------
// Expected errors

type ServiceError = ServiceError of string
type FetchItemError = FeedItem * string


