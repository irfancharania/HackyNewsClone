module HackyNewsDomain.Utils

type AsyncResult<'success,'failure> = Async<Result<'success,'failure>>


let asyncResultBind (fn: 'b -> Async<Result<'c, 'Error>>) (fn2: Async<Result<'b, 'Error>>) :Async<Result<'c, 'Error>> =
    async{
        let! x = fn2
        
        match x with
        | Ok(y) ->  let! z = fn(y)
                    return z
        | Error (y) -> return Error (y)
    }
