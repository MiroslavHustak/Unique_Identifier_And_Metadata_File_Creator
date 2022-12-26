namespace Settings

[<Struct>]  //vhodne pro 16 bytes => 4096 characters
type DG_Settings = 
    {      
        archiveCodeTxb: string
        archiveCodeCkbx: bool
        nadTxb: int
        nadCkbx: bool
        pomTxb: int
        pomCkbx: bool
        invTxb1: string
        invTxb2: int
        invCkbxLeft: bool
        invCkbxLeftE: bool
        invCkbxRight: bool
        sgTxb1: string
        sgTxb2: string
        sgTxb3: string
        sgCkbx: bool
        sgCkbxE: bool
        karTxb1: string
        karTxb2: int
        karCkbxLeft: bool 
        karCkbxLeftE: bool 
        karCkbxRight: bool
    }
    static member Default = 
        {
            archiveCodeTxb = "214000010"
            archiveCodeCkbx = true
            nadTxb = 4
            nadCkbx = true
            pomTxb = 4
            pomCkbx = true
            invTxb1 = "ic"
            invTxb2 = 4
            invCkbxLeft = false
            invCkbxLeftE = false
            invCkbxRight = true
            sgTxb1 = "sg"
            sgTxb2 = @"/"
            sgTxb3 = "_19"              
            sgCkbx = true
            sgCkbxE = true
            karTxb1 = "kar"
            karTxb2 = 4
            karCkbxLeft = false  
            karCkbxLeftE = false  
            karCkbxRight = true  
        }
    
