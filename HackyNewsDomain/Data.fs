module HackyNewsDomain.Data

open System
open System.Text.RegularExpressions
open FSharp.Data
open FSharp.Configuration
open HackyNewsDomain.Domain
open HackyNewsDomain.Utils
open HackyNewsDomain.Logging

open Microsoft.Extensions.Logging
// Set up Type Providers
// ----------------------
type Settings = YamlConfig<"config.yml">
let settings = Settings()

type Rss = XmlProvider<"sample/rss.xml">
type MercuryResponse = JsonProvider<"sample/mercury.json">



// Unparsable blacklist
// ----------------------
let getBlacklist (settings:Settings) :UnparsableSites = 
    settings.Blacklist
    |> Seq.map (fun x -> new Regex(x))


// Fetch service info
// ----------------------
let getFetchServiceInfo (settings:Settings) :FetchServiceInfo =
    {
        apiUrl = settings.Mercury.ApiUrl;
        apiKey = settings.Mercury.ApiKey;
        testUrl = settings.Mercury.TestUrl
    }


// Log errors
// ----------------------

let logFetchItemError x =
    x


let logServiceError x =
    x


// Main functions
// ----------------------
let getRssFeed:GetRssFeed = fun url ->
    try
        let rss = Rss.Load(url.AbsoluteUri)

        let feed: RssFeed = {
            title = rss.Channel.Title;
            description = rss.Channel.Description;
            link = new Uri(rss.Channel.Link);
            items = rss.Channel.Items 
                    |> Array.map (fun x -> { title = x.Title;
                                            description = x.Description;
                                            link = new Uri(x.Link);
                                            pubDate = x.PubDate;
                                            comments = new Uri(x.Comments)})
                    |> Array.toList;

        }

        Ok feed
    with
        | ex -> Result.Error (FetchRssErrorCase ("Unable to load Rss feed: " + ex.Message))

        
let tryFetchItemContent' = fun (service:FetchServiceInfo) (item:FeedItem) ->
    async {
        try
            let args = List.singleton ("url", item.link.AbsoluteUri);
            let headers = Seq.singleton ("x-api-key", service.apiKey)

            let! responseBody = Http.AsyncRequestString(service.apiUrl.AbsoluteUri
                                                  , args, headers)
        
            let data = MercuryResponse.Parse responseBody


            if data.WordCount > 0
                then return Ok {item = item; content = data.Content}
                else return Result.Error ({item = item; message = "failed to parse content"})
        with
            | ex -> return Result.Error ({item = item; message = ex.Message})
    }

let tryFetchItemContent:TryFetchItemContent = fun blacklist service item ->
    let isBlackListed (item:FeedItem) = 
        blacklist 
        |> Seq.exists (fun x -> x.IsMatch(item.link.AbsoluteUri))

    if isBlackListed item then
        async { return Result.Error ({item = item; message = "Site is blacklisted"}) }
    else
        tryFetchItemContent' service item


let tryFetchItems:TryFetchItems = fun blacklist service feed -> 
    let fetch items = 
        tryFetchItemContent blacklist service items

    feed.items
    |> Seq.map fetch
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Array.toSeq


let isFetchServiceAvailable:IsFetchServiceAvailable<RssFeed> = fun (service:FetchServiceInfo) feed ->
    try
        let args = List.singleton ("url", service.testUrl.AbsoluteUri);
        let headers = Seq.singleton ("x-api-key", service.apiKey)

        let responseBody = Http.Request(service.apiUrl.AbsoluteUri
                                                , args, headers)
        Ok (feed)
    with
    | ex -> Result.Error (FetchServiceNotAvailableErrorCase {message = ex.Message; items = feed.items})


// Run it!
let getData (settings:Settings) (logger:ILogger)=
    let blacklist = getBlacklist settings
    let fetchService = getFetchServiceInfo settings
    let url = settings.Feed.Url

    logWithArgs logger Info "Commence feed fetching at {StartTime} ({RandomNumber})" [|DateTimeOffset.Now; 42|]

    let fetchRssFeed:FetchRssFeedItems = fun blacklist service uri ->
        let isFetchServiceAvailable' = isFetchServiceAvailable service
        let tryFetchItems' = tryFetchItems blacklist service

        uri
        |> getRssFeed 
        |> Result.bind isFetchServiceAvailable'
        |> Result.map tryFetchItems'
        |> Result.mapError logServiceError

    fetchRssFeed blacklist fetchService url
