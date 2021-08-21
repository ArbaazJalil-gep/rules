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
using Syncfusion.XlsIO.Parser.Biff_Records.PivotTable;
using System.Collections.Concurrent;
using Newtonsoft.Json.Schema;
using System.Data.SqlClient;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trie;

namespace Validator.Controllers
{

    public class Rule
    {
        public string ConcatinatedValue { get; set; }
        public List<string> columns { get; set; }
    }
    public class CustomRule
    {
        public string propertyName { get; set; }
        public string value { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class ValidateController : ControllerBase
    {

        [HttpGet]
        public IActionResult Validate()
        {
            
            var lineitems = ConvertExcelToDataTable(@".\Data\dataset5000.xlsx");
            var rules = ConvertExcelToDataTable(@".\Data\2MRules.xlsx");
            var timer = new Stopwatch();
            timer.Start();
       
            SuffixTrie suffixTrie = new SuffixTrie(rules);
            var list = new List<int>();
            for (int i = 0; i < lineitems.Rows.Count; i++)
            {
                var isLineValid = suffixTrie.Contains(lineitems, i);
                if (isLineValid)
                    list.Add(i);
            }


 


            timer.Stop();

            TimeSpan timeTaken = timer.Elapsed;
            string foo = "Time taken: " + timeTaken.ToString(@"m\:ss\.fff");
            Console.WriteLine(foo);












            //CreateTableAndInsertData(rules,"rules");
            //CreateTableAndInsertData(lineitems,"lineitems");
            //var header = rules.Rows[0];
            //var sqlsb = new StringBuilder();
            //sqlsb.Append("SELECT Id FROM LINEITEMS WHERE 1=1 AND");
            //for (int i = 1; i < lineitems.Rows.Count; i++)
            //{

            //    for (int j = 0; j < lineitems.Columns.Count; j++)
            //    {
            //        if (lineitems.Rows[i][j].ToString() == "*")
            //            continue;
            //            if (j > 0)
            //            {
            //                sqlsb.Append(" AND ");
            //            }
            //            else
            //            {
            //                sqlsb.Append(" (");
            //            }

            //            sqlsb.Append($"{lineitems.Columns[j].ColumnName}='{ lineitems.Rows[i][j].ToString()}'");

            //            if (j == lineitems.Columns.Count - 1)
            //                sqlsb.Append(")");

            //    }
            //    sqlsb.Append(")");

            //    if (i<rules.Rows.Count-1)
            //    sqlsb.Append(" OR \n");
            //}

            //var sqlQuery = "select * from lineitems where id not in ("+ sqlsb.ToString()+")";

            //var resultDt = extentions.ValidateLineItems("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=test;User Id=sa;Password=Admin@2012",sqlQuery);

            //return Ok(!(resultDt.Rows.Count>0));
            return Ok(true);



            //var ruleDictionary = new Dictionary<string, List<string>>();
            //var ruleList = new List<Rule>();

            //var lineChunks = lineitems.AsEnumerable().ToChunks(100)
            //              .Select(rows => rows.CopyToDataTable());

            //for (int i = 1; i < rules.Rows.Count; i++)
            //{
            //    var sb = new StringBuilder();
            //    var colList = new List<string>();
            //    for (int j = 0; j < rules.Columns.Count; j++)
            //    {
            //        if (rules.Rows[i][j].ToString() != "*")
            //        {
            //            sb.Append(rules.Rows[i][j].ToString());
            //            colList.Add(rules.Columns[j].ColumnName);
            //        }
            //    }
            //    ruleList.Add(new Rule() { ConcatinatedValue = sb.ToString(), columns = colList });
            //    // ruleDictionary.Add(sb.ToString(), colList);
            //    sb.Clear();
            //}

            //int ruleid = 1;
            //var results = new ConcurrentBag<bool>();
            //Parallel.ForEach(lineChunks, chunk => ValidateLineItems(chunk, ruleList, results));

            //timer.Stop();

            //TimeSpan timeTaken = timer.Elapsed;
            //string foo = "Time taken: " + timeTaken.ToString(@"m\:ss\.fff");
            //Console.WriteLine(foo);
            //return Ok(!results.Contains(false));
        }

        private static void CreateTableAndInsertData(DataTable dt, string tablename)
        {
            using (SqlConnection connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=test;User Id=sa;Password=Admin@2012"))
            {
                connection.Open();
                var sqlCreator = new SqlTableCreator(connection);
                sqlCreator.DestinationTableName = tablename;

                var r = sqlCreator.CreateFromDataTable(dt);
                connection.Close();
            }
            dt.BulkInsert("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=test;User Id=sa;Password=Admin@2012", tablename);
        }

        private void ValidateLineItems(DataTable lineitems, List<Rule> ruleList, ConcurrentBag<bool> result)
        {
            var isLineValid = false;
            for (int lineIdx = 1; lineIdx < lineitems.Rows.Count; lineIdx++)
            {
                isLineValid = false;
                for (int ruleIdx = 0; ruleIdx < ruleList.Count; ruleIdx++)
                {
                    var cols = ruleList[ruleIdx].columns;
                    var line = getLineItemStringCompact(lineitems, lineIdx, cols);
                    if (line == ruleList[ruleIdx].ConcatinatedValue)
                    {
                        isLineValid = true;
                        result.Add(true);
                        break;
                    }
                }
                if (!isLineValid)
                {
                    result.Add(false);
                    break;
                }
            }

        }


        private string getLineItemStringCompact(DataTable dt, int idx, List<string> list)
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