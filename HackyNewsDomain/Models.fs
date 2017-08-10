module HackyNewsDomain.Models
open HackyNewsDomain.Domain
open HackyNewsDomain.Dto


type Feed(settings:Data.Settings) =
    member this.GetItems() =
        Data.getData settings
        |> Dto.fromDomain
