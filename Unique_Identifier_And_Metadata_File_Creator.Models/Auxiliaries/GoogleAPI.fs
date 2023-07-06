namespace Auxiliaries

open System
open System.Data
open System.Net.NetworkInformation

open GoogleSheets
open CheckForNetConn
open ROP_Functions

module GoogleAPI =

    //****************** auxiliary function definitions **********************

    let private checkForNetConn() = 

        Seq.initInfinite (fun _ -> checkForNetConn()) //F#    
        |> Seq.takeWhile ((=) None) 
        |> Seq.iter      (fun _ -> ())  

    //****************** main function definitions **********************

    let readingFromGoogleSheets jsonFileName columnStart rowStart columnEnd rowEnd firstRowIsHeaders id sheetName6 =
   
        do checkForNetConn()     

        ReadingFromGoogleSheets.ReadFromGoogleSheets(
            jsonFileName, 
            id, 
            sheetName6,
            columnStart, rowStart, columnEnd, rowEnd, firstRowIsHeaders
        ) |> Option.ofObj //DLL C# 

    let writingToGoogleSheets dtGoogle jsonFileName1 id sheetName6 endIndex =
       
        do checkForNetConn()     

        let writeToGoogleSheets = new WritingToGoogleSheets(dtGoogle)
        do writeToGoogleSheets.WriteToGoogleSheets(jsonFileName1, id, sheetName6, endIndex) 
    
    