namespace Auxiliaries

open System
open System.IO
open System.Data
open System.Linq

open Errors
open Strings
open ROP_Functions

module CreatingCSV =

    let writeIntoCSV (dt: DataTable) (pathCSV: string) (nameOfCVSFile: string) = //predpoklad, ze pathCSV a nameOfCVSFile jsou osetrene 
           
        let dt =
            dt
            |> Option.ofObj
            |> optionToGenerics2 "při čtení dat z DataTable" (new DataTable()) //whatever of the particular type                                          
   
        let csvPath =
            match string (pathCSV.Last()) with 
            | "\\" -> pathCSV.Remove(pathCSV.Length - 1, 1)
            | _    -> pathCSV

        let path = sprintf "%s\%s.csv" csvPath nameOfCVSFile

        //TODO try with
        use sw1 =
            new StreamWriter(Path.GetFullPath(path))
            |> Option.ofObj  
            |> optionToGenerics2 "při zápisu pomocí StreamWriter()" (new StreamWriter(String.Empty)) //whatever of the particular type  
             
        //headers  
        //tady do in scope nelze dt, bo shadowing zpusobi vzani hodnoty z parametru
        //TODO try with quli Seq.fold
        let strHeaders = 
            let columnNames = 
                dt.Columns.Cast<System.Data.DataColumn>() //TODO try with
                |> Option.ofObj 
                |> optionToGenerics2 "při použití dt.Columns.Cast" (dt.Columns.Cast<System.Data.DataColumn>()) //whatever of the particular type  
                |> Seq.map (fun item -> item.Caption)
            columnNames
            |> Seq.fold (fun acc item -> (+) acc (sprintf "%s%s" item ";")) String.Empty  //| nebo take > Seq.map (fun item -> sprintf "%s%s" item ";") |> Seq.fold (+) String.Empty
        
        do sw1.WriteLine(strHeaders.Remove(strHeaders.Length - 1, 1)) //odstraneni posledniho znaku, coz je ";"                      

        //rows
        [ 0 .. dt.Rows.Count - 1 ] //mohl by byt seq, array, atd.
        |> List.iter
            (fun r -> 
                    let str =  
                        [ 0 .. dt.Columns.Count - 1 ]
                        |> List.map 
                            (fun c -> 
                                    dt.Rows[r][c] |> Option.ofObj                                                             
                                    |> function
                                        | None when c = 0 -> (string dt.Rows.[r].[c])                                                                                        
                                        | _               -> (string dt.Rows.[r].[c]).Replace(';', ',')
                            )  
                    let str = sprintf "%s%s" (String.concat <| ";" <| str) ";" //TODO try with
                    do sw1.WriteLine(str.Remove(str.Length - 1, 1)) //odstraneni posledniho znaku, coz je ";"     
                    do sw1.Flush()  
            )    
     
        "Převod hodnot z Google tabulky do csv souboru se zdařil."