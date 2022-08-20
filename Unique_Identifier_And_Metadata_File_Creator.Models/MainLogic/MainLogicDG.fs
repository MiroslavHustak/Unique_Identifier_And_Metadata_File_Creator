﻿module MainLogicDG

open System
open FSharp.Core
open System.Threading
open System.Text.RegularExpressions

open Errors
open Settings
open GoogleAPI
open SettingsDG
open ROP_Functions
open Helpers.Process
open PatternBuilders
open Helpers.MyString
open DiscriminatedUnions
open Helpers.Deserialisation
open Creating_CSV_And_Excel_Files

//***************************** auxiliary function definitions ***********************************

let private (++++++) a b c d e f g = a + b + c + d + e + f + g

let private myTasks task1 task2 task3 =           
    [| 
      task1 
      task2 
      task3 
    |] 
    |> Async.Parallel //viz strana 433 Isaac Abraham
    |> Async.Catch
    |> Async.RunSynchronously
    |> function
       | Choice1Of2 result    -> result
       | Choice2Of2 (ex: exn) -> error0 ex.Message       
                                 
let private whatIs(x: obj) =
    match x with
    | :? TaskResults as du -> du  //aby nedoslo k nerizene chybe behem runtime
    | _                    -> error4 "error 4 - x :?> TaskResults"                              
                              x :?> TaskResults


//***************************** main module functions ********************************** 
 
