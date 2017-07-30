module HackyNewsDomain.Data

open System
open System.Text.RegularExpressions
open FSharp.Data
open FSharp.Configuration
open HackyNewsDomain



// Set up Type Providers
// ----------------------
type Settings = YamlConfig<"config.yml">
let settings = Settings()

type Rss = XmlProvider<"sample/rss.xml">
type MercuryResponse = JsonProvider<"sample/mercury.json">



// Unparsable blacklist
// ----------------------
let blacklist (settings:Settings) :UnparsableSites = 
    settings.Blacklist
    |> Seq.map (fun x -> new Regex(x))


// Main functions
// ----------------------
let getRssFeed:GetRssFeed = fun url ->
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

    feed
   
let tryFetchContent:TryFetchItem = fun item ->
    try
        let args = List.singleton ("url", item.link.AbsoluteUri);
        let headers = Seq.singleton ("x-api-key", settings.Mercury.ApiKey)

        let responseBody = Http.RequestString(settings.Mercury.ApiUrl.AbsoluteUri
                                              , args, headers)
        
        let data = MercuryResponse.Parse responseBody


        if data.WordCount > 0
            then Ok {item = item; content = data.Content}
            else Error {item = item; errorMessage = "failed to parse content"}
    with
        | ex -> Error {item = item; errorMessage = ex.Message}


let maybeFetchItem:MaybeFetchItem = fun blacklist item -> 
    let isBlackListed = 
        blacklist 
        |> Seq.exists (fun x -> x.IsMatch(item.link.AbsoluteUri))

    if isBlackListed then
        Unfetched item
    else
        Fetched (tryFetchContent item)


let tryFetchItems:TryFetchItems = fun blacklist feed -> 
    match feed with
    | Ok x -> x.items |> Seq.map (maybeFetchItem blacklist)
    | Error x -> x.feed.items |> Seq.map Unfetched
    


let fetchServiceAvailable:FetchServiceAvailable = fun feed ->
    try
        let item = feed.items |> Seq.head
        let args = List.singleton ("url", item.link.AbsoluteUri);
        let headers = Seq.singleton ("x-api-key", settings.Mercury.ApiKey)

        let responseBody = Http.Request(settings.Mercury.ApiUrl.AbsoluteUri
                                                , args, headers)
        Ok (feed)
    with
        | ex -> Error ({feed = feed; errorMessage = ex.Message})


// Run it!
let getData (settings:Settings) =
    settings.Feed.Url 
    |> getRssFeed
    |> fetchServiceAvailable
    |> tryFetchItems (blacklist settings)

