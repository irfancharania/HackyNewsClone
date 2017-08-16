module HackyNewsDomain.Logging

open Microsoft.Extensions.Logging

type Level = 
    | Debug
    | Info
    | Warn
    | Error

let logUsing (logger : ILogger) (level : Level) =
    match level with
    | Debug -> logger.LogDebug
    | Info -> logger.LogInformation
    | Warn -> logger.LogWarning
    | Error -> logger.LogError

let logWithArgs (logger : ILogger) (level : Level) (message : string) (args : obj array) =
    match level with
    | Debug -> logger.LogDebug(message=message, args=args)
    | Info -> logger.LogInformation(message=message, args=args)
    | Warn -> logger.LogWarning(message=message, args=args)
    | Error -> logger.LogError(message=message, args=args)

