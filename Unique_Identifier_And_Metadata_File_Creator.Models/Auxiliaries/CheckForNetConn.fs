namespace Auxiliaries

open System
open System.Net.NetworkInformation

module CheckForNetConn =  
        
    let checkForNetConn() = 

        try 
            try
                use myPing = new Ping()                          
                let host: String = "google.com"
                let buffer: byte[] = Array.zeroCreate <| 32
                let timeout = 1000
                let pingOptions: PingOptions = new PingOptions()                                       
                     
                myPing.Send(host, timeout, buffer, pingOptions)
                |> Option.ofObj  
                |> Option.bind (fun pingReply -> 
                                                match pingReply.Status = IPStatus.Success with
                                                | true  -> Some (pingReply |> ignore) 
                                                | false -> None                     
                               )
            finally
                ()
        with           
        | _ -> None       