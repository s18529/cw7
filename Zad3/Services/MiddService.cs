using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Zad3.Services
{
    public class MiddService : IMiddService
    {
        private string SqlCon = "Data Source=db-mssql;Initial Catalog=s18529;Integrated Security=True";
        public bool checkIndex(string index)
        {
            using(var client = new SqlConnection(SqlCon))
            using(var command = new SqlCommand())
            {
                command.Connection = client;
                client.Open();
                command.CommandText = "select * from student where IndexNumber = @index";
                command.Parameters.AddWithValue("index", index);
                var dr = command.ExecuteReader();
                if (dr.Read())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
