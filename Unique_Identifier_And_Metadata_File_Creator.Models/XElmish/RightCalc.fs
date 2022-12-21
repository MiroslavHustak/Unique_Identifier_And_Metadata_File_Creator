namespace Unique_Identifier_And_Metadata_File_Creator.Models

module RightCalc =

    open System
    open System.IO;

    open Elmish
    open Elmish.WPF
    
    open MainLogicDG
    open MainLogicMetaData

    open Errors
    open ROP_Functions
    open Elmish.Support
    open Helpers.Parsing
    open Helpers.Process
    open Helpers.Deserialisation

    open FSharp.Control   
    open System.Threading
    open System.Windows.Media
    
    type ProgressIndicator = Idle | InProgress of percent: int
    
    type Model =
        {
            MainTextBoxText: string
            StartWithNumber: string
            Prefix: string
            LowLimit: string
            HighLimit: string
            ProgressIndeterminateRight: bool
            ProgressBackgroundRight: Brush
            ProgressIndicatorRight: ProgressIndicator    
            UniqueIdentifierButtonIsEnabled: bool
            MetaDataButtonIsEnabled: bool
        }    
    
    let private deserializeMe xmlFile defaultRecord = 

        let message ex = String.Empty 
        let title = "Závažná chyba při deserializaci"
        let perform x = deserialize xmlFile 
        tryWith perform (fun x -> ()) (fun ex -> ())  |> deconstructor3 title message defaultRecord 

    let initialModel xmlFile = 
       
        let deserialize: Settings.Common_Settings = deserializeMe xmlFile Settings.Common_Settings.Default //nutno zadat explicitne typ quli generics <'a>  v deserialisation v Helpers 

        {
            MainTextBoxText = String.Empty
            StartWithNumber = String.Empty
            Prefix = deserialize.prefix
            LowLimit = String.Empty
            HighLimit = String.Empty
            ProgressIndeterminateRight = false
            ProgressBackgroundRight = Brushes.LightSkyBlue
            ProgressIndicatorRight = Idle
            UniqueIdentifierButtonIsEnabled = true
            MetaDataButtonIsEnabled = true
        }
    
    let init(): Model * Cmd<'a> = initialModel "json.xml", Cmd.none 
                
    type Msg =  
        | StartWithNumberChanged of string
        | LowLimitChanged of string
        | HighLimitChanged of string
        | UpdateStatusRight of progress:int
        | WorkIsCompleteRight of string
        | CreateUniqueIdentifier 
        | CreateMetaDataFiles   

    let update (msg: Msg) (m: Model) : Model * Cmd<Msg> = 
        match msg with    
        | StartWithNumberChanged s   -> { m with StartWithNumber = s }, Cmd.none
        | LowLimitChanged low        -> { m with LowLimit = low }, Cmd.none
        | HighLimitChanged high      -> { m with HighLimit = high }, Cmd.none
        | UpdateStatusRight progress -> { m with ProgressIndicatorRight = InProgress progress; ProgressBackgroundRight = Brushes.White }, Cmd.none 
        | WorkIsCompleteRight result -> { m with ProgressIndicatorRight = Idle; UniqueIdentifierButtonIsEnabled = true; MetaDataButtonIsEnabled = true; ProgressBackgroundRight = Brushes.LightSkyBlue; MainTextBoxText = result; ProgressIndeterminateRight = false }, Cmd.none 
        | CreateUniqueIdentifier     ->                                       
                                        let result = 
                                            let myOptionList = [ parseMeOption m.StartWithNumber; parseMeOption m.LowLimit; parseMeOption m.HighLimit ] 
                                            myOptionList |> List.map (fun item -> 
                                                                                match item with 
                                                                                | Some value -> 
                                                                                                match value <= 0 with 
                                                                                                | true  -> 0  //prestoze parseMe take dava nulu, davam zde parseMeOption pro zobecneni 
                                                                                                | false -> value
                                                                                | None       -> 0   //prestoze parseMe take dava nulu, davam zde parseMeOption pro zobecneni                                           
                                                                     )      
                                        
                                        let maxNumberOfCharacters = 
                                            let deserialize: Settings.Common_Settings = deserializeMe <| "json.xml" <| Settings.Common_Settings.Default //nutno zadat explicitne typ quli generics <'a>  v deserialisation v Helpers 
                                            String.length deserialize.exampleString - String.length deserialize.prefix 
                                        
                                        let startWithNumber = result.Item 0                                               

                                        match (result |> List.contains 0) || String.length (string startWithNumber) > maxNumberOfCharacters with
                                        | true  -> { m with ProgressIndicatorRight = Idle; MetaDataButtonIsEnabled = true; UniqueIdentifierButtonIsEnabled = true; MainTextBoxText = "Chybný rozdíl limitních hodnot anebo pracovní značení či limity nebyly správně zadány." + "\n"; }, Cmd.none
                                        | false ->                        
                                                   let tryWith =
                                                       let myMsgBox() = 
                                                           let message = "Proces stanovení řetezce digitalizační sady je pozastaven, neb momentálně procesor pracuje ze všech sil na tvorbě průvodek. Buď zruš konzolovou aplikaci (křížek vpravo nahoře), anebo jednej dle pokynů v okně konzolové aplikace. Až poté klikni na \"OK\", jinak se ti budu objevovat znovu a znovu. Pak se přerušený proces opět rozběhne."
                                                           let title = "Prosím o trpělivost"
                                                           error1 message title
                                                       try
                                                          try  
                                                             let fInfodat = new FileInfo(@"Xlsx_To_Jpg_PDF\XLSX_To_PDF_JPG_forWPF.exe")
                                                                            |> Option.ofObj  
                                                                            |> optionToGenerics @"Xlsx_To_Jpg_PDF\XLSX_To_PDF_JPG_forWPF.exe" "FileInfo()"
                                                             match fInfodat.Exists with 
                                                             | true  ->  Seq.initInfinite (fun _ -> detectFileRunning "XLSX_To_PDF_JPG_forWPF") 
                                                                         |> Seq.takeWhile ((=) true) 
                                                                         |> Seq.iter      (fun _ -> myMsgBox() |> ignore
                                                                                                    Thread.Sleep(1000)) //Ceka do doby, nez se dokonci tvorba pruvodek, necham testovat po vterine, at to chudak pocitac nekontroluje furt.                                                                                        
                                                             | false -> failwith (sprintf "Soubor %A nenalezen" fInfodat)  

                                                             //failwith "Simulated exception2" 
                                                             let delayedCmd (dispatch: Msg -> unit): unit =   
                                                                 let rowStart = result.Item 1
                                                                 let rowEnd = result.Item 2   
                                                                 let delayedDispatch: Async<unit> = 
                                                                     async
                                                                        {
                                                                            let reportProgress progressValue = dispatch (UpdateStatusRight progressValue)                                                               
                                                                            let! hardWork = Async.StartChild (async { return getUniqueIdentifier rowStart rowEnd startWithNumber reportProgress }) 
                                                                            let! result3 = hardWork 
                                                                            let result = result3 
                                                                            dispatch (WorkIsCompleteRight result)
                                                                        }                                       
                                                                 Async.StartImmediate delayedDispatch                                                            
                                                                                                               
                                                             { m with MainTextBoxText = "Příslušné hodnoty se načítají a zpracovávají, dochází k tvorbě CSV a XLSX souborů a odstraňují se některé překlepy ..."; MetaDataButtonIsEnabled = false; UniqueIdentifierButtonIsEnabled = false; ProgressIndicatorRight = InProgress 0; ProgressBackgroundRight = Brushes.White; ProgressIndeterminateRight = true }, Cmd.ofSub delayedCmd 
                                                          finally
                                                          ()     
                                                       with
                                                       | ex -> { m with ProgressIndicatorRight = Idle; MetaDataButtonIsEnabled = true; UniqueIdentifierButtonIsEnabled = true; MainTextBoxText = ex.Message; }, Cmd.none        
                                                   tryWith     
        | CreateMetaDataFiles           -> 
                                           let tryWith = //jen kvuli WorkIsCompleteRight, abych mohl ButtonIsEnabled opet uvest do stavu true 
                                               try
                                                  try  
                                                     //failwith "Simulated exception2" 
                                                     let explicitCmd (dispatch: Msg -> unit): unit =  
                                                         createMetadataFiles() 
                                                         dispatch (WorkIsCompleteRight "Průvodky jsou vytvářeny externím procesem, sleduj hlašení v okně konzolové aplikace.")
                                                     { m with UniqueIdentifierButtonIsEnabled = false; MetaDataButtonIsEnabled = false; }, Cmd.ofSub explicitCmd                                                                                                                 
                                                  finally
                                                  ()
                                               with
                                               | ex -> { m with MetaDataButtonIsEnabled = true; UniqueIdentifierButtonIsEnabled = true; MainTextBoxText = ex.Message; }, Cmd.none        
                                           tryWith    
                                       
    //cmdIf disables the relevant button, cmd does not
    let bindings(): Binding<Model,Msg> list =
        [
          "MainTextBoxText"                  |> Binding.oneWay(fun m -> m.MainTextBoxText)
          "UniqueIdentifierButton"           |> Binding.cmdIf(CreateUniqueIdentifier, fun m -> match m.ProgressIndicatorRight with Idle -> true | _ -> false)  
          "StartWithNumber"                  |> Binding.twoWay((fun m -> m.StartWithNumber), (fun newVal -> newVal |> StartWithNumberChanged))
          "LowLimit"                         |> Binding.twoWay((fun m -> m.LowLimit), (fun newVal -> newVal |> LowLimitChanged))
          "HighLimit"                        |> Binding.twoWay((fun m -> m.HighLimit), (fun newVal -> newVal |> HighLimitChanged))
          "ProgressRightIndeter"             |> Binding.oneWay(fun m -> m.ProgressIndeterminateRight)  
          "Prefix"                           |> Binding.oneWay(fun m -> m.Prefix)  
          "ProgressRightBackg"               |> Binding.oneWay(fun m -> m.ProgressBackgroundRight) 
          "ProgressRight"                    |> Binding.oneWay(fun m -> match m.ProgressIndicatorRight with Idle -> 0.0 | InProgress v -> float v)
          "MetaDataButton"                   |> Binding.cmd CreateMetaDataFiles 
          "UniqueIdentifierButtonIsEnabled"  |> Binding.oneWay(fun m -> m.UniqueIdentifierButtonIsEnabled)
          "MetaDataButtonIsEnabled"          |> Binding.oneWay(fun m -> m.MetaDataButtonIsEnabled) 
        ]