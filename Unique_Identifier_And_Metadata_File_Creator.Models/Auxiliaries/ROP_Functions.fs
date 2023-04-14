namespace Auxiliaries

open System
open System.IO

open Types

open Errors
open DiscriminatedUnions

module ROP_Functions =

    let tryWith f1 f2 x = //x se ne vzdy pouziva, ale z duvodu jednotnosti ponechano 
        try
            try          
               f1 x |> Success
            finally
               f2 x
        with
        | ex -> Failure ex.Message  

    let deconstructor1 =  
        function
        | Success x  -> x, String.Empty                                                   
        | Failure ex -> Array.empty, ex 

    let deconstructor2 =  
        function
        | Success x  -> x                                                   
        | Failure ex -> true

    let deconstructor3 title message y =  
        function
        | Success x  -> x                                                   
        | Failure ex -> error6 <| title <| message ex
                        y

    let deconstructor4 y =  
        function
        | Success x  -> x                                                   
        | Failure ex -> error4 ex
                        y
    
    let optionToArraySort str1 str2 x = 
        function
        | Some value -> Array.sort (Array.ofSeq (value)) 
        | None       -> error3 str1 str2      

    let optionToGenerics str1 str2 x = 
        match x with 
        | Some value -> value 
        | None       -> error3 str1 str2 |> Array.head

    let optionToDirectoryInfo str (x: DirectoryInfo option) = 
        match x with 
        | Some value -> value
        | None       -> 
                        error4 str //ukonci program
                        new DirectoryInfo(String.Empty) //whatever of DirectoryInfo type

    let optionToGenerics2 str x = 
        function
        | Some value -> value
        | None       -> 
                        error4 str                                   
                        x //whatever of the particular type   
                                      
                    



