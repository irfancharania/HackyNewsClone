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
    let canFetchContent = 
        blacklist 
        |> Seq.exists (fun x -> x.IsMatch(item.link.AbsoluteUri))
        |> not

    if canFetchContent then
        Fetched (tryFetchContent item)
    else
        Unfetched item


let tryFetchItems:TryFetchItems = fun blacklist feed -> 
    feed.items
    |> Seq.map (maybeFetchItem blacklist)


// Run it!
let getData (settings:Settings) =
    settings.Feed.Url 
    |> getRssFeed
    |> tryFetchItems (blacklist settings)

