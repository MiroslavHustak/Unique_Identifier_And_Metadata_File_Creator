﻿using System;
using System.IO;
using System.Text;
using System.Data;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;


namespace Creating_CSV_And_Excel_Files
{
    //
    //   September 2016
    //   https://mikesknowledgebase.azurewebsites.net/pages/Home/index.htm
    //
    //   (c) Mike Gledhill 2016 
    //   
    //   Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
    //   (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
    //   publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
    //   subject to the following conditions:
    //   
    //   The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. 
    //   
    public static class CreateExcelFile
    {
        public static bool CreateExcelDocument<T>(List<T> list, string xlsxFilePath)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(ListToDataTable(list));

            return CreateExcelDocument(ds, xlsxFilePath);
        }

        public static DataTable ListToDataTable<T>(List<T> list)
        {
            DataTable dt = new DataTable();

            foreach (PropertyInfo info in typeof(T).GetProperties())
            {
                dt.Columns.Add(new DataColumn(info.Name, GetNullableType(info.PropertyType)));
            }
            foreach (T t in list)
            {
                DataRow row = dt.NewRow();
                foreach (PropertyInfo info in typeof(T).GetProperties())
                {
                    if (!IsNullableType(info.PropertyType))
                        row[info.Name] = info.GetValue(t, null);
                    else
                        row[info.Name] = (info.GetValue(t, null) ?? DBNull.Value);
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        private static Type GetNullableType(Type t)
        {
            Type returnType = t;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                returnType = Nullable.GetUnderlyingType(t);
            }
            return returnType;
        }

        private static bool IsNullableType(Type type)
        {
            return (type == typeof(string) ||
                    type.IsArray ||
                    (type.IsGenericType &&
                     type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))));
        }

