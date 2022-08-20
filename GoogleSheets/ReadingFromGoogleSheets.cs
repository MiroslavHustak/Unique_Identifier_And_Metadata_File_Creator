using System;
using System.Data;
using System.Linq;
using System.Dynamic;
using GoogleSheetsHelper;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

/* Do csproj pridat toto (aby fungoval System.Windows.Forms):
  <PropertyGroup>
    <TargetFramework>net5.0-windows</TargetFramework>
	  <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
*/

namespace GoogleSheets
{
    public class ReadingFromGoogleSheets
    {
        //musi byt static kvuli F#
        public static DataTable ReadFromGoogleSheets(string jsonFileName, string id, string sheetName, int columnStart, int rowStart, int columnEnd, int rowEnd, bool firstRowIsHeaders)
        {
            try
            {
                List<string> myList = new List<string>();

                var gsh = new GoogleSheetsHelper.GoogleSheetsHelper(jsonFileName, id);

                List<ExpandoObject> generatedItems =
                    gsh.GetDataFromSheet(new GoogleSheetParameters() { SheetName = sheetName, RangeColumnStart = columnStart, RangeRowStart = rowStart, RangeColumnEnd = columnEnd, RangeRowEnd = rowEnd, FirstRowIsHeaders = firstRowIsHeaders });

                generatedItems.ForEach(item => item.ToList().ForEach(item1 => myList.Add(DealWithNull(item1))));

                //vystupni myList nebere prazdne radky !!!
                return CreateDataTable(myList, columnStart, rowStart, columnEnd, rowEnd);
            }
            catch (Exception ex)
            {
                string title = "Závažná chyba při čtení z Google tabulky";

                string message = $"Vyskytla se následující chyba: {ex.Message}. Klikni na \"OK\" pro restart této aplikace a oveř hodnoty pro Google tabulku v nastavení (json, id, názvy listů a hodnoty pro řádky a sloupce).";

                MessageBox.Show(message, title);

                string currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
                Process.Start(currentExecutablePath);
                Environment.Exit(1);

                return null;
            }
        }
    
        private static string DealWithNull(KeyValuePair<string, object> item) => (string.IsNullOrEmpty(item.Value.ToString())) ? string.Empty : item.Value.ToString();//nezda se, ze by to nejak pomohlo...

        private static DataTable CreateDataTable(List<string> myList, int columnStart, int rowStart, int columnEnd, int rowEnd)
        {
            // Create a new DataTable.
            DataTable dtGoogle = new DataTable("MyDataTable");
            DataColumn dtColumn;
            DataRow myDataRow;

            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string);
            dtColumn.ColumnName = "PracovniZnaceni";
            dtColumn.Caption = "Pracovní značení";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);

            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string);
            dtColumn.ColumnName = "DGsada";
            dtColumn.Caption = "Digitalizační sada";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);

            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string);
            dtColumn.ColumnName = "Archiv";
            dtColumn.Caption = "Archiv";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);

            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string); 
            dtColumn.ColumnName = "Fond";
            dtColumn.Caption = "Fond";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);

            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string);
            dtColumn.ColumnName = "NAD";
            dtColumn.Caption = "Číslo NAD";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);

            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string);
            dtColumn.ColumnName = "CisloPomucky";
            dtColumn.Caption = "Číslo pomůcky";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);

            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string); 
            dtColumn.ColumnName = "InventarniCislo";
            dtColumn.Caption = "Inventární číslo";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);

            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string);
            dtColumn.ColumnName = "Signatura";
            dtColumn.Caption = "Signatura";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);

            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string); 
            dtColumn.ColumnName = "CisloKartonu";
            dtColumn.Caption = "Číslo kartonu";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);

            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string);
            dtColumn.ColumnName = "UpresIdentifikator";
            dtColumn.Caption = "Upřesňující indentifikátor";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);

            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string);
            dtColumn.ColumnName = "Regest";
            dtColumn.Caption = "Regest";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);

            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string); 
            dtColumn.ColumnName = "DataceVzniku";
            dtColumn.Caption = "Datace vzniku";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);

            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string); 
            dtColumn.ColumnName = "Poznamka";
            dtColumn.Caption = "Poznámka";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);

            /*
            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(string);
            dtColumn.ColumnName = "Dummy";
            dtColumn.Caption = "Dummy";
            dtColumn.ReadOnly = false;
            dtColumn.Unique = false;
            dtGoogle.Columns.Add(dtColumn);
            */

            // Create a new DataSet
            //DataSet dtSet = new DataSet();

            // Add dtGoogle to the DataSet.
            //dtSet.Tables.Add(dtGoogle); //jen z vyukovych duvodu (DataSet neco jako DB, DataTable neco jako db table) //stejne jsem to musel vycommentovat, bo to koliduje s DataSet v GoogleSheetHelper a kopii se mi nechtelo delat

            int limit = Math.Abs(rowEnd - rowStart + 1) * columnEnd; //columnEnd musi byt 14 nebo mene vzhledem k konstantnimu poctu columns v DataTable

            if (myList.Count != limit) //vystupny myList nebere prazdne radky !!!
            {
                myDataRow = dtGoogle.NewRow();
                myDataRow["PracovniZnaceni"] = "error";  
                dtGoogle.Rows.Add(myDataRow);
            }
            else
            {
                for (int i = 0; i < limit; i = i + columnEnd)
                {
                    myDataRow = dtGoogle.NewRow();
                    myDataRow["PracovniZnaceni"] = myList[0 + i];
                    myDataRow["DGsada"] = myList[1 + i];
                    myDataRow["Archiv"] = myList[2 + i];
                    myDataRow["Fond"] = myList[3 + i];
                    myDataRow["NAD"] = myList[4 + i];
                    myDataRow["CisloPomucky"] = myList[5 + i];
                    myDataRow["InventarniCislo"] = myList[6 + i];
                    myDataRow["Signatura"] = myList[7 + i];
                    myDataRow["CisloKartonu"] = myList[8 + i];
                    myDataRow["UpresIdentifikator"] = myList[9 + i];
                    myDataRow["Regest"] = myList[10 + i];
                    myDataRow["DataceVzniku"] = myList[11 + i];
                    myDataRow["Poznamka"] = myList[12 + i];
                    //myDataRow["Dummy"] = myList[13 + i];

                    dtGoogle.Rows.Add(myDataRow);
                }
            }
            
            return dtGoogle;
        }

    }
}
