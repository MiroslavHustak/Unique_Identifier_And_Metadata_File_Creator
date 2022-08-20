module PatternBuilders

let (>>=) condition nextFunc =
    match fst condition with
    | false -> snd condition
    | true  -> nextFunc()  
    
[<Struct>]
type MyPatternBuilder = MyPatternBuilder with    
    member _.Bind(condition, nextFunc) = (>>=) <| condition <| nextFunc
    member _.Return x = x