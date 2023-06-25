module MainLogicMetaData

open System
open System.IO
open FSharp.Core
open Microsoft.Win32

open Types
open Settings
open Auxiliaries

open Errors
open ROP_Functions
open PatternBuilders
open Helpers.Process
open Helpers.Deserialisation

//***************************** auxiliary function definitions ***********************************

//v tomto modulu nejsou

//***************************** main module functions **********************************  

let createMetadataFiles() = 

    //******************** main sub functions ***********************
    let perform x = 
        let processName = "XLSX_To_PDF" //"Excel" "AcroRd32" "FoxitPDFReader" "scalc"     
        let message = sprintf "Je třeba uzavřít aplikaci %s. Bez toho tě to dál nepustí. Už jsi uzavřel/a? Tak klikni na tlačítko \"OK\" pro pokračování programu." processName
        let title = "Upozornění"
        do closeSingleProcess message title processName    
        //a pro jistotu jeste.... 
        do killSingleProcess("XLSX_To_PDF", String.Empty, false) //"Excel" "AcroRd32" "FoxitPDFReader" "scalc"
    tryWith perform (fun x -> ()) (fun ex -> ()) |> deconstructor4 ()
             
    let deserialize =
        let message ex = sprintf "Vyskytla se chyba při deserializaci. Klikni na \"OK\" pro restart programu a odstraň problém. K tomu dopomáhej ti následující chybové hlášení: \n\n%s" ex
        let title = "Závažná chyba při deserializaci"                    
        let perform x = deserialize "json.xml" 
        tryWith perform (fun x -> ()) (fun ex -> ()) |> deconstructor3 title message Common_Settings.Default              

    let openFileDialog() = 
        new OpenFileDialog() 
        |> Option.ofObj
        |> optionToGenerics2 "OpenFileDialog()" (new OpenFileDialog())                           
     
    let procFn (str1: string) (str2: seq<string>) =         
        System.Diagnostics.Process.Start(str1, str2)
        |> Option.ofObj  
        |> optionToGenerics2 "System.Diagnostics.Process.Start()" (new System.Diagnostics.Process())                   
                                                    
    let procKill() =
        let proc = procFn String.Empty Seq.empty                              
        proc.Kill |> ignore   
    
    //************************ main function utilising sub functions ****************************
    let startMetadataFileCreation() = 
                     
        let openFileDialog =         
            let perform x = 
                let openFileDialog = openFileDialog() 
                openFileDialog                 
            tryWith perform (fun x -> ()) (fun ex -> ()) |> deconstructor4 (openFileDialog())              
        
        openFileDialog.Filter <- @"Excel soubor (*.xlsx, *.xls)| *.xlsx; *.xls"         
           
        match openFileDialog.ShowDialog().HasValue with 
        | true  -> openFileDialog.FileName
                   |> Option.ofObj                   
                   |> function 
                       | Some value -> 
                                      MyPatternBuilder    
                                          {     
                                             let jpgPath = deserialize.jpgPath 
                                             let pdfPath = deserialize.pdfPath 

                                             let! _ = not (value = String.Empty), () 
                                             let! _ = not (isFileLocked value), () 
                                             let mySeq =                                                           
                                                 let exampleString = deserialize.exampleString 
                                                 let prefix = deserialize.prefix 
                                                 let fontTypeCode = string deserialize.fontType 
                                                 let columnRange = string deserialize.columnEnd 
                                                 seq { value; jpgPath; pdfPath; exampleString; prefix; fontTypeCode; columnRange }     
                                             let myBoolArray = [| deleteAllFilesInDir pdfPath; deleteAllFilesInDir jpgPath |]  
                                             let! _ = not (myBoolArray |> Array.contains true), ()   
                                             let proc = procFn @"Xlsx_To_Jpg_PDF\XLSX_To_PDF_JPG_forWPF.exe" mySeq
                                                                                                  
                                             return proc |> ignore //.exe -> do binu (release) v C# projektu, bo tam se to spusta 
                                          }                                                             
                       | None       -> procKill()   
        | false -> procKill()  
                
    startMetadataFileCreation() 
   
   (*
    //pokud umistim rop try with blok s celou funkci tady na konec, tak to uz reaguje na neposkytnuti souboru pri aktivaci, coz je zbytecna hlaska navic
      let perform x = startMetadataFileCreation() 
      tryWith perform (fun x -> ()) (fun ex -> ()) |> deconstructor4 
    *)

