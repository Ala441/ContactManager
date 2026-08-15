using System.Data;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace ContactManager.DataAccess
{
    public class clsContactData
    {
        private static string Connectionstring = "Server=.; Database = contactsdb1; User Id=your name; Password = your password; Encrypt = True;TrustServerCertificate = True;";

        public static async Task<DataTable> ExecuteStoredProcedure(string Procedurename, SqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection Connection = new SqlConnection(Connectionstring))
            {
                using (SqlCommand Command = new SqlCommand(Procedurename, Connection))
                {
                    Command.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        Command.Parameters.AddRange(parameters);
                    }
                    await Connection.OpenAsync();
                    dt.Load(await Command.ExecuteReaderAsync());
                }
            }
            return dt;
        }

        public static async Task<object> ExecuteScalar(string Procedurename, SqlParameter[] parameter = null)
        {
            object Result = null;
            using (SqlConnection Connection = new SqlConnection(Connectionstring))
            {
                using (SqlCommand Command = new SqlCommand(Procedurename, Connection))
                {
                    Command.CommandType = CommandType.StoredProcedure;

                    if (parameter != null)
                    {
                        Command.Parameters.AddRange(parameter);
                    }
                    await Connection.OpenAsync();

                    Result = await Command.ExecuteScalarAsync();
                }
            }
            return Result;
        }

        public static async Task<int> ExecuteNonQuery(string ProcedurName, SqlParameter[] Parameters = null)
        {
            int RowAfect = 0;
            using (SqlConnection Connection = new SqlConnection(Connectionstring))
            {
                using (SqlCommand Command = new SqlCommand(ProcedurName, Connection))
                {
                    Command.CommandType = CommandType.StoredProcedure;

                    if (Parameters != null)
                    {
                        Command.Parameters.AddRange(Parameters);
                    }
                    await Connection.OpenAsync();

                    RowAfect = await Command.ExecuteNonQueryAsync();
                }
            }
            return RowAfect;
        }
    }
}
