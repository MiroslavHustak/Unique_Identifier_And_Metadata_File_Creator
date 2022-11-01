module GoogleAPI

open GoogleSheets
open CheckingNetConn

//****************** auxiliary function definitions **********************

let private checkForNetConn() = 

    //Seq.initInfinite (fun _ -> NetConn.CheckForNetConn()) //DLL C# //neni tam kontrola na null
    Seq.initInfinite (fun _ -> Helpers.NetConn.checkForNetConn()) //F#    
    |> Seq.takeWhile ((=) false) 
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

    let writeToGoogleSheets = new WritingToGoogleSheets(dtGoogle);
    do writeToGoogleSheets.WriteToGoogleSheets(jsonFileName1, id, sheetName6, endIndex) 
    
    
       
  