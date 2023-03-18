namespace Auxiliaries

open System
open System.IO;
open Newtonsoft.Json
open System.Diagnostics
open System.Runtime.Serialization
open System.Net.NetworkInformation

open Errors
open ROP_Functions

module Helpers = 

    module NetConn = 
       
        let checkForNetConn() = 

            try 
                try
                    use myPing = new Ping()                          
                    let host: String = "google.com"
                    let buffer: byte[] = Array.zeroCreate <| 32
                    let timeout = 1000
                    let pingOptions: PingOptions = new PingOptions() // |> Option.ofObj                                                        
                    //asi nevadi, ze parametr pingOptions by mohl byt null, kdyz Option.ofObj nefunguje 
                    
                    myPing.Send(host, timeout, buffer, pingOptions)
                    |> Option.ofObj  
                    |> function 
                        | Some value -> value.Status = IPStatus.Success
                        | None       -> false
                    //myPing.Dispose()        
                finally
                    ()
            with           
            | _ -> false      
    
    
    module Process =
         
        //ROP... 
        let detectLockedFile (file: FileInfo) =       
            
                let perform x =  
                   //zamerne nepouzivam use stream =  
                   let stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None) 
                   stream.Close()
                   stream.Dispose()
                   false             
                tryWith perform (fun x -> ()) (fun ex -> ()) |> deconstructor2 
         
        //... a pro porovnani normalni try with block (ale bez finaly, coz mam vyse)
        let detectLockedFileTryWith (file: FileInfo) = 
            
            try      
                //zamerne nepouzivam use stream = 
                let stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None) 
                stream.Close()
                stream.Dispose()
                false                                                      
            with  
            | ex -> true  
              
        let detectFileRunning processName =     
            
            let perform x =                                    
                let getProcesses = Process.GetProcessesByName(processName)
                                   |> Option.ofObj                                
                                   |> optionToGenerics processName "GetProcessesByName()"   
                getProcesses.Length > 0   
            tryWith perform (fun x -> ()) (fun ex -> ()) |> deconstructor2 

        let isFileLocked path =    
            
                let title = "Závažná chyba při přístupu k Excel souboru"
                let message ex = sprintf "Vyskytla se následující chyba: %s. Klikni na \"OK\" pro restart této aplikace a vypni v Excelu soubor %s." ex path
                                                                      
                let perform x =                                    
                    let file = new FileInfo(path)
                               |> Option.ofObj                                
                               |> optionToGenerics path "FileInfo(path)"   
                    match detectLockedFile file with
                    | true   -> failwith (sprintf "Soubor %A je již používán jiným procesem." path)
                                true // tohle true nevezme, proto viz nize                                                               
                    | false -> false                      
                tryWith perform (fun x -> ()) (fun ex -> ()) |> deconstructor3 title message true   
                        

        let deleteAllFilesInDir path =    //!!! AVG blokuje mazani souboru, nutno zadat vyjimku z blokace    

            let title = "Závažná chyba při mazání adresáře"
            let message ex = sprintf "Vyskytla se následující chyba při mazání adresáře: %s. Klikni na \"OK\" pro restart této aplikace a oveř adresář %s." ex path
                                                                       
            let perform x =                                    
                let dirInfo = new DirectoryInfo(path)
                              |> Option.ofObj                                
                              |> optionToGenerics path "DirectoryInfo(path)"   
                let fiArray = dirInfo.GetFiles("*", SearchOption.AllDirectories)
                              |> Option.ofObj                                
                              |> optionToGenerics path "GetFiles()"   
                
                fiArray |> Array.map(fun item -> 
                                                match detectLockedFile item with
                                                | false -> item.Delete()
                                                           false                                                                   
                                                | true  -> failwith (sprintf "Soubor %A je používán jiným procesem a nelze jej smazat." item)
                                                           true  //k tomuto true to nedojede, proto [| true; true |]                                                                                                                                
                                    ) 
                
            tryWith perform (fun x -> ()) (fun ex -> ()) |> deconstructor3 title message [| true; true |] |> Array.contains true   

        let closeSingleProcess message title processName =        

            Seq.initInfinite (fun _ -> let getProcesses = Process.GetProcessesByName(processName)
                                                          |> Option.ofObj                                
                                                          |> optionToGenerics processName "GetProcessesByName()"   
                                       getProcesses.Length > 0
                              ) 
            |> Seq.takeWhile ((=) true) 
            |> Seq.iter      (fun _ -> error1  <| message <| title)  
        
        
        [<CompiledName "KillSingleProcess">] 
        let killSingleProcess(name: string, errorNumber: string, consoleApp: bool): unit = 

           try          
              let iterateThroughProcess =
                  let getProcesses = Process.GetProcessesByName(name)
                                    |> Option.ofObj                                
                                    |> optionToGenerics name "GetProcessesByName()"   
                  getProcesses 
                  |> Array.toList 
                  |> List.map (fun item -> 
                                          match (item.ProcessName |> String.IsNullOrEmpty) with
                                          | true  -> ()
                                          | false -> item.Kill() 
                              )
              ()                                            
           with  
           | ex when (consoleApp = true)  -> do printfn "%s: %s" <| errorNumber <| string ex.Message
                                             let result = Console.ReadKey()
                                             () 
           | ex when (consoleApp = false) -> error4 <| string ex.Message
           | _  when (consoleApp = false) -> ()

    module private TryParserInt =

         let tryParseWith (tryParseFunc: string -> bool * _) = tryParseFunc >> function
             | true, value -> Some value
             | false, _    -> None
         let parseInt    = tryParseWith <| System.Int32.TryParse  
         let (|Int|_|)   = parseInt        

    module Parsing =

         let f x = let isANumber = x                                          
                   isANumber   
                   
         let rec parseMe = 
             function            
             | TryParserInt.Int i -> f i
             | _                  -> 0  

         let rec parseMeOption = 
             function            
             | TryParserInt.Int i -> f Some i
             | _                  -> None   
    
    module MyString = 

        //priklad pouziti: GetString(8, "0")//tuple a nazev velkym kvuli DLL pro C#
        let GetString (numberOfStrings: int, stringToAdd: string): string =   

            let initialString = String.Empty                //initial value of the string
            let listRange = [ 1 .. numberOfStrings ]
            let rec loop list acc stringToAdd =
                match list with 
                | []        -> acc
                | _ :: tail -> let finalString = (+) acc stringToAdd
                               loop <| tail <| finalString <| stringToAdd
            loop <| listRange <| initialString <| stringToAdd  
    
    //Vsechny serializace dat povinne do trywith bloku (a jeste overit Option.ofObj, kdyby nahodou tam byl nullable typ)
    module Serialisation = 
         
         let serialize record xmlFile = 
            
             let filepath = Path.GetFullPath(xmlFile) 
                            |> Option.ofObj 
                            |> optionToGenerics "čtení cesty k souboru json.xml" "Path.GetFullPath()"

             let xmlSerializer = new DataContractSerializer(typedefof<string>)          
                                 |> Option.ofObj 
                                 |> optionToGenerics "při tvorbě nové instance" "DataContractSerializer()"
             use stream = File.Create(filepath)   
             xmlSerializer.WriteObject(stream, JsonConvert.SerializeObject(record))            

    //Vsechny deserializace dat povinne do trywith bloku (a jeste overit Option.ofObj, kdyby nahodou tam byl nullable typ)          
    module Deserialisation =       
              
       let deserialize xmlFile = 
           
           let filepath = Path.GetFullPath(xmlFile) 
                          |> Option.ofObj 
                          |> optionToGenerics (sprintf "čtení cesty k souboru souboru %s" xmlFile) "Path.GetFullPath()"
                          //za timto je trywith, kere by asi zrobilo NullReference Exception, tra se rozhodnut, esli chybu resit jako tady vypnutim, anebo ji nechat projit s nactenim defaultnich hodnot  
          
           let jsonString() = 

               let xmlSerializer = new DataContractSerializer(typedefof<string>) 
                                   |> Option.ofObj 
                                   |> optionToGenerics "při tvorbě nové instance" "DataContractSerializer()"
               let fileStream = File.ReadAllBytes(filepath)
                                |> Option.ofObj 
                                |> optionToGenerics (sprintf "čtení dat ze souboru %s" xmlFile) "File.ReadAllBytes()"
               use memoryStream = new MemoryStream(fileStream) 
               let resultObj = xmlSerializer.ReadObject(memoryStream)  
                               |> Option.ofObj 
                               |> optionToGenerics (sprintf "čtení dat ze souboru %s" xmlFile) "xmlSerializer.ReadObject()"      
               let resultString = unbox resultObj  
                                  |> Option.ofObj 
                                  |> optionToGenerics "downcasting" "(unbox resultObj)"      
               let jsonString = JsonConvert.DeserializeObject<'a>(resultString) 
               jsonString
           
           let fInfodat: FileInfo = new FileInfo(filepath)  
           match fInfodat.Exists with 
           | true  -> jsonString()              
           | false -> failwith (sprintf "Soubor %s nenalezen" xmlFile) //Common_Settings.Default //DG_Settings.Default

    module CopyingFiles =     
        
       let copyFiles source destination =
                                                                    
          let perform x =                                    
              let sourceFilepath = Path.GetFullPath(source) 
                                   |> Option.ofObj 
                                   |> optionToGenerics (sprintf "čtení cesty k souboru %s" source)  "Path.GetFullPath()"
              let destinFilepath = Path.GetFullPath(destination) 
                                   |> Option.ofObj 
                                   |> optionToGenerics (sprintf "čtení cesty k souboru %s" destination)  "Path.GetFullPath()"
                    
              let fInfodat: FileInfo = new FileInfo(sourceFilepath)  
              match fInfodat.Exists with 
              | true  -> File.Copy(sourceFilepath, destinFilepath, true)             
              | false -> failwith (sprintf "Soubor %s nenalezen" source)
          tryWith perform (fun x -> ()) (fun ex -> ()) |> deconstructor4 ()
                      
    (*       
    System.IO.File provides static members related to working with files, whereas System.IO.FileInfo represents a specific file and contains non-static members for working with that file.          
    Because all File methods are static, it might be more efficient to use a File method rather than a corresponding FileInfo instance method if you want to perform only one action. All File methods 
    require the path to the file that you are manipulating.    
    The static methods of the File class perform security checks on all methods. If you are going to reuse an object several times, consider using the corresponding 
    instance method of FileInfo instead, because the security check will not always be necessary.    
    *) 
        
    
        
       
          

