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

module Licences =

    open System
    open Elmish
    open Elmish.WPF

    type Model = 
        {
            LicencesTextBox0: string
            LicencesTextBox1: string //pro pripadne budouci pouziti
        }

    type Msg =
        | FixedText    

    let initialModel = 
       
        let str1 = "\r\n\nCode and/or software design in all files in the XElmish folder, file Converters.cs and file MainWindow.xaml.cs in the source code: MIT License, Copyright (c) 2021 Bent Tranberg, https://github.com/BentTranberg" 
        let str2 = "\r\n\nFile GoogleSheetsHelper.cs in the source code: Except where noted, code in this file was taken from this publicly available source: https://www.hardworkingnerd.com/how-to-read-and-write-to-google-sheets-with-c. The website is owned by Ian Preston. No licence conditions specified - check the website for possible updates." 
        let str3 = "\r\n\nFile CreateExcelFile.cs in the source code: MIT License, Copyright (c) 2016 Mike Gledhill, https://mikesknowledgebase.azurewebsites.net/pages/Home/index.htm"
        let str4 = "\r\n\nConsole progress bar in XLSX_To_PDF_JPG_forWPF.exe: MIT License, Copyright (c) 2017 Daniel Wolf, https://gist.github.com/DanielSWolf"
        let str5 = "\r\n\nPDF File Writer in XLSX_To_PDF_JPG_forWPF.exe: Code Project Open License (CPOL) 1.02, https://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version-2-0-0"
        let str6 = "\r\n\nSpire JPG Creator in XLSX_To_PDF_JPG_forWPF.exe: Free Spire.PDF for .NET is a Community Edition of the Spire.PDF for .NET, which is a totally free PDF API for commercial and personal use, see https://www.e-iceblue.com/Introduce/free-pdf-component.html#.YmMIflDP2Uk"
        let str7 = "\r\n\nXLSX_To_PDF_JPG_forWPF.exe (with the aforementioned exceptions): Copyright (c) 2021 Miroslav Husťák, http://hustak.somee.com" 
        let str8 = "\r\n\nAll code in Unique_Identifier_And_Metadata_File_Creator.exe (with the aforementioned exceptions): Copyright (c) 2022 Miroslav Husťák, http://hustak.somee.com" 
        
        {
            LicencesTextBox0 = sprintf"%s%s%s%s%s%s%s%s" str1 str2 str3 str4 str5 str6 str7 str8
            LicencesTextBox1 = sprintf"" //pro pripadne budouci pouziti               
        }

    let init(): Model * Cmd<'a> = initialModel, Cmd.none 

    let update (msg: Msg) (m: Model) : Model * Cmd<Msg> =
                   
               match msg with          
               | FixedText -> { m with LicencesTextBox0 = initialModel.LicencesTextBox0
                                       LicencesTextBox1 = initialModel.LicencesTextBox1 //pro pripadne budouci pouziti
                              }, Cmd.none    

    let bindings(): Binding<Model,Msg> list =
        [
          "licencesTextBox0"  |> Binding.oneWay(fun m -> m.LicencesTextBox0)
          "licencesTextBox1"  |> Binding.oneWay(fun m -> m.LicencesTextBox1) //pro pripadne budouci pouziti
        ]
  