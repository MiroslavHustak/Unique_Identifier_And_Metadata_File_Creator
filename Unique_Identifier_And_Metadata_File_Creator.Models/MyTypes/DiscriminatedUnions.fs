module DiscriminatedUnions

type Result<'TSuccess,'TFailure> =
   | Success of 'TSuccess
   | Failure of 'TFailure

[<Struct>]
type TaskResults =    
   | MyString of outputString: string
   | MyBool of outputBool: bool
   | MyUnit of outputUnit: unit