        //*************************************************************************************
        //Tuto metodu volam z MainLogicDG
        public static bool CreateExcelDocument(DataTable dt, string xlsxFilePath)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            bool result = CreateExcelDocument(ds, xlsxFilePath);
            ds.Tables.Remove(dt);
            return result;
        }
        //*************************************************************************************

        /// <summary>
        /// Create an Excel file, and write it to a file.
        /// </summary>
        /// <param name="ds">DataSet containing the data to be written to the Excel.</param>
        /// <param name="excelFilename">Name of file to be written.</param>
        /// <returns>True if successful, false if something went wrong.</returns>
        public static bool CreateExcelDocument(DataSet ds, string excelFilename)
        {
            try
            {
                using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(excelFilename, SpreadsheetDocumentType.Workbook))
                {
                    WriteExcelFile(ds, spreadsheet);
                }
                Trace.WriteLine("Successfully created: " + excelFilename);
                return true;
            }
            catch (Exception ex)
            {
                string title = "Závažná chyba při převodu hodnot z Google tabulky do Excelu";

                string message = $"Vyskytla se následující chyba: {ex.Message}. Klikni na \"OK\" pro restart této aplikace a oveř hodnoty pro Excel v nastavení.";

                MessageBox.Show(message, title);

                string currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
                Process.Start(currentExecutablePath);
                Environment.Exit(1);

                Trace.WriteLine("Failed, exception thrown: " + ex.Message);
                return false;
            }
        }

        private static void WriteExcelFile(DataSet ds, SpreadsheetDocument spreadsheet)
        {
            //  Create the Excel file contents.  This function is used when creating an Excel file either writing 
            //  to a file, or writing to a MemoryStream.
            spreadsheet.AddWorkbookPart();
            spreadsheet.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

            //  My thanks to James Miera for the following line of code (which prevents crashes in Excel 2010)
            spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

            //  If we don't add a "WorkbookStylesPart", OLEDB will refuse to connect to this .xlsx file !
            WorkbookStylesPart workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
            Stylesheet stylesheet = new Stylesheet();
            workbookStylesPart.Stylesheet = stylesheet;

            //  Loop through each of the DataTables in our DataSet, and create a new Excel Worksheet for each.
            uint worksheetNumber = 1;
            Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
            foreach (DataTable dt in ds.Tables)
            {
                //  For each worksheet you want to create
                string worksheetName = dt.TableName;

                //  Create worksheet part, and add it to the sheets collection in workbook
                WorksheetPart newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
                Sheet sheet = new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart), SheetId = worksheetNumber, Name = worksheetName };

                // If you want to define the Column Widths for a Worksheet, you need to do this *before* appending the SheetData
                // http://social.msdn.microsoft.com/Forums/en-US/oxmlsdk/thread/1d93eca8-2949-4d12-8dd9-15cc24128b10/

                sheets.Append(sheet);

                //  Append this worksheet's data to our Workbook, using OpenXmlWriter, to prevent memory problems
                WriteDataTableToExcelWorksheet(dt, newWorksheetPart);

                worksheetNumber++;
            }

            spreadsheet.WorkbookPart.Workbook.Save();
        }

        private static void WriteDataTableToExcelWorksheet(DataTable dt, WorksheetPart worksheetPart)
        {
            OpenXmlWriter writer = OpenXmlWriter.Create(worksheetPart, Encoding.ASCII);
            writer.WriteStartElement(new Worksheet());
            writer.WriteStartElement(new SheetData());

            string cellValue = "";
            string cellReference = "";

            //  Create a Header Row in our Excel file, containing one header for each Column of data in our DataTable.
            //
            //  We'll also create an array, showing which type each column of data is (Text or Numeric), so when we come to write the actual
            //  cells of data, we'll know if to write Text values or Numeric cell values.
            int numberOfColumns = dt.Columns.Count;
            bool[] IsIntegerColumn = new bool[numberOfColumns];
            bool[] IsFloatColumn = new bool[numberOfColumns];
            bool[] IsDateColumn = new bool[numberOfColumns];

            string[] excelColumnNames = new string[numberOfColumns];
            for (int n = 0; n < numberOfColumns; n++)
                excelColumnNames[n] = GetExcelColumnName(n);

            //
            //  Create the Header row in our Excel Worksheet
            //
            uint rowIndex = 1;

            writer.WriteStartElement(new Row { RowIndex = rowIndex });
            for (int colInx = 0; colInx < numberOfColumns; colInx++)
            {
                DataColumn col = dt.Columns[colInx];
                AppendHeaderTextCell(excelColumnNames[colInx] + "1", col.Caption, writer);//Caption misto ColumnName
                IsIntegerColumn[colInx] = (col.DataType.FullName.StartsWith("System.Int"));
                IsFloatColumn[colInx] = (col.DataType.FullName == "System.Decimal") || (col.DataType.FullName == "System.Double") || (col.DataType.FullName == "System.Single");
                IsDateColumn[colInx] = (col.DataType.FullName == "System.DateTime");

            }
            writer.WriteEndElement();   //  End of header "Row"

            //
            //  Now, step through each row of data in our DataTable...
            //
            double cellFloatValue = 0;
            CultureInfo ci = new CultureInfo("en-US");

            foreach (DataRow dr in dt.Rows)
            {
                // ...create a new row, and append a set of this row's data to it.
                ++rowIndex;

                writer.WriteStartElement(new Row { RowIndex = rowIndex });

                for (int colInx = 0; colInx < numberOfColumns; colInx++)
                {
                    cellValue = dr.ItemArray[colInx].ToString();
                    cellValue = ReplaceHexadecimalSymbols(cellValue);
                    cellReference = excelColumnNames[colInx] + rowIndex.ToString();

                    // Create cell with data
                    if (IsIntegerColumn[colInx] || IsFloatColumn[colInx])
                    {
                        //  For numeric cells without any decimal places.
                        //  If this numeric value is NULL, then don't write anything to the Excel file.
                        cellFloatValue = 0;
                        if (double.TryParse(cellValue, out cellFloatValue))
                        {
                            cellValue = cellFloatValue.ToString(ci);
                            AppendNumericCell(cellReference, cellValue, writer);
                        }
                    }
                    else if (IsDateColumn[colInx])
                    {
                        //  This is a date value.
                        DateTime dateValue;
                        if (DateTime.TryParse(cellValue, out dateValue))
                        {
                            AppendDateCell(cellReference, dateValue, writer);
                        }
                        else
                        {
                            //  This should only happen if we have a DataColumn of type "DateTime", but this particular value is null/blank.
                            AppendTextCell(cellReference, cellValue, writer);
                        }
                    }
                    else
                    {
                        //  For text cells, just write the input data straight out to the Excel file.
                        AppendTextCell(cellReference, cellValue, writer);
                    }
                }
                writer.WriteEndElement(); //  End of Row
            }
            writer.WriteEndElement(); //  End of SheetData
            writer.WriteEndElement(); //  End of worksheet

            writer.Close();
        }

        private static void AppendHeaderTextCell(string cellReference, string cellStringValue, OpenXmlWriter writer)
        {
            //  Add a new "text" Cell to the first row in our Excel worksheet
            //  We set these cells to use "Style # 3", so they have a gray background color & white text.
            writer.WriteElement(new Cell
            {
                CellValue = new CellValue(cellStringValue),
                CellReference = cellReference,
                DataType = CellValues.String
            });
        }

        private static void AppendTextCell(string cellReference, string cellStringValue, OpenXmlWriter writer)
        {
            //  Add a new "text" Cell to our Row 

            //  Add a new Excel Cell to our Row 
            writer.WriteElement(new Cell
            {
                CellValue = new CellValue(cellStringValue),
                CellReference = cellReference,
                DataType = CellValues.String
            });
        }

        private static void AppendDateCell(string cellReference, DateTime dateTimeValue, OpenXmlWriter writer)
        {
            //  Add a new "datetime" Excel Cell to our Row.
            //
            string cellStringValue = dateTimeValue.ToShortDateString();

            writer.WriteElement(new Cell
            {
                CellValue = new CellValue(cellStringValue),
                CellReference = cellReference,
                DataType = CellValues.String
            });
        }

        private static void AppendFormulaCell(string cellReference, string cellStringValue, OpenXmlWriter writer)
        {
            //  Add a new "formula" Excel Cell to our Row 
            writer.WriteElement(new Cell
            {
                CellFormula = new CellFormula(cellStringValue),
                CellReference = cellReference,
                DataType = CellValues.Number
            });
        }

        private static void AppendNumericCell(string cellReference, string cellStringValue, OpenXmlWriter writer)
        {
            //  Add a new numeric Excel Cell to our Row.
            writer.WriteElement(new Cell
            {
                CellValue = new CellValue(cellStringValue),
                CellReference = cellReference,
                DataType = CellValues.Number
            });
        }

        private static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }

        //  Convert a zero-based column index into an Excel column reference  (A, B, C.. Y, Y, AA, AB, AC... AY, AZ, B1, B2..)
        public static string GetExcelColumnName(int columnIndex)
        {
            //  eg  (0) should return "A"
            //      (1) should return "B"
            //      (25) should return "Z"
            //      (26) should return "AA"
            //      (27) should return "AB"
            //      ..etc..
            char firstChar;
            char secondChar;
            char thirdChar;

            if (columnIndex < 26)
            {
                return ((char)('A' + columnIndex)).ToString();
            }

            if (columnIndex < 702)
            {
                firstChar = (char)('A' + (columnIndex / 26) - 1);
                secondChar = (char)('A' + (columnIndex % 26));

                return string.Format("{0}{1}", firstChar, secondChar);
            }

            int firstInt = columnIndex / 676;
            int secondInt = (columnIndex % 676) / 26;
            if (secondInt == 0)
            {
                secondInt = 26;
                firstInt = firstInt - 1;
            }
            int thirdInt = (columnIndex % 26);

            firstChar = (char)('A' + firstInt - 1);
            secondChar = (char)('A' + secondInt - 1);
            thirdChar = (char)('A' + thirdInt);

            return string.Format("{0}{1}{2}", firstChar, secondChar, thirdChar);
        }

    }
}

