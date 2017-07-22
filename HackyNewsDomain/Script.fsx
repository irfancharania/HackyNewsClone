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
    let isUnfetchable = blacklist |> List.exists (fun x -> x.IsMatch(item.link.AbsoluteUri))

    match isUnfetchable with 
    | true -> FetchedItem.Unfetched item
    //| false -> 
    

//let fetchFeedItem:FetchFeedItem = fun item ->
    
type mercuryResponse = JsonProvider<"""
{
  "title": "An Ode to the Rosetta Spacecraft as It Flings Itself Into a Comet",
  "content": "<div><article class=\"content body-copy\"> <p>Today, the European Space Agency’s... ",
  "date_published": "2016-09-30T07:00:12.000Z",
  "lead_image_url": "https://www.wired.com/wp-content/uploads/2016/09/Rosetta_impact-1-1200x630.jpg",
  "dek": "Time to break out the tissues, space fans.",
  "url": "https://www.wired.com/2016/09/ode-rosetta-spacecraft-going-die-comet/",
  "domain": "www.wired.com",
  "excerpt": "Time to break out the tissues, space fans.",
  "word_count": 1031,
  "direction": "ltr",
  "total_pages": 1,
  "rendered_pages": 1,
  "next_page_url": null
}
""">



let getMercuryResponse (item:FeedItem) =
    let response = Http.RequestString("https://mercury.postlight.com/parser"
                        , [("url",item.link.AbsoluteUri)]
                        , seq {yield ("x-api-key", "------")})
    let data = mercuryResponse.Parse(response)

    if data.WordCount > 0 then
        Result.Ok {item= item; content = data.Content}
    else
        Result.Error {item = item; errorMessage = "failed to parse content"}
        

    