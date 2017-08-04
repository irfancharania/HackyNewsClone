module HackyNewsDomain.Data

open System
open System.Text.RegularExpressions
open FSharp.Data
open FSharp.Configuration
open HackyNewsDomain.Domain



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
                    |> Array.map (fun x -> { title= x.Title;
                                            description= x.Description;
                                            link = new Uri(x.Link);
                                            pubDate = x.PubDate;
                                            comments = new Uri(x.Comments)})
                    |> Array.toList;

        }

        Ok feed
    with
        | ex -> Error (ServiceError ("Unable to load Rss feed: " + ex.Message))

        
let tryFetchItemContent' = fun (item:FeedItem) ->
    try
        let args = List.singleton ("url", item.link.AbsoluteUri);
        let headers = Seq.singleton ("x-api-key", settings.Mercury.ApiKey)

        let responseBody = Http.RequestString(settings.Mercury.ApiUrl.AbsoluteUri
                                              , args, headers)
        
        let data = MercuryResponse.Parse responseBody


        if data.WordCount > 0
            then Ok {item = item; content = data.Content}
            else Error (item, "failed to parse content")
    with
        | ex -> Error (item, ex.Message)


let tryFetchItemContent:TryFetchItemContent = fun blacklist item ->
    let isBlackListed (item:FeedItem) = 
        blacklist 
        |> Seq.exists (fun x -> x.IsMatch(item.link.AbsoluteUri))

    if isBlackListed item then
        Error (item, "Site is blacklisted")
    else
        tryFetchItemContent' item

let tryFetchItems:TryFetchItems = fun blacklist feed -> 
    feed.items
    |> Seq.map (tryFetchItemContent blacklist)


let isFetchServiceAvailable:IsFetchServiceAvailable = fun feed ->
    try
        let item = feed.items |> Seq.head
        let args = List.singleton ("url", item.link.AbsoluteUri);
        let headers = Seq.singleton ("x-api-key", settings.Mercury.ApiKey)

        let responseBody = Http.Request(settings.Mercury.ApiUrl.AbsoluteUri
                                                , args, headers)
        Ok feed
    with
    | ex -> Error (ServiceError (ex.Message))

// Run it!
let getData (settings:Settings) =
    let blacklist = getBlacklist settings
    let url = settings.Feed.Url

    let fetchRssFeed:FetchRssFeedItems = fun blacklist uri ->
        uri
        |> getRssFeed 
        |> Result.bind isFetchServiceAvailable
        |> Result.map (tryFetchItems blacklist)
    
    fetchRssFeed blacklist url

