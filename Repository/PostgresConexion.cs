using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.Extensions.Configuration;

namespace Repository
{
    public class PostgresConexion : IConexion
    {
        private NpgsqlConnection _connection;
        private  string _connectionString;
       

        public bool IsConected => _connection != null && _connection.State == System.Data.ConnectionState.Open;
        
        public PostgresConexion()
        {
            
        }

        public void Open()
        {
            _connection = new NpgsqlConnection(_connectionString);
            _connection.Open();
        }

        public void Close() 
        { 
            if (_connection != null && _connection.State != System.Data.ConnectionState.Closed)
                _connection.Close();
        }

        public string setConnectionString(string conectionString)
        {
            _connectionString = conectionString;
            return _connectionString;
        }
        public object GetConexion() => _connection;

    }
}
