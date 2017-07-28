// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

#load "Domain.fs"
open HackyNewsDomain

// Define your library scripting code here
#r "../packages/FSharp.Data.2.3.3/lib/net40/FSharp.Data.dll"
#r "System.Xml.Linq.dll"
open FSharp.Data
open System;
open System.Text.RegularExpressions;
#r "../packages/FSharp.Configuration.1.3.0/lib/net45/FSharp.Configuration.dll"
#r "System.Configuration.dll"
open FSharp.Configuration

type Settings = YamlConfig<"config.yml">
let settings = Settings()

type Rss = XmlProvider<"sample/rss.xml">


//let blacklist: UnparsableSites = [ new Regex(".*twilio.*")]

let getRssFeed:GetRssFeed = fun url ->
    let test = Rss.Parse(url.AbsoluteUri)
    //for item in test.Channel.Items do
    //  printfn " - %s (%s)" item.Title item.Link

    let feed : RssFeed = {
        title = test.Channel.Title;
        description = test.Channel.Description;
        link = new Uri(test.Channel.Link);
        items = test.Channel.Items 
                |> Array.map (fun x -> { title= x.Title;
                                        description= x.Description;
                                        link = new Uri(x.Link);
                                        pubDate = x.PubDate;
                                        comments = new Uri(x.Comments)})
                |> Array.toList;

    }

    feed


//let (filterFeedItem:TryFetchFullContent) = fun blacklist item ->
//    let isUnfetchable = blacklist |> List.exists (fun x -> x.IsMatch(item.link.AbsoluteUri))

//    match isUnfetchable with 
//    | true -> FetchedItem.Unfetched item
//    //| false -> 
    

//let fetchFeedItem:FetchFeedItem = fun item ->



type mercuryResponse = JsonProvider<"sample/mercury.json">



let getMercuryResponse (item:FeedItem) =
    let response = Http.RequestString(settings.Mercury.ApiUrl.AbsoluteUri
                        , [("url",item.link.AbsoluteUri)]
                        , seq {yield ("x-api-key", settings.Mercury.ApiKey)})
    let data = mercuryResponse.Parse(response)

    if data.WordCount > 0 then
        Result.Ok {item= item; content = data.Content}
    else
        Result.Error {item = item; errorMessage = "failed to parse content"}
        

