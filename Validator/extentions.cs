using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Validator
{
    public static class extentions
    {
        public static IEnumerable<IEnumerable<T>> ToChunks<T>(this IEnumerable<T> enumerable,
                                                           int chunkSize)
        {
            int itemsReturned = 0;
            var list = enumerable.ToList(); // Prevent multiple execution of IEnumerable.
            int count = list.Count;
            while (itemsReturned < count)
            {
                int currentChunkSize = Math.Min(chunkSize, count - itemsReturned);
                yield return list.GetRange(itemsReturned, currentChunkSize);
                itemsReturned += currentChunkSize;
            }
        }


        public static void BulkInsert(this DataTable table,string connectionString, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlBulkCopy bulkCopy =
                    new SqlBulkCopy
                    (
                    connection,
                    SqlBulkCopyOptions.TableLock |
                    SqlBulkCopyOptions.FireTriggers |
                    SqlBulkCopyOptions.UseInternalTransaction,
                    null
                    );

                bulkCopy.DestinationTableName = tableName;
                connection.Open();

                bulkCopy.WriteToServer(table);
                connection.Close();
            }
        }

        public static DataTable ValidateLineItems(string connectionString, string query)
        {
            string constring = connectionString;
            using (SqlConnection con = new SqlConnection(constring))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
        }
    }
}
