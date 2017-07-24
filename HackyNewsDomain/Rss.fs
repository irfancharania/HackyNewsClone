module HackyNewsDomain.Data

open System
open System.Text.RegularExpressions
open FSharp.Data
open FSharp.Configuration
open HackyNewsDomain


// Unparsable blacklist
// ----------------------
let blacklist: UnparsableSites = [ 
    new Regex(".*phone.*")
]


// Set up Type Providers
// ----------------------
type Settings = YamlConfig<"config.yml">
let settings = Settings()

type Rss = XmlProvider<"sample/rss.xml">
type MercuryResponse = JsonProvider<"sample/mercury.json">


// Main functions
// ----------------------
let getRssFeed:GetRssFeed = fun url ->
    let rss = Rss.Parse(url)

    let feed: RssFeed = {
        title = rss.Channel.Title;
        description = rss.Channel.Description;
        link = new Uri(rss.Channel.Link);
        item = rss.Channel.Items 
                |> Array.map (fun x -> { title= x.Title;
                                        description= x.Description;
                                        link = new Uri(x.Link);
                                        pubDate = x.PubDate;
                                        comments = new Uri(x.Comments)})
                |> Array.toList;

    }

    feed

