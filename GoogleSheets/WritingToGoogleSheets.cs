using System;
using System.Linq;
using System.Diagnostics;
using GoogleSheetsHelper;
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
    public class WritingToGoogleSheets
    {
        private readonly System.Data.DataTable _dt;

        public WritingToGoogleSheets(System.Data.DataTable dt)
        {
            _dt = dt;
        }

        public void WriteToGoogleSheets(string jsonFileName1, string id, string sheetName6, int endIndex)
        {
            try
            {
                var gsh = new GoogleSheetsHelper.GoogleSheetsHelper(jsonFileName1, id);

                List<GoogleSheetCell> MyCells = new List<GoogleSheetCell>();
                List<GoogleSheetRow> MyRows = new List<GoogleSheetRow>();

                int numberOfRows = _dt.Rows.Count; int numberOfColumns = _dt.Columns.Count;

                for (int i = -1; i < numberOfRows; i++)
                {
                    if (i == -1)
                    {
                        Enumerable.Range(0, numberOfColumns).ToList().ForEach(j => MyCells.Add(new GoogleSheetCell() { CellValue = _dt.Columns[j].Caption.ToString(), IsBold = true }));
                        AddRows();
                    }
                    else
                    {
                        Enumerable.Range(0, numberOfColumns).ToList().ForEach(j => MyCells.Add(new GoogleSheetCell() { CellValue = _dt.Rows[i][j].ToString() }));
                        AddRows();
                    }
                }

                gsh.AddCells(new GoogleSheetParameters() { SheetName = sheetName6, RangeColumnStart = 1, RangeRowStart = 1 }, MyRows, endIndex);//nechame to umistit natvrdo na zacatek

                void AddRows()
                {                    
                    GoogleSheetRow gsr = new GoogleSheetRow();
                    gsr.Cells.AddRange(MyCells);
                    MyRows.Add(gsr);
                    MyCells.Clear();
                }
            }
            catch (Exception ex)
            {
                string title = "Závažná chyba při zápisu do Google tabulky";

                string message = $"Vyskytla se následující chyba: {ex.Message}\n\n Klikni na \"OK\" pro restart této aplikace a podívej se, jestli nemáš k prasknutí narvanou Google tabulku nebo oveř hodnoty pro Google tabulku v nastavení (json, id, názvy listů a hodnoty pro řádky a sloupce).";

                MessageBox.Show(message, title);

                string currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
                Process.Start(currentExecutablePath);
                Environment.Exit(1);
            }
        }
    }
}
