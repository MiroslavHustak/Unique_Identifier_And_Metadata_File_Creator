namespace Auxiliaries

open System
open System.Windows
open System.Diagnostics

(*
open System.Windows vyzaduje doplneni nize uvedeneho do fsproj
<TargetFramework>net5.0-windows</TargetFramework>
<UseWPF>true</UseWPF>
*)

module Errors =    

    //************************** auxiliary function *******************

    let private restartApp title message f = 
    
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning)
        |> function  
           | MessageBoxResult.OK ->  
                                     let currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName
                                     Process.Start(currentExecutablePath) |> ignore //restart
                                     Environment.Exit(1)  
                                     f
           | _                    -> f

    //************************** main error functions ******************

    let error0 ex =

        let message = 
            sprintf "Vyskytla se chyba v nastavení. Oprav hodnoty v nastavení a klikni na tlačítko \"OK\" pro restart programu. 
                     K tomu dopomáhej ti následující chybové hlášení: \n\n%s" <| ex
        restartApp "Závažná chyba v nastavení" message List.empty

    let error1 message title = 
        
        MessageBox.Show (message, title, MessageBoxButton.OK, MessageBoxImage.Warning)   
        |> function   
           | MessageBoxResult.OK -> ()                    
           | _                   -> ()

    let error3 str1 str2 = 

        let message =
            sprintf "Vyskytla se chyba při čtení %s metodou %s, která není způsobena tímto programem. 
                     Oprav I/O problém a klikni na \"OK\" pro restart programu." <| str1 <| str2                      
        restartApp "Závažná chyba" message Array.empty                        

    let error4 str = 

        let message = 
            sprintf "Vyskytla se chyba (%s). Klikni na \"OK\" pro ukončení této aplikace a sežeň programátora, anebo oprav problém sám." <| str
        let title = "Závažná chyba"      
    
        MessageBox.Show (message, title, MessageBoxButton.OK, MessageBoxImage.Warning)   
        |> function  
           | MessageBoxResult.OK -> Environment.Exit(1)                     
           | _                   -> ()

    let error5 ex  = 

        let message = 
            sprintf "Vyskytla se chyba při načítání nastavených hodnot. Odstraň problém a klikni na tlačítko \"OK\" pro restart 
                     programu. K tomu dopomáhej ti následující chybové hlášení: \n\n%s" <| ex
        restartApp "Závažná chyba" message ()

    let error6 title message = restartApp title message ()


