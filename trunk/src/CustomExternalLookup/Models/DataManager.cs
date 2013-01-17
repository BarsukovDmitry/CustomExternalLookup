using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Microsoft.SharePoint;

namespace CustomExternalLookup.Models
{
    class DataManager
    {
        readonly string _connectionString;
        readonly string _queryString;
        readonly SqlConnection _conn;

        public DataManager(string connectionString, string queryString)
        {
            var regEx = new Regex(@"(\binsert\b)|(\bupdate\b)|(\bdelete\b)", RegexOptions.IgnoreCase);
            if(regEx.IsMatch(queryString))
                throw new SPException("Строка запроса поля CustomExternalLookup не может содержать слова \"insert\", \"update\", \"delete\"");
            
            _connectionString = connectionString;
            _queryString = queryString;
            _conn = new SqlConnection(_connectionString);
        }
        
        private DataTable CreateDataTable(SqlDataReader reader)
        {
            var table = new DataTable();
            table.PrimaryKey = new[] {table.Columns.Add("ID", typeof(int))};
            table.Columns.Add("Value", typeof(string));
            table.DefaultView.Sort = "ID";

            while (reader.Read())
            {
                DataRow newRow = table.NewRow();
                newRow["ID"] = reader["ID"];
                newRow["Value"] = reader["Value"];
                table.Rows.Add(newRow);
            }

            return table;
        }
        
        public DataTable GetRecords()
        {
            SqlCommand command = _conn.CreateCommand();
            command.CommandText = _queryString;
            _conn.Open();
            SqlDataReader reader = command.ExecuteReader();

            DataTable result = CreateDataTable(reader);

            _conn.Close();
            return result;
        }

        public DataTable GetRecords(string valuePattern)
        {
            SqlCommand command = _conn.CreateCommand();
            string commandText = string.Format("SELECT * FROM ({0}) as st WHERE st.Value LIKE @pattern", _queryString);
            command.CommandText = commandText;
            command.Parameters.Add("@pattern", SqlDbType.NVarChar);
            command.Parameters["@pattern"].Value = string.Format("%{0}%", valuePattern);
            _conn.Open();
            SqlDataReader reader = command.ExecuteReader();

            DataTable result = CreateDataTable(reader);

            _conn.Close();
            return result;
        }

        public DataRow GetRecord(string value)
        {
            SqlCommand command = _conn.CreateCommand();
            string commandText = string.Format("SELECT * FROM ({0}) as st WHERE st.Value = @value", _queryString);
            command.CommandText = commandText;
            command.Parameters.Add("@value", SqlDbType.NVarChar);
            command.Parameters["@value"].Value = value;
            _conn.Open();
            SqlDataReader reader = command.ExecuteReader();

            DataTable result = CreateDataTable(reader);
            _conn.Close();

            if (result.Rows.Count > 0)
                return result.Rows[0];

            return null;
        }

        public DataTable GetRecordsByIds(int[] ids)
        {
            SqlCommand command = _conn.CreateCommand();

            string inParam = "";
            for (int i=0; i<ids.Length; ++i)
            {
                if (i > 0)
                    inParam += ",";
                inParam += ids[i].ToString();
            }

            string commandText = string.Format("SELECT * FROM ({0}) as st WHERE st.ID IN ({1})", _queryString, inParam);
            command.CommandText = commandText;
            _conn.Open();
            SqlDataReader reader = command.ExecuteReader();

            DataTable result = CreateDataTable(reader);

            _conn.Close();
            return result;
        }
    }
}
