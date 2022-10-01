(*
MIT License for software design in this source file

Copyright (c) 2021 Bent Tranberg

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
*)

namespace Unique_Identifier_And_Metadata_File_Creator.Models

open System
open System.IO
open System.Windows
open System.Windows.Media
open Microsoft.FSharp.Core
open System.Text.RegularExpressions

(*
open System.Windows vyzaduje doplneni nize uvedeneho do fsproj
<TargetFramework>net5.0-windows</TargetFramework>
<UseWPF>true</UseWPF>
*)

module SettingsDG =
    
    open Elmish
    open Elmish.WPF
    
    open Helpers
    open SettingsDG    
    open ROP_Functions
    open PatternBuilders
    open Helpers.Serialisation
    open Helpers.Deserialisation

    let inline xor a b = (a || b) && not (a && b) //zatim nevyuzito
    
    let [<Literal>] limitNumberOfCharacters = 9
    
    let private cond x y =           
        match String.IsNullOrWhiteSpace(string x) with
        | true  -> y     
        | false -> x
    
    type Model =
        {
            ArchiveCodeTextBoxText: string
            ArchiveCodeCheckBoxIsChecked: bool  
            NADTextBoxTextText: string
            NADCheckBoxIsChecked: bool
            POMTextBoxTextText: string
            POMCheckBoxIsChecked: bool
            INVTextBox1TextText: string
            INVTextBox2TextText: string
            INVCheckBoxLeftIsChecked: bool
            INVCheckBoxLeftIsEnabled: bool
            INVCheckBoxRightIsChecked: bool
            SGTextBox1TextText: string
            SGTextBox2TextText: string
            SGTextBox3TextText: string
            SGCheckBoxIsChecked: bool
            SGCheckBoxIsEnabled: bool
            KARTextBox1TextText: string
            KARTextBox2TextText: string
            KARCheckBoxLeftIsChecked: bool
            KARCheckBoxLeftIsEnabled: bool
            KARCheckBoxRightIsChecked: bool

            InfoTextBoxText: string
            InfoTextBoxForeground: SolidColorBrush 
        }

    let defaultValues message = 
        {
            ArchiveCodeTextBoxText = DG_Settings.Default.archiveCodeTxb 
            ArchiveCodeCheckBoxIsChecked = DG_Settings.Default.archiveCodeCkbx
            NADTextBoxTextText = string DG_Settings.Default.nadTxb 
            NADCheckBoxIsChecked = DG_Settings.Default.nadCkbx
            POMTextBoxTextText = string DG_Settings.Default.pomTxb 
            POMCheckBoxIsChecked = DG_Settings.Default.pomCkbx
            INVTextBox1TextText = DG_Settings.Default.invTxb1 
            INVTextBox2TextText = string DG_Settings.Default.invTxb2 
            INVCheckBoxLeftIsChecked = DG_Settings.Default.invCkbxLeft
            INVCheckBoxLeftIsEnabled = DG_Settings.Default.invCkbxLeftE
            INVCheckBoxRightIsChecked = DG_Settings.Default.invCkbxRight
            SGTextBox1TextText = DG_Settings.Default.sgTxb1 
            SGTextBox2TextText = DG_Settings.Default.sgTxb2 
            SGTextBox3TextText = DG_Settings.Default.sgTxb3 
            SGCheckBoxIsChecked = DG_Settings.Default.sgCkbx
            SGCheckBoxIsEnabled = DG_Settings.Default.sgCkbxE
            KARTextBox1TextText = DG_Settings.Default.karTxb1 
            KARTextBox2TextText = string DG_Settings.Default.karTxb2 
            KARCheckBoxLeftIsChecked = DG_Settings.Default.karCkbxLeft
            KARCheckBoxLeftIsEnabled = DG_Settings.Default.karCkbxLeftE
            KARCheckBoxRightIsChecked = DG_Settings.Default.karCkbxRight

            InfoTextBoxText = message
            InfoTextBoxForeground = Brushes.Green 
        }       

    let initialModel xmlFile message = 
        
        let deserializeWhenLoaded message = 
            
            let deserialize: SettingsDG.DG_Settings = deserialize xmlFile //nutno zadat explicitne typ quli generics <'a> v deserialisation v Helpers 

            let myInitialModel = 
                {  
                    ArchiveCodeTextBoxText = deserialize.archiveCodeTxb 
                    ArchiveCodeCheckBoxIsChecked = deserialize.archiveCodeCkbx
                    NADTextBoxTextText = string deserialize.nadTxb 
                    NADCheckBoxIsChecked = deserialize.nadCkbx
                    POMTextBoxTextText = string deserialize.pomTxb 
                    POMCheckBoxIsChecked = deserialize.pomCkbx
                    INVTextBox1TextText = deserialize.invTxb1 
                    INVTextBox2TextText = string deserialize.invTxb2 
                    INVCheckBoxLeftIsChecked = deserialize.invCkbxLeft
                    INVCheckBoxLeftIsEnabled = deserialize.invCkbxLeftE
                    INVCheckBoxRightIsChecked = deserialize.invCkbxRight
                    SGTextBox1TextText = deserialize.sgTxb1 
                    SGTextBox2TextText = deserialize.sgTxb2 
                    SGTextBox3TextText = deserialize.sgTxb3
                    SGCheckBoxIsChecked = deserialize.sgCkbx
                    SGCheckBoxIsEnabled = deserialize.sgCkbxE
                    KARTextBox1TextText = deserialize.karTxb1 
                    KARTextBox2TextText = string deserialize.karTxb2 
                    KARCheckBoxLeftIsChecked = deserialize.karCkbxLeft
                    KARCheckBoxLeftIsEnabled = deserialize.karCkbxLeftE
                    KARCheckBoxRightIsChecked = deserialize.karCkbxRight

                    InfoTextBoxText = message
                    InfoTextBoxForeground = Brushes.Blue                    
                }             
            myInitialModel

        try
            try       
                deserializeWhenLoaded message 
            finally
                () 
        with
        | _ as ex ->                           
                    try
                        try
                            serialize DG_Settings.Default <| xmlFile
                            defaultValues ((+) "Byly načteny defaultní hodnoty, jelikož se objevila následující chyba: " (string ex.Message))  
                        finally
                        () 
                    with
                    | _ as  ex -> defaultValues ((+) "Defaultní hodnoty neuloženy, jelikož se objevila následující chyba: " (string ex.Message))  
        
    let updateSettings m =           

           let condPathChar (x: string) (y: string) =  //nevydedukoval
               MyPatternBuilder //komentare viz XElmish/Settings.fs
                    {  
                        let! _ = not (String.IsNullOrWhiteSpace(string x)), y 
                        let! _ =             
                            let left = 
                               System.IO.Path.GetInvalidPathChars() 
                               |> Option.ofObj 
                               |> optionToGenerics "nepovolených znaků" "Path.GetInvalidPathChars()"     
                               |> Array.map(fun item -> x.Contains(string item)) 
                               |> Array.contains true                                     
                            let right = Regex.IsMatch(x, """^[a-z]:\\(?:[^\\/:*?"<>|\r\n]+\\)*[^\\/:*?"<>|\r\n]*$""")  
                            let result = (not left) && right && (not (x.Contains "+"))
                            result, y                                        
                        let! _ = Directory.Exists(x), y 
                        return x
                    }   
           
           let condInt x y =                             
                match Parsing.parseMeOption (string x) with
                | Some value -> 
                                let result = 
                                    match value <= limitNumberOfCharacters && value > 0 with 
                                    | true  -> value                                           
                                    | false -> limitNumberOfCharacters  //limitni hodnota v prislusnem textboxu
                                result                              
                | None      -> y
                
           
           let myCopyOfSettings() =  //to je, co se ulozi
               {
                   archiveCodeTxb = cond m.ArchiveCodeTextBoxText DG_Settings.Default.archiveCodeTxb
                   archiveCodeCkbx = unbox (m.ArchiveCodeCheckBoxIsChecked)
                   nadTxb = condInt m.NADTextBoxTextText DG_Settings.Default.nadTxb
                   nadCkbx = unbox (m.NADCheckBoxIsChecked)
                   pomTxb = condInt m.POMTextBoxTextText DG_Settings.Default.pomTxb 
                   pomCkbx = unbox (m.POMCheckBoxIsChecked)
                   invTxb1 = cond m.INVTextBox1TextText DG_Settings.Default.invTxb1 
                   invTxb2 = condInt m.INVTextBox2TextText DG_Settings.Default.invTxb2 
                   invCkbxLeft = unbox (m.INVCheckBoxLeftIsChecked)
                   invCkbxLeftE = unbox (m.INVCheckBoxLeftIsEnabled)  
                   invCkbxRight = unbox (m.INVCheckBoxRightIsChecked)
                   sgTxb1 = cond m.SGTextBox1TextText DG_Settings.Default.sgTxb1 
                   sgTxb2 = cond m.SGTextBox2TextText DG_Settings.Default.sgTxb2 
                   sgTxb3 = cond m.SGTextBox3TextText DG_Settings.Default.sgTxb3 
                   sgCkbx = unbox (m.SGCheckBoxIsChecked)
                   sgCkbxE = unbox (m.SGCheckBoxIsEnabled)
                   karTxb1 = cond m.KARTextBox1TextText DG_Settings.Default.karTxb1 
                   karTxb2 = condInt m.KARTextBox2TextText DG_Settings.Default.karTxb2
                   karCkbxLeft = unbox (m.KARCheckBoxLeftIsChecked)  
                   karCkbxLeftE = unbox (m.KARCheckBoxLeftIsEnabled)  
                   karCkbxRight = unbox (m.KARCheckBoxRightIsChecked)        
               }
           
           let myCopyOfModel() =  //to je, co se hned zobrazi
               {
                   ArchiveCodeTextBoxText = cond m.ArchiveCodeTextBoxText DG_Settings.Default.archiveCodeTxb 
                   ArchiveCodeCheckBoxIsChecked = unbox (m.ArchiveCodeCheckBoxIsChecked)
                   NADTextBoxTextText = string (condInt m.NADTextBoxTextText DG_Settings.Default.nadTxb) 
                   NADCheckBoxIsChecked = unbox (m.NADCheckBoxIsChecked)
                   POMTextBoxTextText = string (condInt m.POMTextBoxTextText DG_Settings.Default.pomTxb) 
                   POMCheckBoxIsChecked = unbox (m.POMCheckBoxIsChecked)
                   INVTextBox1TextText = cond m.INVTextBox1TextText DG_Settings.Default.invTxb1 
                   INVTextBox2TextText = string (condInt m.INVTextBox2TextText DG_Settings.Default.invTxb2) 
                   INVCheckBoxLeftIsChecked = unbox (m.INVCheckBoxLeftIsChecked)
                   INVCheckBoxLeftIsEnabled = unbox (m.INVCheckBoxLeftIsEnabled)
                   INVCheckBoxRightIsChecked = unbox (m.INVCheckBoxRightIsChecked)
                   SGTextBox1TextText = cond m.SGTextBox1TextText DG_Settings.Default.sgTxb1 
                   SGTextBox2TextText = cond m.SGTextBox2TextText DG_Settings.Default.sgTxb2 
                   SGTextBox3TextText = cond m.SGTextBox3TextText DG_Settings.Default.sgTxb3 
                   SGCheckBoxIsChecked = unbox (m.SGCheckBoxIsChecked)
                   SGCheckBoxIsEnabled = unbox (m.SGCheckBoxIsEnabled)
                   KARTextBox1TextText = cond m.KARTextBox1TextText DG_Settings.Default.karTxb1 
                   KARTextBox2TextText = string (condInt m.KARTextBox2TextText DG_Settings.Default.karTxb2) 
                   KARCheckBoxLeftIsChecked = unbox (m.KARCheckBoxLeftIsChecked) 
                   KARCheckBoxLeftIsEnabled = unbox (m.KARCheckBoxLeftIsEnabled) 
                   KARCheckBoxRightIsChecked = unbox (m.KARCheckBoxRightIsChecked) 

                   InfoTextBoxText = m.InfoTextBoxText
                   InfoTextBoxForeground = m.InfoTextBoxForeground
               }           
           
           let strEx (ex: exn) = (+) "Hodnoty neuloženy, neboť se objevila následující chyba: " (string ex.Message)
           try
              try 
                 serialize <| myCopyOfSettings() <| "jsonDG.xml"
                 initialModel "jsonDG.xml" |> ignore
                 { myCopyOfModel() with InfoTextBoxForeground = m.InfoTextBoxForeground; InfoTextBoxText = m.InfoTextBoxText }                
              finally
              () 
           with
           | _ as ex -> { myCopyOfModel() with InfoTextBoxForeground = Brushes.Red; InfoTextBoxText = strEx ex } 
       
    let init(): Model * Cmd<'a> = initialModel "jsonDG.xml" "Hodnoty načteny. V případě prázdné kolonky je vždy dosazena defaultní hodnota.", Cmd.none 
        
    type Msg =
        | CancelButtonEvent 
        | DefaultButtonEvent         
        | ArchiveCodeTextBox of string
        | ArchiveCodeCheckBox of bool
        | NADTextBox of string
        | NADCheckBox of bool
        | POMTextBox of string
        | POMCheckBox of bool
        | INVTextBox1 of string
        | INVTextBox2 of string
        | INVCheckBoxLeft of bool
        | INVCheckBoxLeftE of bool
        | INVCheckBoxRight of bool
        | SGTextBox1 of string
        | SGTextBox2 of string
        | SGTextBox3 of string
        | SGCheckBox of bool
        | SGCheckBoxE of bool
        | KARTextBox1 of string
        | KARTextBox2 of string
        | KARCheckBoxLeft of bool 
        | KARCheckBoxLeftE of bool 
        | KARCheckBoxRight of bool    
        | InfoTextBoxForeground //of SolidColorBrush
                 
    // A command in Elmish is a function that can trigger events into the dispatch loop. // A command is essentially a function that takes a dispatch function as input and returns unit:
    let update (msg: Msg) (m: Model) : Model * Cmd<Msg> = 
                       
            let str = sprintf "Pokud nebyla zadaná prázdná hodnota nebo chybná číselná či textová hodnota, hodnota %s byla změněna. V opačném případě byla dosazena defaultní nebo limitní hodnota."
            let strCbx = sprintf "Status \"%s\" byl právě změněn."

            match msg with          
            | CancelButtonEvent   -> initialModel "jsonDGBackUp.xml" "Načteny hodnoty ze záložního souboru (hodnoty uložené před spuštěním programu). \n" |> updateSettings, Cmd.none                                   
            | DefaultButtonEvent  -> 
                                     let title = "Rozmysli si to !!!"
                                     let buttons = MessageBoxButton.YesNo  
                                     let message = "Kliknutím na \"Ano\" nebo \"Yes\" bude proveden návrat k defaultním hodnotám a navždy ztratíš nastavené hodnoty. Je to opravdu to, co chceš?"
                                     MessageBox.Show(message, title, buttons, MessageBoxImage.Warning, MessageBoxResult.No)                                     
                                     |> function
                                        | MessageBoxResult.Yes -> defaultValues "Načteny defaultní hodnoty." |> updateSettings, Cmd.none 
                                        | _                    -> m, Cmd.none                
                                      
            | ArchiveCodeTextBox archiveCodeTxb   -> { m with ArchiveCodeTextBoxText = archiveCodeTxb; InfoTextBoxText = str "archivního kódu"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none
            | ArchiveCodeCheckBox archiveCodeCkbx -> { m with ArchiveCodeCheckBoxIsChecked = archiveCodeCkbx; InfoTextBoxText = strCbx "archivního kódu"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none 
            | NADTextBox nadTxb   -> { m with NADTextBoxTextText = nadTxb; InfoTextBoxText = str "počtu znaků čísla NAD"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none
            | NADCheckBox nadCkbx -> { m with NADCheckBoxIsChecked = nadCkbx; InfoTextBoxText = strCbx "čísla NAD"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none  
            | POMTextBox pomTxb   -> { m with POMTextBoxTextText = pomTxb; InfoTextBoxText = str "počtu znaků čísla pomůcky"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none
            | POMCheckBox pomCkbx -> { m with POMCheckBoxIsChecked = pomCkbx; InfoTextBoxText = strCbx "čísla pomůcky"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none  
            | INVTextBox1 invTxb1 -> { m with INVTextBox1TextText = invTxb1; InfoTextBoxText = str "předpony inventárního čísla"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none
            | INVTextBox2 invTxb2 -> { m with INVTextBox2TextText = invTxb2; InfoTextBoxText = str "počtu znaků inventárního čísla"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none
            | INVCheckBoxLeft invCkbxLeft    -> match invCkbxLeft with
                                                | true  -> { m with INVCheckBoxLeftIsChecked = invCkbxLeft; SGCheckBoxIsEnabled = false; SGCheckBoxIsChecked = false; InfoTextBoxText = strCbx "inventárního čísla";  InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none   
                                                | false -> { m with INVCheckBoxLeftIsChecked = invCkbxLeft; SGCheckBoxIsEnabled = true; InfoTextBoxText = strCbx "inventárního čísla"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none    
            | INVCheckBoxLeftE  invCkbxLeftE -> { m with INVCheckBoxLeftIsEnabled = invCkbxLeftE; } |> updateSettings, Cmd.none  
            | INVCheckBoxRight invCkbxRight  -> { m with INVCheckBoxRightIsChecked = invCkbxRight; InfoTextBoxText = strCbx "příslušného checkboxu"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none 
            | SGTextBox1 sgTxb1   -> { m with SGTextBox1TextText = sgTxb1; InfoTextBoxText = str "předpony signatury"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none
            | SGTextBox2 sgTxb2   -> { m with SGTextBox2TextText = sgTxb2; InfoTextBoxText = str "příslušného políčka"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none
            | SGTextBox3 sgTxb3   -> { m with SGTextBox3TextText = sgTxb3; InfoTextBoxText = str "příslušného políčka"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none
            | SGCheckBox sgCkbx   -> match sgCkbx with
                                     | true  -> { m with SGCheckBoxIsChecked = sgCkbx; INVCheckBoxLeftIsChecked = false; KARCheckBoxLeftIsChecked = false; INVCheckBoxLeftIsEnabled = false; KARCheckBoxLeftIsEnabled = false; InfoTextBoxText = strCbx "signatury"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none  
                                     | false -> { m with SGCheckBoxIsChecked = sgCkbx; INVCheckBoxLeftIsEnabled = true; KARCheckBoxLeftIsEnabled = true; InfoTextBoxText = strCbx "signatury"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none               
            | SGCheckBoxE sgCkbxE -> { m with SGCheckBoxIsEnabled = sgCkbxE; } |> updateSettings, Cmd.none               
            | KARTextBox1 karTxb1 -> { m with KARTextBox1TextText = karTxb1; InfoTextBoxText = str "předpony čísla kartonu"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none
            | KARTextBox2 karTxb2 -> { m with KARTextBox2TextText = karTxb2; InfoTextBoxText = str "počtu znaků čísla kartonu"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none
            | KARCheckBoxLeft karCkbxLeft   -> 
                                              match karCkbxLeft with
                                              | true  -> { m with KARCheckBoxLeftIsChecked = karCkbxLeft; SGCheckBoxIsEnabled = false; SGCheckBoxIsChecked = false; InfoTextBoxText = strCbx "čísla kartonu"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none  
                                              | false -> { m with KARCheckBoxLeftIsChecked = karCkbxLeft; SGCheckBoxIsEnabled = true; InfoTextBoxText = strCbx "čísla kartonu"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none 
            | KARCheckBoxRight karCkbxRight -> { m with KARCheckBoxRightIsChecked = karCkbxRight; InfoTextBoxText = strCbx "příslušného checkboxu"; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none                                              
            | KARCheckBoxLeftE karCkbxLeftE -> { m with KARCheckBoxLeftIsEnabled = karCkbxLeftE; } |> updateSettings, Cmd.none   
            | InfoTextBoxForeground         -> { m with InfoTextBoxForeground = Brushes.Black }, Cmd.none //tohle je barva, na kterou se to po pohybu mysi nebo po zvednuti klavesy zmeni
      
   
    let condInt y x =   //musim x a y prehodit, nebot hodnota pres piping je dosazena az nakonec vpravo   
        match Parsing.parseMeOption (string x) with
        | Some value -> 
                        let result = 
                            match value <= limitNumberOfCharacters && value > 0 with 
                            | true  -> value                                           
                            | false -> limitNumberOfCharacters  //limitni hodnota v prislusnem textboxu
                        string result                              
        | None      -> string y
         

    let condition x y = (cond x y) |> condInt y 

    let bindings(): Binding<Model,Msg> list =
        [ 
          "CancelButton"          |> Binding.cmd CancelButtonEvent        
          "DefaultButton"         |> Binding.cmd DefaultButtonEvent      
          "ArchiveCodeTextBox"    |> Binding.twoWay((fun m -> m.ArchiveCodeTextBoxText), (fun newVal m -> cond (string newVal) m.ArchiveCodeTextBoxText |> ArchiveCodeTextBox)) 
          "ArchiveCodeCheckBox"   |> Binding.twoWay((fun m -> m.ArchiveCodeCheckBoxIsChecked), (fun newVal -> newVal |> ArchiveCodeCheckBox))
          "NADTextBox"            |> Binding.twoWay((fun m -> m.NADTextBoxTextText), (fun newVal m -> condition (string newVal) m.NADTextBoxTextText |> NADTextBox)) 
          "NADCheckBox"           |> Binding.twoWay((fun m -> m.NADCheckBoxIsChecked), (fun newVal -> newVal |> NADCheckBox))
          "POMTextBox"            |> Binding.twoWay((fun m -> m.POMTextBoxTextText), (fun newVal m -> condition (string newVal) m.POMTextBoxTextText |> POMTextBox)) 
          "POMCheckBox"           |> Binding.twoWay((fun m -> m.POMCheckBoxIsChecked), (fun newVal -> newVal |> POMCheckBox))
          "INVTextBox1"           |> Binding.twoWay((fun m -> m.INVTextBox1TextText), (fun newVal m -> cond (string newVal) m.INVTextBox1TextText |> INVTextBox1))
          "INVTextBox2"           |> Binding.twoWay((fun m -> m.INVTextBox2TextText), (fun newVal m -> condition (string newVal) m.INVTextBox2TextText |> INVTextBox2))
          "INVCheckBoxLeft"       |> Binding.twoWay((fun m -> m.INVCheckBoxLeftIsChecked), (fun newVal -> newVal |> INVCheckBoxLeft))
          "INVCheckBoxLeftE"      |> Binding.twoWay((fun m -> m.INVCheckBoxLeftIsEnabled), (fun newVal -> newVal |> INVCheckBoxLeftE))
          "INVCheckBoxRight"      |> Binding.twoWay((fun m -> m.INVCheckBoxRightIsChecked), (fun newVal -> newVal |> INVCheckBoxRight))
          "SGTextBox1"            |> Binding.twoWay((fun m -> m.SGTextBox1TextText), (fun newVal m -> cond (string newVal) m.SGTextBox1TextText |> SGTextBox1)) 
          "SGTextBox2"            |> Binding.twoWay((fun m -> m.SGTextBox2TextText), (fun newVal m -> cond (string newVal) m.SGTextBox2TextText |> SGTextBox2)) 
          "SGTextBox3"            |> Binding.twoWay((fun m -> m.SGTextBox3TextText), (fun newVal m -> cond (string newVal) m.SGTextBox3TextText |> SGTextBox3)) 
          "SGCheckBox"            |> Binding.twoWay((fun m -> m.SGCheckBoxIsChecked), (fun newVal -> newVal |> SGCheckBox))   
          "SGCheckBoxE"           |> Binding.twoWay((fun m -> m.SGCheckBoxIsEnabled), (fun newVal -> newVal |> SGCheckBoxE))   
          "KARTextBox1"           |> Binding.twoWay((fun m -> m.KARTextBox1TextText), (fun newVal m -> cond (string newVal) m.KARTextBox1TextText |> KARTextBox1)) 
          "KARTextBox2"           |> Binding.twoWay((fun m -> m.KARTextBox2TextText), (fun newVal m -> condition (string newVal) m.KARTextBox2TextText |> KARTextBox2)) 
          "KARCheckBoxLeft"       |> Binding.twoWay((fun m -> m.KARCheckBoxLeftIsChecked), (fun newVal -> newVal |> KARCheckBoxLeft))
          "KARCheckBoxLeftE"      |> Binding.twoWay((fun m -> m.KARCheckBoxLeftIsEnabled), (fun newVal -> newVal |> KARCheckBoxLeftE))
          "KARCheckBoxRight"      |> Binding.twoWay((fun m -> m.KARCheckBoxRightIsChecked), (fun newVal -> newVal |> KARCheckBoxRight))
          "TriggerEvent"          |> Binding.cmd InfoTextBoxForeground
          "InfoTextBox"           |> Binding.oneWay(fun m -> m.InfoTextBoxText)
          "InfoTextBoxForeground" |> Binding.oneWay(fun m -> m.InfoTextBoxForeground)
        ]      