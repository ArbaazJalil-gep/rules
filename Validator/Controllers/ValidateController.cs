using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.XlsIO;
using System.Text;
using System.Diagnostics;

namespace Validator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValidateController : ControllerBase
    {

        [HttpGet]
        public IActionResult Validate()
        {
            var timer = new Stopwatch();
            timer.Start();
            var lineitems = ConvertExcelToDataTable(@".\Data\dataset.xlsx");
            var rules = ConvertExcelToDataTable(@".\Data\rules.xlsx");

            var header = rules.Rows[0];


            var ruleDictionary = new Dictionary<string, List<string>>();


            for(int i = 1; i < rules.Rows.Count; i++)
            {
                var sb = new StringBuilder();
                var colList = new List<string>();
                for (int j = 0; j < rules.Columns.Count; j++)
                {
                    if (rules.Rows[i][j].ToString() != "*")
                    {
                        sb.Append(rules.Rows[i][j].ToString());
                        colList.Add(rules.Columns[j].ColumnName);
                    }
                }
                ruleDictionary.Add(sb.ToString(), colList);
                sb.Clear();
            }
            int ruleid = 1;
            foreach (KeyValuePair<string, List<string>> entry in ruleDictionary)
            {
                Console.WriteLine(ruleid++);
                var cols = entry.Value;
                
                for (int lineIdx = 1; lineIdx < lineitems.Rows.Count; lineIdx++)
                {
                    var line = getLineItemStringCompact(lineitems, lineIdx, cols);
                    if (line == entry.Key)
                    {
                        timer.Stop();

                        TimeSpan timeTaken2 = timer.Elapsed;
                        string foo2 = "Time taken: " + timeTaken2.ToString(@"m\:ss\.fff");
                        Console.WriteLine(foo2);
                        return Ok(true);
                    }
                }
            }
            timer.Stop();

            TimeSpan timeTaken = timer.Elapsed;
            string foo = "Time taken: " + timeTaken.ToString(@"m\:ss\.fff");
            Console.WriteLine(foo);
            return Ok(false);
        }

        private string getLineItemStringCompact(DataTable dt, int idx,List<string> list)
        {
           
            var sb = new StringBuilder();
            foreach (var col in list)
            {
                if (dt.Columns.Contains(col) && dt.Rows[idx][col] != null)
                {
                    sb.Append(dt.Rows[idx][col].ToString());
                }

            }
            return sb.ToString();

        }
        public string getLineItemString(DataTable dt, int idx)
        {
            var list = new List<string>(){"ORG_Field1", "ORG_Field2",  "ORG_Field3", "ORG_Field4",  "ORG_Field5",
                "ORG_Field6",  "ORG_Field7", "ORG_Field8",  "ORG_Field9", "ORG_Field10", "ORG_Field11", "ORG_Field12",
                "CCStructureId"  };
            var sb = new StringBuilder();
            foreach (var col in list)
            {
                if (dt.Columns.Contains(col) && dt.Rows[idx][col] != null)
                {
                    sb.Append(dt.Rows[idx][col].ToString());
                }              

            }
            return sb.ToString();

        }
        internal Dictionary<string, object> GetDict(DataTable dt)
        {
            return dt.AsEnumerable()
              .ToDictionary<DataRow, string, object>(row => row.Field<string>(0),
                                        row => row.Field<object>(1));
        }
        private DataTable ConvertExcelToDataTable(string path)
        {
            using Stream inputStream = System.IO.File.OpenRead(path);
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                IWorkbook workbook = application.Workbooks.Open(inputStream);
                IWorksheet worksheet = workbook.Worksheets[0];

                DataTable dataTable = worksheet.ExportDataTable(worksheet.UsedRange, ExcelExportDataTableOptions.ColumnNames);
                return dataTable;
            }
        }
    }
}