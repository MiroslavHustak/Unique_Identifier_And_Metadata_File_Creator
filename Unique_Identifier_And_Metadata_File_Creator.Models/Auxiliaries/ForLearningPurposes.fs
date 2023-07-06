namespace Auxiliaries

open System
open System.Data
open System.Net.NetworkInformation

open GoogleSheets

module ForLearningPurposes =  

    //****************** For learning purposes *****************************************
    
        //Now let's consider a hypothetical case where the Seq loop is not nedded 
    
        let checkForNetConn(): Result<_, string> = //Ok value not needed
            
            try
                try
                    use myPing = new Ping()
                    let host = "google.com"
                    let buffer = Array.zeroCreate 32
                    let timeout = 1000
                    let pingOptions = PingOptions()

                    myPing.Send(host, timeout, buffer, pingOptions)
                    |> Option.ofObj
                    |> Option.bind (fun pingReply ->
                                                    match pingReply.Status = IPStatus.Success with
                                                    | true  -> Some (pingReply |> ignore) //pingReply not needed
                                                    | false -> None 
                                    )
                    |> function
                        | Some () -> Ok () 
                        | None    -> Error "12029 Cannot Connect"
                finally
                ()
            with
            | ex -> Error (string ex)
           
        let readingFromGoogleSheets jsonFileName columnStart rowStart columnEnd rowEnd firstRowIsHeaders id sheetName6 : Result<DataTable, string> =
          
            match checkForNetConn() with
            | Ok _      ->
                           try
                               try
                                   //This C# custom-made library function does contain a try-catch block (I did not omit it as I had thought)
                                   //but let's pretend for this moment that the C# try-catch block is not implemented 
                                   ReadingFromGoogleSheets.ReadFromGoogleSheets(
                                   jsonFileName, 
                                   id, 
                                   sheetName6,
                                   columnStart, rowStart, columnEnd, rowEnd, firstRowIsHeaders
                                   ) 
                                   |> Option.ofObj 
                                   |> function   
                                       | Some value -> Ok value 
                                       | None       -> Error "Cannot Read Data From DataTable"
                               finally
                               ()
                           with
                           | ex -> Error (string ex)
            | Error err -> Error err  
                
        let writingToGoogleSheets dtGoogle jsonFileName1 id sheetName6 endIndex : Result<unit, string> =
              
            match checkForNetConn() with
            | Ok _      ->
                           try
                               try
                                   //This C# custom-made library function does contain a try-catch block (I did not omit it as I had thought)
                                   //but let's pretend for this moment that the C# try-catch block is not implemented 
                                   let writeToGoogleSheets = new WritingToGoogleSheets(dtGoogle)
                                   Ok <| writeToGoogleSheets.WriteToGoogleSheets(jsonFileName1, id, sheetName6, endIndex) 
                               finally
                               ()
                           with
                           | ex -> Error (string ex)
            | Error err -> Error err          
    
    
       
    
        //***************************** Now I'll try some refactoring *******************************************************************

        let mapResult f err : Result<'a, 'b> = 
               f 
               |> Option.ofObj            
               |> function   
                   | Some value -> Ok value 
                   | None       -> Error err    
        
        let tryWith f : Result<'a, string> =            
            try
                try                 
                   f
                finally
                ()
            with
            | ex -> Error (string ex)
    
        let checkForNetConnRF(): Result<_, string> = //Ok value not needed
                
            let f = 
                use myPing = new Ping()
                let host = "google.com"
                let buffer = Array.zeroCreate 32
                let timeout = 1000
                let pingOptions = PingOptions()
    
                myPing.Send(host, timeout, buffer, pingOptions)
                |> Option.ofObj
                |> Option.bind (fun pingReply ->
                                                match pingReply.Status = IPStatus.Success with
                                                | true  -> Some (pingReply |> ignore) //pingReply not needed
                                                | false -> None 
                               )
                |> function
                    | Some () -> Ok () 
                    | None    -> Error "12029 Cannot Connect"
                
            tryWith f           
    
        let readingFromGoogleSheetsRF jsonFileName columnStart rowStart columnEnd rowEnd firstRowIsHeaders id sheetName6 : Result<DataTable, string> =
             
            match checkForNetConnRF() with
            | Ok _      ->
                            let f = 
                                ReadingFromGoogleSheets.ReadFromGoogleSheets(
                                    jsonFileName, 
                                    id, 
                                    sheetName6,
                                    columnStart, rowStart, columnEnd, rowEnd, firstRowIsHeaders
                                    ) 
    
                            tryWith <| mapResult f "Cannot Read Data From DataTable"
    
            | Error err -> Error err  
                   
        let writingToGoogleSheetsRF dtGoogle jsonFileName1 id sheetName6 endIndex : Result<unit, string> =
                 
            match checkForNetConnRF() with
            | Ok _      ->
                           let f = 
                               let writeToGoogleSheets = new WritingToGoogleSheets(dtGoogle)
                               Ok <| writeToGoogleSheets.WriteToGoogleSheets(jsonFileName1, id, sheetName6, endIndex)                 
                           
                           tryWith f   
    
            | Error err -> Error err    
            

        let doSomethingWithResult1() = 
           
            let emptyDataTable = new DataTable("Empty")
                   
            match readingFromGoogleSheetsRF "" 1 1 1 1 true "" "" with
            | Ok value  -> value, "Data from Google Sheets successfully read and loaded"
            | Error err -> emptyDataTable, err
                        
            //value  -> to be processed  
            //string -> into a textbox
           
        let doSomethingWithResult2() = 
           
            let dummyDataTable = new DataTable("Dummy")
                      
            match writingToGoogleSheetsRF dummyDataTable "" "" "" 1 with
            | Ok value  -> value, "Data successfully uploaded to Google Sheets"
            | Error err -> (), err
                      
            //value  -> to be processed  
            //string -> into a textbox
