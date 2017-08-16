module HackyNewsDomain.Models
open HackyNewsDomain.Domain
open HackyNewsDomain.Dto
open HackyNewsDomain
open Microsoft.Extensions.Logging


type Feed(settings:Data.Settings, logger : ILogger) =
    member this.GetItems() =
        Data.getData settings logger
        |> Dto.fromDomain