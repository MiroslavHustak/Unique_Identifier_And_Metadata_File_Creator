namespace Settings
    
[<Struct>]  //vhodne pro 16 bytes => 4096 characters
type Common_Settings = 
    {        
        //***************************** settings
        fontType: string
        prefix: string
        exampleString: string
        csvPath: string
        xlsxPath: string
        jpgPath: string
        pdfPath: string
        numOfRowsGoogle: int   
        firstRowIsHeaders: bool
        jsonFileName1: string
        id: string
        sheetName: string 
        sheetName6: string 
        columnStart: int //pracovni znaceni      
        columnEnd: int //poznamka    
    }
    static member Default = 
        {
            fontType = "T"
            prefix = "LT-" 
            exampleString = "LT-01402"
            csvPath = @"e:\E\Mirek po osme hodine a o vikendech\Pruvodky Litomerice\CSV Digitalizacni sada\"
            xlsxPath = @"e:\E\Mirek po osme hodine a o vikendech\Pruvodky Litomerice\XLSXnew Digitalizacni sada\"
            jpgPath = @"e:\E\Mirek po osme hodine a o vikendech\Pruvodky Litomerice\JPG Digitalizacni sada\"
            pdfPath = @"e:\E\Mirek po osme hodine a o vikendech\Pruvodky Litomerice\PDF Digitalizacni sada\"
            numOfRowsGoogle = 0//6999          
            firstRowIsHeaders = false
            jsonFileName1 = @"c:\Users\User\source\repos\Unique_Identifier_And_Metadata_File_Creator\JSON\json1\ampacolitomerice-bca0f962c6f9.json" //u tasks nesmi byt stejny json pro excel a csv
            id = @"1n429ukClsoDxiCzRrWcX9PeWegKARby0PeMSe9cR51o" // je to soucast URL Google tabulky // Ampaco Google Sheet
            sheetName = "Vysledek" 
            sheetName6 = "Kontrolni list" 
            columnStart = 1         
            columnEnd = 13          
        }

type DG_Sada = 
    {        
        errorDG: string[]  
        msg1: string
        msg2: string
        msg3: string
    }
    static member Default = 
        {
            errorDG = [||]
            msg1 = ""
            msg2 = ""
            msg3 = ""
        }
        
