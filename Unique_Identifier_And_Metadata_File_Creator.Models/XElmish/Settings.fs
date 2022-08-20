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

module Settings =
    
    open Elmish
    open Elmish.WPF
    
    open Helpers
    open Settings
    open ROP_Functions
    open PatternBuilders
    open Helpers.Serialisation
    open Helpers.Deserialisation

    let inline xor a b = (a || b) && not (a && b) //zatim nevyuzito

    let [<Literal>] limitGoogle = 0
    let [<Literal>] limitColumnStart = 12
    let [<Literal>] limitColumnEnd = 13

     //pouze pro malo pravdepodobny pripad, kdyby nekdo v kodu nebo primo do json.xml/jsonBackup.xml zadal prazdne hodnoty
    let private cond x y =           
        match String.IsNullOrWhiteSpace(string x) with
        | true  -> y     
        | false -> x
      
    type Model =
        {
            FontTypeTextBoxText: string
            PrefixTextBoxText: string   
            ExampleStringTextBoxText: string
            CsvPathTextBoxText: string  
            XlsxPathTextBoxText: string  
            JpgPathTextBoxText: string  
            PdfPathTextBoxText: string  
            NumOfRowsGoogleTextBoxText: string  
                      
            FirstRowIsHeadersCheckBoxIsChecked: bool  
            JsonFileName1TextBoxText: string  
            IdTextBoxText: string  
            
            SheetNameTextBoxText: string  
            SheetName6TextBoxText: string  
            ColumnStartTextBoxText: string   
            ColumnEndTextBoxText: string   

            InfoTextBoxText: string
            InfoTextBoxForeground: SolidColorBrush 
            
            FontTypeLabel: string 
            SheetNameLabel: string     
            SheetName6Label: string   
            JsonFileName1Label: string   
            IdLabel: string   
            PrefixLabel: string   
            ExampleStringLabel: string   
            NumOfRowsGoogleLabel: string   
            ColumnStartLabel: string   
            ColumnEndLabel: string  
            CsvPathLabel: string   
            XlsxPathLabel: string   
            JpgPathLabel: string   
            PdfPathLabel: string   
            FirstRowIsHeadersCheckBox: string  
        }

    let defaultValues message = 
        {
            FontTypeTextBoxText = Common_Settings.Default.fontType 
            PrefixTextBoxText = Common_Settings.Default.prefix
            ExampleStringTextBoxText = Common_Settings.Default.exampleString 
            CsvPathTextBoxText = Common_Settings.Default.csvPath
            XlsxPathTextBoxText = Common_Settings.Default.xlsxPath
            JpgPathTextBoxText = Common_Settings.Default.jpgPath
            PdfPathTextBoxText = Common_Settings.Default.pdfPath
            NumOfRowsGoogleTextBoxText = string Common_Settings.Default.numOfRowsGoogle
                          
            FirstRowIsHeadersCheckBoxIsChecked = Common_Settings.Default.firstRowIsHeaders
            JsonFileName1TextBoxText = Common_Settings.Default.jsonFileName1
            IdTextBoxText = Common_Settings.Default.id 
                
            SheetNameTextBoxText = Common_Settings.Default.sheetName
            SheetName6TextBoxText = Common_Settings.Default.sheetName6 
            ColumnStartTextBoxText = string Common_Settings.Default.columnStart
            ColumnEndTextBoxText = string Common_Settings.Default.columnEnd

            InfoTextBoxText = message
            InfoTextBoxForeground = Brushes.Green 

            FontTypeLabel = "Typ fontu pro průvodku (T = Times New Roman, C = Calibri, A = Arial)" 
            SheetNameLabel = "Název listu v Google Sheets pro výsledný DG řetezec" 
            SheetName6Label = "Název listu v Google Sheets se zdrojovými daty" 
            JsonFileName1Label = "Cesta k Json1" 
            IdLabel = "Id" 
            PrefixLabel = "Předpona (například LT-)" 
            ExampleStringLabel = "Příklad pracovního značení složky (například LT-01442)" 
            NumOfRowsGoogleLabel = "Přidání řádků do Google tabulky" 
            ColumnStartLabel = "Číslo sloupce s pracovním značením" 
            ColumnEndLabel = "Číslo sloupce s poznámkou (poslední sloupec), max. 13" 
            CsvPathLabel = "Cesta k CSV souboru" 
            XlsxPathLabel = "Cesta k XLSX souboru" 
            JpgPathLabel = "Cesta k JPG průvodkám" 
            PdfPathLabel = "Cesta k PDF průvodkám" 
            FirstRowIsHeadersCheckBox = "Header"             
        }       

    let initialModel xmlFile message = 
        
        let deserializeWhenLoaded message = 
            
            let deserialize = deserialize xmlFile // tady nebylo nutno zadat explicitne typ quli generics <'a> v deserialisation v Helpers, vydedukoval sikula jeden...

            let myInitialModel = 
                {  
                    FontTypeTextBoxText = deserialize.fontType 
                    PrefixTextBoxText = deserialize.prefix
                    ExampleStringTextBoxText = deserialize.exampleString    
                    CsvPathTextBoxText = deserialize.csvPath
                    XlsxPathTextBoxText = deserialize.xlsxPath
                    JpgPathTextBoxText = deserialize.jpgPath
                    PdfPathTextBoxText = deserialize.pdfPath

                    NumOfRowsGoogleTextBoxText = string deserialize.numOfRowsGoogle
                          
                    FirstRowIsHeadersCheckBoxIsChecked = deserialize.firstRowIsHeaders
                    JsonFileName1TextBoxText = deserialize.jsonFileName1
                    IdTextBoxText = deserialize.id 
                
                    SheetNameTextBoxText = deserialize.sheetName
                    SheetName6TextBoxText = deserialize.sheetName6 
                    ColumnStartTextBoxText = string deserialize.columnStart
                    ColumnEndTextBoxText = string deserialize.columnEnd

                    InfoTextBoxText = message
                    InfoTextBoxForeground = Brushes.Blue

                    FontTypeLabel = "Typ fontu pro průvodku (T = Times New Roman, C = Calibri, A = Arial)" 
                    SheetNameLabel = "Název listu v Google Sheets pro výsledný DG řetezec" 
                    SheetName6Label = "Název listu v Google Sheets se zdrojovými daty" 
                    JsonFileName1Label = "Cesta k Json1" 
                    IdLabel = "Id" 
                    PrefixLabel = "Předpona (například LT-)" 
                    ExampleStringLabel = "Příklad pracovního značení složky (například LT-01442)" 
                    NumOfRowsGoogleLabel = "Přidání řádků do Google tabulky" 
                    ColumnStartLabel = "Číslo sloupce s pracovním značením" 
                    ColumnEndLabel = "Číslo sloupce s poznámkou (poslední sloupec), max. 13" 
                    CsvPathLabel = "Cesta k CSV souboru" 
                    XlsxPathLabel = "Cesta k XLSX souboru" 
                    JpgPathLabel = "Cesta k JPG průvodkám" 
                    PdfPathLabel = "Cesta k PDF průvodkám" 
                    FirstRowIsHeadersCheckBox = "Header" 
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
                            serialize Common_Settings.Default xmlFile
                            defaultValues ((+) "Byly načteny defaultní hodnoty, jelikož se objevila následující chyba: " (string ex.Message))  
                        finally
                        () 
                    with
                    | _ as  ex -> defaultValues ((+) "Defaultní hodnoty neuloženy, jelikož se objevila následující chyba: " (string ex.Message))  
        
    let updateSettings m =           

           let condPathChar (x: string) (y: string) =  //nevydedukoval
               MyPatternBuilder 
                    {  
                        let! _ = not (String.IsNullOrWhiteSpace(string x)), y 
                        let! _ =             
                            let left = 
                               System.IO.Path.GetInvalidPathChars() //The array returned from this method is not guaranteed to contain the complete set of characters that are invalid in file and directory names. 
                               |> Option.ofObj 
                               |> optionToGenerics "nepovolených znaků" "Path.GetInvalidPathChars()"     
                               |> Array.map(fun item -> x.Contains(string item)) //dostaneme pole hodnot bool
                               |> Array.contains true //tj. obsahuje nepovoleny znak 
                               //podminka nutna ....  (obsahuje ASCII znaky, ktere muj regex nezachyti) 
                               //...nikoliv vsak dostacujici (GetInvalidPathChars() nezachyti +, <> a vetsinu dalsich znaku z regexu)          
                            let right = Regex.IsMatch(x, """^[a-z]:\\(?:[^\\/:*?"<>|\r\n]+\\)*[^\\/:*?"<>|\r\n]*$""")  //tento regex pattern nezachyti +
                            let result = (not left) && right && (not (x.Contains "+"))
                            result, y                                        
                        let! _ = Directory.Exists(x), y  
                        return x
                    }
               
           let condFontType x y =   
                let myList = [ "C"; "T"; "A" ]
                match myList |> List.contains x with
                | true  -> x  
                | false -> y  
             
           let condInt x y limit =   
               let result =            
                   match Parsing.parseMeOption (string x) with
                   | Some value -> 
                                  let result = 
                                      match value <= limit && value > 0 with 
                                      | true  -> value                                           
                                      | false -> limit 
                                  result                              
                   | None      -> y
               result 

           let myCopyOfSettings() =  //to je, co se ulozi
               {
                   fontType = condFontType m.FontTypeTextBoxText Common_Settings.Default.fontType 
                   prefix = cond m.PrefixTextBoxText Common_Settings.Default.prefix 
                   exampleString = cond m.ExampleStringTextBoxText Common_Settings.Default.exampleString 
                   csvPath = condPathChar m.CsvPathTextBoxText Settings.Common_Settings.Default.csvPath 
                   xlsxPath = condPathChar m.XlsxPathTextBoxText Settings.Common_Settings.Default.xlsxPath 
                   jpgPath = condPathChar m.JpgPathTextBoxText Settings.Common_Settings.Default.jpgPath 
                   pdfPath = condPathChar m.PdfPathTextBoxText Settings.Common_Settings.Default.pdfPath 
                   numOfRowsGoogle = condInt m.NumOfRowsGoogleTextBoxText Common_Settings.Default.numOfRowsGoogle limitGoogle 
       
                   firstRowIsHeaders = unbox (m.FirstRowIsHeadersCheckBoxIsChecked)
                   jsonFileName1 = condPathChar m.JsonFileName1TextBoxText Common_Settings.Default.jsonFileName1 
                   id = cond m.IdTextBoxText Common_Settings.Default.id
       
                   sheetName = cond m.SheetNameTextBoxText Common_Settings.Default.sheetName
                   sheetName6 = cond m.SheetName6TextBoxText Common_Settings.Default.sheetName6
                   columnStart = condInt m.ColumnStartTextBoxText Common_Settings.Default.columnStart limitColumnStart
                   columnEnd = condInt m.ColumnEndTextBoxText Common_Settings.Default.columnEnd limitColumnEnd
               }
           
           let myCopyOfModel() =  //to je, co se hned zobrazi
               {
                   FontTypeTextBoxText = condFontType m.FontTypeTextBoxText Common_Settings.Default.fontType 
                   PrefixTextBoxText = cond m.PrefixTextBoxText Common_Settings.Default.prefix 
                   ExampleStringTextBoxText = cond m.ExampleStringTextBoxText Common_Settings.Default.exampleString 
                   CsvPathTextBoxText = condPathChar m.CsvPathTextBoxText Common_Settings.Default.csvPath 
                   XlsxPathTextBoxText = condPathChar m.XlsxPathTextBoxText Common_Settings.Default.xlsxPath 
                   JpgPathTextBoxText = condPathChar m.JpgPathTextBoxText Common_Settings.Default.jpgPath 
                   PdfPathTextBoxText = condPathChar m.PdfPathTextBoxText Common_Settings.Default.pdfPath 
                   NumOfRowsGoogleTextBoxText = string (condInt m.NumOfRowsGoogleTextBoxText Common_Settings.Default.numOfRowsGoogle limitGoogle)
                   
                   FirstRowIsHeadersCheckBoxIsChecked = unbox (m.FirstRowIsHeadersCheckBoxIsChecked)
                   JsonFileName1TextBoxText = condPathChar m.JsonFileName1TextBoxText Common_Settings.Default.jsonFileName1 
                   IdTextBoxText = cond m.IdTextBoxText Common_Settings.Default.id
       
                   SheetNameTextBoxText = cond m.SheetNameTextBoxText Common_Settings.Default.sheetName
                   SheetName6TextBoxText = cond m.SheetName6TextBoxText Common_Settings.Default.sheetName6
                   ColumnStartTextBoxText = string (condInt m.ColumnStartTextBoxText Common_Settings.Default.columnStart limitColumnStart)
                   ColumnEndTextBoxText = string (condInt m.ColumnEndTextBoxText Common_Settings.Default.columnEnd limitColumnEnd)

                   InfoTextBoxText = m.InfoTextBoxText
                   InfoTextBoxForeground = m.InfoTextBoxForeground

                   FontTypeLabel = m.FontTypeLabel 
                   SheetNameLabel = m.SheetNameLabel 
                   SheetName6Label = m.SheetName6Label 
                   JsonFileName1Label = m.JsonFileName1Label 
                   IdLabel = m.IdLabel 
                   PrefixLabel = m.PrefixLabel 
                   ExampleStringLabel = m.ExampleStringLabel 
                   NumOfRowsGoogleLabel = m.NumOfRowsGoogleLabel 
                   ColumnStartLabel = m.ColumnStartLabel 
                   ColumnEndLabel = m.ColumnEndLabel 
                   CsvPathLabel = m.CsvPathLabel 
                   XlsxPathLabel = m.XlsxPathLabel 
                   JpgPathLabel = m.JpgPathLabel 
                   PdfPathLabel = m.PdfPathLabel 
                   FirstRowIsHeadersCheckBox = m.FirstRowIsHeadersCheckBox 
               }           
           
           let strEx (ex: exn) = (+) "Hodnoty neuloženy, neboť se objevila následující chyba: " (string ex.Message)
           try
              try 
                 serialize <| myCopyOfSettings() <| "json.xml"
                 initialModel "json.xml" |> ignore
                 { myCopyOfModel() with InfoTextBoxForeground = m.InfoTextBoxForeground; InfoTextBoxText = m.InfoTextBoxText }                
              finally
              () 
           with
           | _ as ex -> { myCopyOfModel() with InfoTextBoxForeground = Brushes.Red; InfoTextBoxText = strEx ex } 
       
    let init(): Model * Cmd<'a> = initialModel "json.xml" "Hodnoty načteny. V případě prázdné kolonky je vždy dosazena defaultní hodnota.", Cmd.none 
        
    type Msg =
        | CancelButton2Event 
        | DefaultButton3Event 
        
        | FontTypeTextBox of string
        | PrefixTextBox of string
        | ExampleStringTextBox of string 
        | CsvPathTextBox of string
        | XlsxPathTextBox of string
        | JpgPathTextBox of string
        | PdfPathTextBox of string
        | NumOfRowsGoogleTextBox of string
                   
        | FirstRowIsHeadersCheckBox of bool
        | JsonFileName1TextBox of string
        | IdTextBox of string
         
        | SheetNameTextBox of string
        | SheetName6TextBox of string
        | ColumnStartTextBox of string
        | ColumnEndTextBox of string
        | InfoTextBoxForeground //of SolidColorBrush
                 
    // A command in Elmish is a function that can trigger events into the dispatch loop. // A command is essentially a function that takes a dispatch function as input and returns unit:
    let update (msg: Msg) (m: Model) : Model * Cmd<Msg> = 
                       
            let str = sprintf "Pokud nebyla zadaná prázdná hodnota nebo chybná číselná či textová hodnota, hodnota \"%s\" byla změněna. V opačném případě byla dosazena defaultní nebo limitní hodnota."
            let strCbx = sprintf "Hodnota \"%s\" byla změněna."

            match msg with          
            | CancelButton2Event  -> initialModel "jsonBackUp.xml" "Načteny hodnoty ze záložního souboru (hodnoty uložené před spuštěním programu). \n" |> updateSettings, Cmd.none                                   
            | DefaultButton3Event -> 
                                     let title = "Rozmysli si to !!!"
                                     let buttons = MessageBoxButton.YesNo  
                                     let message = "Kliknutím na \"Ano\" nebo \"Yes\" bude proveden návrat k defaultním hodnotám a navždy ztratíš nastavené hodnoty. Je to opravdu to, co chceš?"
                                     let result = MessageBox.Show(message, title, buttons, MessageBoxImage.Warning, MessageBoxResult.No)
                
                                     match result with
                                     | MessageBoxResult.Yes -> defaultValues "Načteny defaultní hodnoty." |> updateSettings, Cmd.none 
                                     | _                    -> m, Cmd.none    

            | FontTypeTextBox fontType   -> { m with FontTypeTextBoxText = fontType; InfoTextBoxText = str m.FontTypeLabel; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none
            | PrefixTextBox prefix       -> { m with PrefixTextBoxText = prefix; InfoTextBoxText = str m.PrefixLabel; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none
            | ExampleStringTextBox exampleString -> { m with ExampleStringTextBoxText = exampleString; InfoTextBoxText = str m.ExampleStringLabel; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none           
            | CsvPathTextBox csvPath     -> { m with CsvPathTextBoxText = csvPath; InfoTextBoxText = str m.CsvPathLabel; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none  
            | XlsxPathTextBox xlsxPath   -> { m with XlsxPathTextBoxText = xlsxPath; InfoTextBoxText = str m.XlsxPathLabel; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none  
            | JpgPathTextBox jpgPath     -> { m with JpgPathTextBoxText = jpgPath; InfoTextBoxText = str m.JpgPathLabel; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none  
            | PdfPathTextBox pdfPath     -> { m with PdfPathTextBoxText = pdfPath; InfoTextBoxText = str m.PdfPathLabel; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none    
            | NumOfRowsGoogleTextBox numOfRowsGoogle      -> { m with NumOfRowsGoogleTextBoxText = string numOfRowsGoogle; InfoTextBoxText = str m.NumOfRowsGoogleLabel; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none    
                              
            | FirstRowIsHeadersCheckBox firstRowIsHeaders -> { m with FirstRowIsHeadersCheckBoxIsChecked = firstRowIsHeaders; InfoTextBoxText = strCbx m.FirstRowIsHeadersCheckBox; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none    
            | JsonFileName1TextBox jsonFileName1 -> { m with JsonFileName1TextBoxText = jsonFileName1; InfoTextBoxText = str m.JsonFileName1Label; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none    
            | IdTextBox id                       -> { m with IdTextBoxText = id; InfoTextBoxText = str m.IdLabel; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none    
                    
            | SheetNameTextBox sheetName     -> { m with SheetNameTextBoxText = sheetName; InfoTextBoxText = str m.SheetNameLabel; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none    
            | SheetName6TextBox sheetName6   -> { m with SheetName6TextBoxText = sheetName6; InfoTextBoxText =  str m.SheetName6Label; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none    
            | ColumnStartTextBox columnStart -> { m with ColumnStartTextBoxText = string columnStart; InfoTextBoxText = str m.ColumnStartLabel; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none    
            | ColumnEndTextBox columnEnd     -> { m with ColumnEndTextBoxText = string columnEnd; InfoTextBoxText = str m.ColumnEndLabel; InfoTextBoxForeground = Brushes.Red } |> updateSettings, Cmd.none    
            | InfoTextBoxForeground          -> { m with InfoTextBoxForeground = Brushes.Black }, Cmd.none //tohle je barva, na kterou se to po pohybu mysi nebo po zvednuti klavesy zmeni
             
    let condFontType x y =   
        let myList = [ "C"; "T"; "A" ]
        match myList |> List.contains x with
        | true  -> x  
        | false -> y  
    
    let condInt y x =   //musim x a y prehodit, nebot hodnota pres piping je dosazena az nakonec vpravo     
        let result =            
            match Parsing.parseMeOption (string x) with
            | Some value -> 
                           let result = 
                               match value <= limitGoogle && value > 0 with 
                               | true  -> value                                           
                               | false -> limitGoogle 
                           string result                              
            | None      -> string y
        result 

    let condition x y = (cond x y) |> condInt y 

    let bindings(): Binding<Model,Msg> list =
        [ 
          "CancelButton2"           |> Binding.cmd CancelButton2Event        
          "DefaultButton3"          |> Binding.cmd DefaultButton3Event      
          "FontTypeTextBox"         |> Binding.twoWay((fun m -> m.FontTypeTextBoxText), (fun newVal m -> condFontType (string newVal) m.FontTypeTextBoxText |> FontTypeTextBox)) 
          "PrefixTextBox"           |> Binding.twoWay((fun m -> m.PrefixTextBoxText), (fun newVal m -> cond (string newVal) m.PrefixTextBoxText |> PrefixTextBox))
          "ExampleStringTextBox"    |> Binding.twoWay((fun m -> m.ExampleStringTextBoxText), (fun newVal m -> cond (string newVal) m.ExampleStringTextBoxText |> ExampleStringTextBox)) 
          "CsvPathTextBox"          |> Binding.twoWay((fun m -> m.CsvPathTextBoxText), (fun newVal m -> cond (string newVal) m.CsvPathTextBoxText |> CsvPathTextBox))
          "XlsxPathTextBox"         |> Binding.twoWay((fun m -> m.XlsxPathTextBoxText), (fun newVal m -> cond (string newVal) m.XlsxPathTextBoxText |> XlsxPathTextBox))
          "JpgPathTextBox"          |> Binding.twoWay((fun m -> m.JpgPathTextBoxText), (fun newVal m -> cond (string newVal) m.JpgPathTextBoxText |> JpgPathTextBox))
          "PdfPathTextBox"          |> Binding.twoWay((fun m -> m.PdfPathTextBoxText), (fun newVal m -> cond (string newVal) m.PdfPathTextBoxText |> PdfPathTextBox))
          "NumOfRowsGoogleTextBox"  |> Binding.twoWay((fun m -> m.NumOfRowsGoogleTextBoxText), (fun newVal m -> condition newVal m.NumOfRowsGoogleTextBoxText |> NumOfRowsGoogleTextBox))
                     
          "FirstRowIsHeadersCheckBox" |> Binding.twoWay((fun m -> m.FirstRowIsHeadersCheckBoxIsChecked), (fun newVal -> newVal |> FirstRowIsHeadersCheckBox))
          
          "JsonFileName1TextBox"   |> Binding.twoWay((fun m -> m.JsonFileName1TextBoxText), (fun newVal m -> cond (string newVal) m.JsonFileName1TextBoxText |> JsonFileName1TextBox))         
          "IdTextBox"              |> Binding.twoWay((fun m -> m.IdTextBoxText), (fun newVal m -> cond (string newVal) m.IdTextBoxText |> IdTextBox))           
          "SheetNameTextBox"       |> Binding.twoWay((fun m -> m.SheetNameTextBoxText), (fun newVal m -> cond (string newVal) m.SheetNameTextBoxText |> SheetNameTextBox))
          "SheetName6TextBox"      |> Binding.twoWay((fun m -> m.SheetName6TextBoxText), (fun newVal m -> cond (string newVal) m.SheetName6TextBoxText |> SheetName6TextBox))
          "ColumnStartTextBox"     |> Binding.twoWay((fun m -> m.ColumnStartTextBoxText), (fun newVal m -> condition newVal m.ColumnStartTextBoxText |> ColumnStartTextBox))
          "ColumnEndTextBox"       |> Binding.twoWay((fun m -> m.ColumnEndTextBoxText), (fun newVal m -> condition newVal m.ColumnEndTextBoxText |> ColumnEndTextBox))

          "TriggerEvent"           |> Binding.cmd InfoTextBoxForeground

          "InfoTextBox"            |> Binding.oneWay(fun m -> m.InfoTextBoxText)
          "InfoTextBoxForeground"  |> Binding.oneWay(fun m -> m.InfoTextBoxForeground)
          
          "FontTypeLabel"          |> Binding.oneWay(fun m -> m.FontTypeLabel) 
          "PrefixLabel"            |> Binding.oneWay(fun m -> m.PrefixLabel) 
          "ExampleStringLabel"     |> Binding.oneWay(fun m -> m.ExampleStringLabel)  
          "CsvPathLabel"           |> Binding.oneWay(fun m -> m.CsvPathLabel)
          "XlsxPathLabel"          |> Binding.oneWay(fun m -> m.XlsxPathLabel)
          "JpgPathLabel"           |> Binding.oneWay(fun m -> m.JpgPathLabel)
          "PdfPathLabel"           |> Binding.oneWay(fun m -> m.PdfPathLabel)
          "NumOfRowsGoogleLabel"   |> Binding.oneWay(fun m -> m.NumOfRowsGoogleLabel)
                              
          "FirstRowIsHeadersCheckBoxContent" |> Binding.oneWay(fun m -> m.FirstRowIsHeadersCheckBox)
                   
          "JsonFileName1Label"     |> Binding.oneWay(fun m -> m.JsonFileName1Label)
          "IdLabel"                |> Binding.oneWay(fun m -> m.IdLabel)                    
          "SheetNameLabel"         |> Binding.oneWay(fun m -> m.SheetNameLabel)
          "SheetName6Label"        |> Binding.oneWay(fun m -> m.SheetName6Label)
          "ColumnStartLabel"       |> Binding.oneWay(fun m -> m.ColumnStartLabel)
          "ColumnEndLabel"         |> Binding.oneWay(fun m -> m.ColumnEndLabel)
        ]      