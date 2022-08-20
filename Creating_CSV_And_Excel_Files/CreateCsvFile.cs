using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;

/* Do csproj pridat toto (aby fungoval System.Windows.Forms):
  <PropertyGroup>
    <TargetFramework>net5.0-windows</TargetFramework>
	  <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
*/

namespace Creating_CSV_And_Excel_Files
{
    public class CreateCsvFile
    {
        public static string WriteIntoCSV(System.Data.DataTable dt, string pathCSV, string nameOfCVSFile)
        {
            try
            {
                string[] columnNames = dt.Columns.Cast<System.Data.DataColumn>().Select(x => x.Caption).ToArray();
                              
                string csvPath = pathCSV.Last().Equals("\\") ? pathCSV.Remove(pathCSV.Length - 1, 1) : pathCSV;

                string path = $@"{csvPath}\{nameOfCVSFile}.csv";               

                using (StreamWriter sw1 = new StreamWriter(Path.GetFullPath(path)))
                {
                    string join1 = String.Empty;

                    for (int c = 0; c < columnNames.Length; c++)
                    {
                        join1 += columnNames[c] + ";";
                    }

                    sw1.WriteLine(join1.Remove(join1.Length - 1, 1));//odstraneni posledniho znaku, coz je ";"

                    int dtXlsxRowsCount = dt.Rows.Count;
                    int dtXlsxColumnsCount = dt.Columns.Count;

                    for (int r = 0; r < dtXlsxRowsCount; r++)
                    {
                        string join = String.Empty;

                        for (int c = 0; c < dtXlsxColumnsCount; c++)
                        {
                            if (dt.Rows[r][c] == null && c == 0)
                            {
                                join += dt.Rows[r][c];
                            }
                            else
                            {
                                join += dt.Rows[r][c].ToString().Replace(';', ',') + ";";
                            }
                        }

                        sw1.WriteLine(join.Remove(join.Length - 1, 1));//odstraneni posledniho znaku, coz je ";"

                        sw1.Flush();
                    }
                }
                return "Převod hodnot z Google tabulky do csv souboru se zdařil."; 
            }
            catch (Exception ex)
            {
                string title = "Závažná chyba při převodu hodnot z Google tabulky do csv souboru";

                string message = $"Vyskytla se následující chyba: {ex.Message}. Klikni na \"OK\" pro restart této aplikace a oveř hodnoty pro csv soubor v nastavení.";

                MessageBox.Show(message, title);

                string currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
                Process.Start(currentExecutablePath);
                Environment.Exit(1);

                return null; 
            }
        }
    }

}