let private getUniqueIdentifierCsvXlsxGoogle rowStart rowEnd startWithNumber reportProgress = //readGoogleSheetsData |> createUniqueIdentifier |> createCsvXlsxGoogle 
        
    //do closeSingleProcess "Excel"
    //do closeSingleProcess "scalc"

    //a pro jistotu jeste.... 
    //do KillSingleProcess("Excel", String.Empty, false) 
    //do KillSingleProcess("scalc", String.Empty, false) //nezavre to

    //*********************** uvodni deserializace ***********************
     
    let message ex = sprintf "Vyskytla se chyba při deserializaci. Klikni na \"OK\" pro restart programu a odstraň problém. K tomu dopomáhej ti následující chybové hlášení: \n\n%s" ex
    let title = "Závažná chyba při deserializaci"
    
    let deserializeCS: Common_Settings =
        let perform x = deserialize "json.xml" 
        tryWith perform (fun x -> ()) (fun ex -> failwith) |> deconstructor3 title message Common_Settings.Default         
    
    let deserializeDG: DG_Settings =
        let perform x = deserialize "jsonDG.xml" 
        tryWith perform (fun x -> ()) (fun ex -> failwith) |> deconstructor3 title message DG_Settings.Default

    let jsonFileName1 = deserializeCS.jsonFileName1 //u async workflows nebo tasks nesmi v jednom okamziku odkaz na stejny json //radeji jsem quli tomu porusil pravidla in scopingu
     //let jsonFileName2 = deserializeCS.jsonFileName2 //u async workflows nebo tasks nesmi v jednom okamziku odkaz na stejny json 
    
    //******************** three main sub-functions ***********************
   
    // 1)
    let readGoogleSheetsData() = 

        let dtGoogle = 
            let firstRowIsHeaders = deserializeCS.firstRowIsHeaders 
            let columnStart = deserializeCS.columnStart
            let columnEnd = deserializeCS.columnEnd //columnEnd musi byt 13 nebo mene vzhledem k poctu columns        
            let id = deserializeCS.id
            let sheetName6 = deserializeCS.sheetName6  
            optionToGenerics 
            <| "dat z Google Sheets"
            <| "readingFromGoogleSheets"
            <| readingFromGoogleSheets jsonFileName1 columnStart rowStart columnEnd rowEnd firstRowIsHeaders id sheetName6//pozor na jsonFileName, nesmi se otevirat ten samy soubor  
        dtGoogle
        // |> Option.ofObj to bych taky mohl donekonecna u DataTable.....                                
        // |> optionToGenerics "dat z Google tabulky" "readGoogleSheetsData()"  
    
    // 2)
    let createUniqueIdentifier (dtGoogle: Data.DataTable) = //nevydedukoval
                            
        //214000010-0743-0251-sg4KLs84_1940
        //214000010-0743-0251-ic0352-kar0283

        let checkBoxFn x chkbx = 
            match chkbx with 
            | true  -> x
            | false -> String.Empty
        
        let msg =  //potrebuji array quli naslednemu retezci
            [| 0 .. abs (rowEnd - rowStart) |] 
            |> Array.mapi (fun i item ->                 //Array.parallel tady tusim nefungovalo s DataTable
                                        Thread.Sleep(15) //aby to krasne vypadalo :-)
                                        reportProgress(i)

                                        let archiveCodeTxb = deserializeDG.archiveCodeTxb
                                        let archiveCodeCkbx = deserializeDG.archiveCodeCkbx
                                        let nadTxb = deserializeDG.nadTxb 
                                        let nadCkbx = deserializeDG.nadCkbx 
                                        let pomTxb = deserializeDG.pomTxb 
                                        let pomCkbx = deserializeDG.pomCkbx 
                                        let invTxb1 = deserializeDG.invTxb1 
                                        let invTxb2 = deserializeDG.invTxb2 
                                        let invCkbxLeft = deserializeDG.invCkbxLeft 
                                        let invCkbxRight = deserializeDG.invCkbxRight 
                                        let sgTxb1 = deserializeDG.sgTxb1 
                                        let sgTxb2 = deserializeDG.sgTxb2 
                                        let sgTxb3 = deserializeDG.sgTxb3
                                        let sgCkbx = deserializeDG.sgCkbx 
                                        let karTxb1 = deserializeDG.karTxb1 
                                        let karTxb2 = deserializeDG.karTxb2 
                                        let karCkbxLeft = deserializeDG.karCkbxLeft 
                                        let karCkbxRight = deserializeDG.karCkbxRight 

                                        let prefix = deserializeCS.prefix
                                        let exampleString = deserializeCS.exampleString
                                        let columnEnd = deserializeCS.columnEnd //columnEnd musi byt 13 nebo mene vzhledem k poctu columns 
                                        
                                        let myRegex = new Regex(@"\s+")//detekuje vsechny spaces v retezci

                                        [ 0 .. columnEnd - 1 ] |> List.iteri (fun j item -> dtGoogle.Rows.[i].[j] <- (string dtGoogle.Rows.[i].[j])
                                                                                                                     .Replace("–", "-").Replace(@"//", @"/").Replace(@"\\", @"\")  
                                                                                                                     .Replace(",,", ",").Replace(";", ",")    
                                                                                            dtGoogle.Rows.[i].[j] <- myRegex.Replace(string dtGoogle.Rows.[i].[j], " ")  //Regex.Replace(string dtGoogle.Rows.[i].[j], @"\s+", " ")                         
                                                                             )    
                                        
                                        let fnDGset = checkBoxFn archiveCodeTxb archiveCodeCkbx
                                        
                                        //0, 1, 3, . .. n odpovida sloupcum v Google Spreadsheets A, B, C, .... n
                                        dtGoogle.Rows.[i].[4] <- (string dtGoogle.Rows.[i].[4]).Replace(" ", String.Empty)                                       
                                        let str = string dtGoogle.Rows.[i].[4]
                                        let numberOfZeros = abs (nadTxb - String.length str)
                                        let fnNAD = checkBoxFn (sprintf "%s%s" <| GetString(numberOfZeros, "0") <| str) nadCkbx  
                                        
                                        dtGoogle.Rows.[i].[5] <- (string dtGoogle.Rows.[i].[5]).Replace(" ", String.Empty)
                                        let str = string dtGoogle.Rows.[i].[5]
                                        let numberOfZeros = abs (pomTxb - String.length str)
                                        let fnCisloPomucky = checkBoxFn (sprintf "%s%s" <| GetString(numberOfZeros, "0") <| str) pomCkbx  
                                       
                                        dtGoogle.Rows.[i].[6] <- (string dtGoogle.Rows.[i].[6]).Replace(" ", String.Empty)                                                                                
                                        let fnInventarniCislo =                                      
                                            MyPatternBuilder    
                                                {   
                                                    let str = string dtGoogle.Rows.[i].[6]
                                                    let bckgProcess = checkBoxFn (sprintf "%s%s" <| invTxb1 <| (string dtGoogle.Rows.[i].[6])) invCkbxLeft

                                                    let! _ = invCkbxRight, bckgProcess                                                  
                                                    let! _ = ((String.length str) <= 4), bckgProcess //max. 4 znaky pro inventarni cislo
                                                    let result = 
                                                        let numberOfZeros = abs (invTxb2 - String.length str)
                                                        checkBoxFn (sprintf "%s%s%s" <| invTxb1 <| GetString(numberOfZeros, "0") <| str) invCkbxLeft                                                   
                                                    return result
                                                }                                        
                                        
                                        dtGoogle.Rows.[i].[7] <- (string dtGoogle.Rows.[i].[7]).Replace(" ", String.Empty)
                                        let signatura = (string dtGoogle.Rows.[i].[7]).Replace(sgTxb2, sgTxb3)                                    
                                        let fnSignatura = checkBoxFn (sprintf "%s%s" sgTxb1 signatura) sgCkbx    
                                        
                                        dtGoogle.Rows.[i].[8] <- (string dtGoogle.Rows.[i].[8]).Replace(" ", String.Empty)                                        
                                        let fnCisloKartonu = 
                                            match karCkbxRight with
                                            | false -> checkBoxFn (sprintf "%s%s" <| karTxb1 <| (string dtGoogle.Rows.[i].[8])) karCkbxLeft
                                            | true  -> let str = string dtGoogle.Rows.[i].[8]
                                                       let numberOfZeros = abs (karTxb2 - String.length str)
                                                       checkBoxFn (sprintf "%s%s%s" <| karTxb1 <| GetString(numberOfZeros, "0") <| str) karCkbxLeft  
                                                                                               
                                        dtGoogle.Rows.[i].[11] <- (string dtGoogle.Rows.[i].[11]).Replace(" ", String.Empty)

                                        let numberOfZeros = (exampleString |> String.length) - (string (i + startWithNumber) |> String.length) - (prefix |> String.length)
                                        dtGoogle.Rows.[i].[0] <- sprintf "%s%s%s" prefix (GetString(abs numberOfZeros, "0")) (string (i + startWithNumber))

                                        let result = sprintf "%s-%s-%s-%s-%s-%s" 
                                                     <| fnDGset 
                                                     <| fnNAD 
                                                     <| fnCisloPomucky
                                                     <| fnInventarniCislo
                                                     <| fnSignatura
                                                     <| fnCisloKartonu 
                                        
                                        let lastCharacter = 
                                            let result = result.Replace("--", "-").Replace("---", "-")
                                            result.[result.Length - 1]                                    
                                        
                                        let result = 
                                            match lastCharacter with
                                            | '-' -> let result = result.Remove(result.Length - 1, 1)
                                                     result  
                                            | _   -> result
                                        dtGoogle.Rows.[i].[1] <- result                                         
                                                                               
                                        MyPatternBuilder    
                                            {   
                                                let str1 = string dtGoogle.Rows.[i].[7]
                                                let str2 = string dtGoogle.Rows.[i].[11]

                                                let! _ = sgCkbx, String.Empty                                                  
                                                let! _ = (String.length str1) >= 2 && (String.length str2) >= 2, (sprintf "Počet znaků buď u signatury nebo u datace vzniku v Google tabulce na řádku %i je menší než 2" <| i+  rowStart) + "\n" 
                                                let! _ = not (str1.Substring((str1.Length - 2), 2) = str2.Substring((str2.Length - 2), 2)), String.Empty  
                                                return (sprintf "Signatura \"%s\" neodpovídá dataci vzniku \"%s\" v Google tabulce na řádku %i" <| str1 <| str2 <| i + rowStart) + "\n"
                                            }    
                                            
                          )
        dtGoogle, msg 
    
    // 3)
    let createCsvXlsxGoogle (myTuple: Data.DataTable*string[]) =   
        
        let dtGoogle = fst myTuple
        let msg = snd myTuple
        
        match string dtGoogle.Rows.[0].[0] with
        | "error"->
                    let myDG_Sada = 
                        {
                            errorDG = [||]
                            msg1 = "Nesprávně zadaný spodní nebo horní limit." + "\n"  
                            msg2 = "Řetězec DG sady, csv soubor a excel soubor nebyly vytvořeny :-( ." 
                            msg3 = String.Empty 
                        }    
                    myDG_Sada
        | _      ->                     
                    let auxLow = string dtGoogle.Rows.[0].[0]
                    let auxHigh = string dtGoogle.Rows.[dtGoogle.Rows.Count - 1].[0]
                    let nameOfCVSFile = sprintf"%s az %s" auxLow auxHigh

                    //CREATING CSV
                    let csv() =  //obe funkce a deserializace uz su uvnitr try with 
                        let pathCSV = deserializeCS.csvPath 
                        
                        deleteAllFilesInDir pathCSV |> ignore
                               
                        let lastCharacter = pathCSV.[pathCSV.Length - 1] 
                        let pathCSV = 
                            match lastCharacter.Equals("\\") with
                            | true  -> pathCSV.Remove(pathCSV.Length - 1, 1)
                            | false -> pathCSV  
                        CreateCsvFile.WriteIntoCSV(dtGoogle, pathCSV, nameOfCVSFile) //string vychazejici z impure prostredi C# ma return mj. aji null
                        |> Option.ofObj
                        |> optionToGenerics "csv souboru" "WriteIntoCSV()" //k tomu nedojde, neb v C#  pred return null je naprogramovana hlaska s restartem, ale z duvodu zobecneni nechavam
                    
                    //CREATING EXCEL
                    let excel() =  //obe funkce a deserializace uz su uvnitr try with 
                        let nameOfXLSXFile = sprintf"%s DGSada %s.xlsx" nameOfCVSFile (DateTime.Now.ToString("dd-MM-yyyy"))       
                        let path = deserializeCS.xlsxPath 
                        
                        deleteAllFilesInDir path |> ignore

                        let lastCharacter = path.[path.Length - 1] 
                        let path = 
                            match lastCharacter.Equals("\\") with
                            | true  -> path.Remove(path.Length - 1, 1)
                            | false -> path
                        let excelFilename = sprintf"%s%s" path nameOfXLSXFile
                        CreateExcelFile.CreateExcelDocument(dtGoogle, excelFilename)  
                    
                    //SENDING TO GOOGLE 
                    let google() = 
                                   let id = deserializeCS.id
                                   let sheetName = deserializeCS.sheetName  
                                   let endIndex = deserializeCS.numOfRowsGoogle + 999 // vyznam 999 je uveden v GoogleSheetsHelper 
                                   writingToGoogleSheets dtGoogle jsonFileName1 id sheetName endIndex // try catch je primo v GoogleSheets -> WritingToGoogleSheets (C#) 

                    //z duvodu pouziti return bool a string nelze tasks od .NET, ktere vyzaduji stejny typ vsude
                    (*                   
                    let ts = [| 
                                Task.Factory.StartNew(fun () -> csv())   
                                Task.Factory.StartNew(fun () -> excel())   
                                Task.Factory.StartNew(fun () -> google()) 
                             |]
                    Task.WaitAll(ts |> Seq.cast<Task> |> Array.ofSeq)
                     *)
                                                        
                    let du: TaskResults[] = myTasks 
                                            <| async { return MyString (csv()) } 
                                            <| async { return MyBool (excel()) }
                                            <| async { return MyUnit (google()) }
       
                    let myString = whatIs <| du.GetValue(0)                        
                                   |> function 
                                      | MyString value -> value                                                  
                                      | _              -> error4 "error4 - MyString csv()"
                                                          String.Empty //whatever           
   
                    let myBool = whatIs <| du.GetValue(1)                         
                                 |> function 
                                    | MyBool value -> value                                           
                                    | _            -> error4 "error4 - MyBool excel()"
                                                      false //whatever   

                    //jen quli error4, jinak nepotrebne
                    let myUnit = whatIs <| du.GetValue(2)                         
                                 |> function 
                                    | MyUnit value -> value                                           
                                    | _            -> error4 "error4 - MyUnit google()"
                                                      ()   
                    
                    let myBool = 
                        match myBool with
                        | true  -> "Převod hodnot z Google tabulky do Excelu se zdařil."
                        | false -> "Převod hodnot z Google tabulky do Excelu se pravděpodobně nezdařil :-( ." 
                        
                    let msg2 = (String.concat <| "\n" <| [| myString; myBool |]) 
                    
                    let msg3 = (String.concat <| String.Empty <| msg)  //tohle zrobi prazdny string z pole, kere obsahuje polozky pouze s hodnotou String.Empty
        
                    let msg3 = 
                        match msg3.Length with
                        | 0 -> "Ani jsem žádné chyby nenašel :-)."  
                        | _ -> "Bohužel se vyskytly následující chyby:" 

                    let myDG_Sada = 
                        {
                            errorDG = msg
                            msg1 = "Byl to těžký výpočet, dal mi pořádně zabrat, ale dokončil jsem jej." + "\n"  
                            msg2 = msg2 + "\n"  
                            msg3 = msg3 + "\n" 
                        }        
                    myDG_Sada
    
    readGoogleSheetsData() |> createUniqueIdentifier |> createCsvXlsxGoogle

 //******************************************** Main function ***************************************************
let getUniqueIdentifier rowStart rowEnd startWithNumber reportProgress =    
    let mainTxbString myDGSada = 
        let result = 
            match (rowEnd - rowStart) > 0 && startWithNumber > 0  with //zachyti to aji startWithNumber vetsi nez int
            | true  -> myDGSada //getUniqueIdentifierCsvXlsxGoogle rowStart rowEnd startWithNumber reportProgress
            | false ->     
                       let myDG_Sada = 
                           {
                               errorDG = [||]
                               msg1 = "Chybný rozdíl limitních hodnot anebo pracovní značení či limity nebyly správně zadány." + "\n"  
                               msg2 = "Řetězec DG sady, csv soubor a excel soubor nebyly vytvořeny :-( ." 
                               msg3 = String.Empty 
                           }   
                       myDG_Sada  
        (++++++) 
        <| result.msg1
        <| "\n"
        <| result.msg2
        <| "\n"
        <| result.msg3
        <| "\n"
        <| (String.concat <| String.Empty <| result.errorDG)  
    
    getUniqueIdentifierCsvXlsxGoogle rowStart rowEnd startWithNumber reportProgress |> mainTxbString


(*
Directory

Directory is a static class.
This should be used when we want to perform one operation in the folder.
As there is not any requirement to create object for Directory class, so not any overhead for using this.

Directory Info Class

DirectoryInfo is not a static class.
If user is required to perform lot of operations on one directory like creation, deletion, file listing etc, then DirectoryInfo class should be used.
A separate object is created for performing all directory related operations.
It's effective if you are going to perform many operations on the folder because, once the object is created, 
it has all the necessary information about the folder such as its creation time, last access time and attributes.
All the members of the DirectoryInfo class are instance members.
*)