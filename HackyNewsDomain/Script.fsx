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

type Rss = XmlProvider<"https://news.ycombinator.com/rss">


let blacklist: UnparsableSites = [ new Regex(".*twilio.*")]

let getRssFeed = 
    let test = Rss.GetSample()
    //for item in test.Channel.Items do
    //  printfn " - %s (%s)" item.Title item.Link

    let feed : RssFeed = {
        title = test.Channel.Title;
        description = test.Channel.Description;
        link = new Uri(test.Channel.Link);
        item = test.Channel.Items 
                |> Array.map (fun x -> { title= x.Title;
                                        description= x.Description;
                                        link = new Uri(x.Link);
                                        pubDate = x.PubDate;
                                        comments = new Uri(x.Comments)})
                |> Array.toList;

    }

    feed


let (filterFeedItem:TryFetchFullContent) = fun blacklist item ->
    

    FetchedItem.Unfetched item