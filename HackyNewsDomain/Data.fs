module HackyNewsDomain.Data

open System
open System.Text.RegularExpressions
open FSharp.Data
open FSharp.Configuration
open HackyNewsDomain.Domain
open HackyNewsDomain.Dto



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
        | ex -> Error (FailedToGetRssCase("Unable to load Rss feed: " + ex.Message))

        
let tryFetchItemContent' = fun (service:FetchServiceInfo) (item:FeedItem) ->
    try
        let args = List.singleton ("url", item.link.AbsoluteUri);
        let headers = Seq.singleton ("x-api-key", service.apiKey)

        let responseBody = Http.RequestString(service.apiUrl.AbsoluteUri
                                              , args, headers)
        
        let data = MercuryResponse.Parse responseBody


        if data.WordCount > 0
            then Ok {item = item; content = data.Content}
            else Error ({item = item; message = "failed to parse content"})
    with
        | ex -> Error ({item = item; message = ex.Message})


let tryFetchItemContent:TryFetchItemContent = fun blacklist service item ->
    let isBlackListed (item:FeedItem) = 
        blacklist 
        |> Seq.exists (fun x -> x.IsMatch(item.link.AbsoluteUri))

    if isBlackListed item then
        Error ({item = item; message = "Site is blacklisted"})
    else
        tryFetchItemContent' service item


let tryFetchItems:TryFetchItems = fun blacklist service feed -> 
    feed.items
    |> Seq.map (tryFetchItemContent blacklist service)


let isFetchServiceAvailable:IsFetchServiceAvailable<RssFeed> = fun (service:FetchServiceInfo) item ->
    try
        let args = List.singleton ("url", service.testUrl.AbsoluteUri);
        let headers = Seq.singleton ("x-api-key", service.apiKey)

        let responseBody = Http.Request(service.apiUrl.AbsoluteUri
                                                , args, headers)
        Ok (item)
    with
    | ex -> Error (FetchServiceNotAvailableCase (ex.Message))


// Run it!
let getData (settings:Settings) =
    let blacklist = getBlacklist settings
    let fetchService = getFetchServiceInfo settings
    let url = settings.Feed.Url
    

    let fetchRssFeed:FetchRssFeedItems = fun blacklist service uri ->
        let isFetchServiceAvailable' = isFetchServiceAvailable service
        let tryFetchItems' = tryFetchItems blacklist service

        uri
        |> getRssFeed 
        |> Result.bind isFetchServiceAvailable'
        |> Result.map tryFetchItems'
    
    fetchRssFeed blacklist fetchService url
    |> Result.map (Seq.map fromDomain)

