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

open Types
open Auxiliaries

open PatternBuilders

//Non-optional variant
module MainWindowNonOpt =

    open System
    open Serilog
    open System.Windows.Media

    open Elmish
    open Elmish.WPF

    let private header1 = " Hlavní stránka "
    let private header2 = " Nastavení pro DG sadu " 
    let private header3 = " Ostatní nastavení "
    let private header4 = " Licence "

    let newGuid () = Guid.NewGuid()

    type Toolbutton =
        {
            Id: Guid
            Text: string
            IsMarkable: bool
        }

    type Tab =
        {
            Id: Guid
            Header: string
            Toolbuttons: Toolbutton list
        }

    type Msg =
        | ButtonClick of id: Guid
        | ShowRightCalc
        | ShowSettingsDG
        | ShowSettings
        | ShowLicences
        | RightCalcMsg of RightCalc.Msg
        | SettingsDGMsg of XElmishSettingsDG.Msg
        | SettingsMsg of XElmishSettings.Msg
        | LicencesMsg of Licences.Msg
        | SetSelectedTabHeader of tabHeader:string

    type Model =
        {
            Tabs: Tab list
            MarkedButton: Guid
            RightCalcPage: RightCalc.Model 
            SettingsPageDG: XElmishSettingsDG.Model 
            SettingsPage: XElmishSettings.Model 
            LicencesPage: Licences.Model 
            SelectedTabHeader: string
        }
           
    let tbNone = newGuid ()
    let tbRightCalc = newGuid ()
    let tbSettingsDG = newGuid ()
    let tbSettings = newGuid ()
    let tbLicences = newGuid ()  

    let tabs =
        let tab header toolButtons =
            { Id = newGuid (); Header = header; Toolbuttons = toolButtons }           
        [ tab header1 []; tab header2 []; tab header3 []; tab header4 []]
    
    let rightCalcPage, (rightCalcPageCmd: Cmd<RightCalc.Msg>) = RightCalc.init ()
    let settingsPageDG, (settingsPageDGCmd: Cmd<XElmishSettingsDG.Msg>) = XElmishSettingsDG.init ()
    let settingsPage, (settingsPageCmd: Cmd<XElmishSettings.Msg>) = XElmishSettings.init ()
    let licencesPage, (licencesPageCmd: Cmd<Licences.Msg>) = Licences.init () 
    
    let startModel =
        {
            Tabs = tabs
            MarkedButton = tbRightCalc           
            RightCalcPage = rightCalcPage               
            SettingsPageDG = settingsPageDG
            SettingsPage = settingsPage
            LicencesPage = licencesPage
            SelectedTabHeader = (tabs |> List.item 0).Header
        }

    let init () : Model * Cmd<Msg> = startModel, Cmd.map RightCalcMsg rightCalcPageCmd

    let findButton (id: Guid) (m: Model) =
        m.Tabs |> List.tryPick (fun tab -> tab.Toolbuttons |> List.tryFind (fun tb -> tb.Id = id))

    let update (msg: Msg) (m: Model) = 
        match msg with
        | ButtonClick id ->
            match findButton id m with
            | None               -> 
                                  m, Cmd.none
            | Some clickedButton ->                                   
                                  let m = 
                                      match clickedButton.IsMarkable with
                                      | true  -> { m with MarkedButton = id }
                                      | false -> m                            

                                  MyPatternBuilder    
                                      {                                               
                                          let! _ = not (clickedButton.Id = tbRightCalc), (m, Cmd.ofMsg ShowRightCalc) 
                                          let! _ = not (clickedButton.Id = tbSettingsDG), (m, Cmd.ofMsg ShowSettingsDG)  
                                          let! _ = not (clickedButton.Id = tbSettings), (m, Cmd.ofMsg ShowSettings)  
                                          let! _ = not (clickedButton.Id = tbLicences), (m, Cmd.ofMsg ShowLicences)  
                                          return m, Cmd.none 
                                      }       

        | ShowRightCalc  -> { m with RightCalcPage = fst (RightCalc.init()) }, Cmd.none   
        | ShowSettingsDG -> { m with SettingsPageDG = fst (XElmishSettingsDG.init()) }, Cmd.none 
        | ShowSettings   -> { m with SettingsPage = fst (XElmishSettings.init()) }, Cmd.none 
        | ShowLicences   -> { m with LicencesPage = fst (Licences.init ()) }, Cmd.none          
    
        | RightCalcMsg msg' ->
                             let m', cmd' = RightCalc.update msg' m.RightCalcPage
                             { m with RightCalcPage = m' }, Cmd.map RightCalcMsg cmd'        
        | SettingsDGMsg msg'->
                             let m', cmd' = XElmishSettingsDG.update msg' m.SettingsPageDG
                             { m with SettingsPageDG = m' }, Cmd.map SettingsDGMsg cmd'
        | SettingsMsg msg'  ->
                             let m', cmd' = XElmishSettings.update msg' m.SettingsPage
                             { m with SettingsPage = m' }, Cmd.map SettingsMsg cmd'
        | LicencesMsg msg'  ->
                             let m', cmd' = Licences.update msg' m.LicencesPage
                             { m with LicencesPage = m' }, Cmd.map LicencesMsg cmd'

        | SetSelectedTabHeader header ->           
            match header with
            | value when header1 = header -> { m with MarkedButton = tbRightCalc; SelectedTabHeader = value }, Cmd.ofMsg ShowRightCalc 
            | value when header2 = header -> { m with MarkedButton = tbSettingsDG; SelectedTabHeader = value }, Cmd.ofMsg ShowSettingsDG 
            | value when header3 = header -> { m with MarkedButton = tbSettings; SelectedTabHeader = value }, Cmd.ofMsg ShowSettings 
            | value when header4 = header -> { m with MarkedButton = tbLicences; SelectedTabHeader = value }, Cmd.ofMsg ShowLicences 
            | _                           -> { m with SelectedTabHeader = header }, Cmd.none
        
    let bindings(): Binding<Model,Msg> list =
        [
            "Tabs" |> Binding.subModelSeq((fun m -> m.Tabs), (fun t -> t), fun () ->
                [
                    "Id" |> Binding.oneWay (fun (_, t) -> t.Id)
                    "Header" |> Binding.oneWay (fun (_, t) -> t.Header)
                ])     
          
            "RightCalcPage"
            |> Binding.SubModel.required RightCalc.bindings
            |> Binding.mapModel (fun m -> m.RightCalcPage)
            |> Binding.mapMsg RightCalcMsg
            "SettingsPageDG"
            |> Binding.SubModel.required XElmishSettingsDG.bindings 
            |> Binding.mapModel (fun m -> m.SettingsPageDG)
            |> Binding.mapMsg SettingsDGMsg
            "SettingsPage"
            |> Binding.SubModel.required XElmishSettings.bindings 
            |> Binding.mapModel (fun m -> m.SettingsPage)
            |> Binding.mapMsg SettingsMsg
            "LicencesPage"
            |> Binding.SubModel.required Licences.bindings 
            |> Binding.mapModel (fun m -> m.LicencesPage)
            |> Binding.mapMsg LicencesMsg
   
            "RightCalcPageVisible"  |> Binding.oneWay (fun m -> m.MarkedButton = tbRightCalc)           
            "SettingsPageDGVisible" |> Binding.oneWay (fun m -> m.MarkedButton = tbSettingsDG)
            "SettingsPageVisible"   |> Binding.oneWay (fun m -> m.MarkedButton = tbSettings)
            "LicencesPageVisible"   |> Binding.oneWay (fun m -> m.MarkedButton = tbLicences)   
            "SelectedTabHeader"     |> Binding.twoWay ((fun m -> m.SelectedTabHeader), SetSelectedTabHeader)
        ]    
    
    let designVm = ViewModel.designInstance startModel (bindings())
