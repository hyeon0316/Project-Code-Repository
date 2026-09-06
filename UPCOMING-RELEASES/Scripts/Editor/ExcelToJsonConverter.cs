using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
using System.Data;
using ExcelDataReader;
using Newtonsoft.Json;

public class ExcelToJsonConverter 
{
    public void ConvertExcelFilesToJson(string inputPath, string outputPath)
    {
        List<string> excelFiles = GetExcelFileNamesInDirectory(inputPath);
        Debug.Log("Excel To Json Converter: " + excelFiles.Count.ToString() + " Excel Files Found.");

        for (int i = 0; i < excelFiles.Count; i++)
        {
            if (!ConvertExcelFileToJson(excelFiles[i], outputPath))
            {
                break;
            }
        }
    }

   
    private List<string> GetExcelFileNamesInDirectory(string directory)
    {
        string[] directoryFiles = Directory.GetFiles(directory);
        List<string> excelFiles = new List<string>();

        Regex excelRegex = new Regex(@"^((?!(~\$)).*\.(xlsx|xls$))$");

        for (int i = 0; i < directoryFiles.Length; i++)
        {
            string fileName = directoryFiles[i].Substring(directoryFiles[i].LastIndexOf('\\') + 1);

            if (excelRegex.IsMatch(fileName))
            {
                excelFiles.Add(directoryFiles[i]);
            }
        }

        return excelFiles;
    }

    public bool ConvertExcelFileToJson(string filePath, string outputPath)
    {
        Debug.Log("Excel To Json Converter: Processing: " + filePath);
        DataSet excelData = GetExcelDataSet(filePath);

        if (excelData == null)
        {
            Debug.LogError("Excel To Json Converter: Failed to Process File: " + filePath);
            return false;
        }

        string spreadSheetJson = "";

        for (int i = 0; i < excelData.Tables.Count; i++)
        {
            spreadSheetJson = GetSpreadSheetJson(excelData, excelData.Tables[i].TableName);
            if (String.IsNullOrEmpty(spreadSheetJson))
            {
                Debug.LogError("Excel To Json Converter: Failed to Covert Spreadsheet '" + excelData.Tables[i].TableName + "' to json.");
                return false;
            }
            else
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                WriteTextToFile(spreadSheetJson, outputPath + "/" + fileName + ".json");
                Debug.Log("Excel To Json Converter: " + excelData.Tables[i].TableName + " Successfully Written to File.");
            }
        }

        return true;
    }

    private IExcelDataReader GetExcelDataReader(string filePath)
    {
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

        IExcelDataReader excelReader;

        Regex xlsRegex = new Regex(@"^(.*\.(xls$))");
        Regex xlsxRegex = new Regex(@"^(.*\.(xlsx$))");

        if (xlsRegex.IsMatch(filePath))
        {
            excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
        }
        else if (xlsxRegex.IsMatch(filePath))
        {
            excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        }
        else
        {
            Debug.LogError("Excel To Json Converter: Unexpected Files Type: " + filePath);
            stream.Close();
            return null;
        }

        return excelReader;
    }

    private DataSet GetExcelDataSet(string filePath)
    {
        using (var excelReader = GetExcelDataReader(filePath))
        {
            if (excelReader == null)
            {
                Debug.LogError("Excel To Json Converter: Excel Reader is Null. Cannot Read Data");
                return null;
            }

            DataSet data = new DataSet();

            do
            {
                DataTable table = GetExcelSheetData(excelReader, filePath);

                if (table != null)
                {
                    data.Tables.Add(table);
                }
            }
            while (excelReader.NextResult()); 

            return data;
        }
    }

   
    private DataTable GetExcelSheetData(IExcelDataReader excelReader, string filePath)
    {
        try
        {
            DataTable table = new DataTable(excelReader.Name);

            while (excelReader.Read())
            {
                DataRow row = table.NewRow();

                for (int i = 0; i < excelReader.FieldCount; i++)
                {
                    //첫 행(헤더)
                    if (excelReader.Depth == 0)
                    {
                        string header = excelReader.GetString(i);
                        table.Columns.Add(header, typeof(string));
                    }
                    else
                    {
                        var raw = excelReader.GetValue(i);
                        row[table.Columns[i]] = raw?.ToString();
                    }
                }

                //헤더를 읽었던 경우는 제외
                if (excelReader.Depth != 0)
                {
                    table.Rows.Add(row);
                }
            }

            return table;
        }
        catch (Exception e)
        {
            Debug.LogError($"Excel To Json Converter: {e.Message}, filePath: {filePath}" );
            return null;
        }
    }

    private string GetSpreadSheetJson(DataSet excelDataSet, string sheetName)
    {
        DataTable dataTable = excelDataSet.Tables[sheetName];

        for (int col = dataTable.Columns.Count - 1; col >= 0; col--)
        {
            bool removeColumn = true;
            foreach (DataRow row in dataTable.Rows)
            {
                if (!row.IsNull(col))
                {
                    removeColumn = false;
                    break;
                }
            }

            if (removeColumn)
            {
                dataTable.Columns.RemoveAt(col);
            }
        }

        return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
    }

    private void WriteTextToFile(string text, string filePath)
    {
        System.IO.File.WriteAllText(filePath, text);
    }

}
