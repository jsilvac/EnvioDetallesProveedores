using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
//using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.Extensions.Configuration;

namespace Repository
{
    public class MySqlConexion : IConexion
    {
        private MySqlConnection _connection;
        private string _connectionString;
 

        public bool IsConected => _connection != null && _connection.State == System.Data.ConnectionState.Open;

        public MySqlConexion()
        {
           
        }

        public void Open()
        {
            _connection = new MySqlConnection(_connectionString);
            _connection.Open();
        }

        public void Close()
        {
            if (_connection != null && _connection.State != System.Data.ConnectionState.Closed)
                _connection.Close();
        }

        public object GetConexion() => _connection;

        public string setConnectionString(string conectionString)
        {
            _connectionString = conectionString;
            return _connectionString;
        }
    }
}
