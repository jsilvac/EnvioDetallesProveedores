using Repository;
using System.Data;

namespace Datos
{
    public class CrudManager : IDisposable
    {
        private  IConexion _conexion;
        private bool _disposed = false;

      
        public CrudManager(IConexion conexion)
        {
            _conexion = conexion ??throw new ArgumentNullException(nameof(conexion)) ;
        }

        public int ExecuteCommand(string sql,Dictionary<string, object> parametros = null) 
        {
            try
            {
                EnsureConnectionOpen();

                using (var cmd = NewCommand(sql, parametros))
                {
                    return cmd.ExecuteNonQuery();
                }

            }
            catch (Exception ex) 
            {
                throw new Exception($"Error ejecutando comando: {sql}", ex);
            }

        }

        public DataTable ExecuteConsulta(string sql, Dictionary<string, object> parametros = null)
        {
            try
            {
                EnsureConnectionOpen();

                using (var cmd = NewCommand(sql, parametros))
                {
                    var reader = cmd.ExecuteReader();  
                    var table = new DataTable();
                    table.Load(reader);  

                    return table;
                }

            }catch (Exception ex) 
            {
                throw new Exception($"Error ejecutando comando: {sql}", ex);
            }
        }


        private IDbCommand NewCommand(string sql, Dictionary<string, object> parametros)
        {
            EnsureConnectionOpen();

            var connection = (IDbConnection)_conexion.GetConexion();
            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            if (parametros != null)
            {
                foreach (var p in parametros)
                {
                    var parametro = cmd.CreateParameter();
                    parametro.ParameterName = p.Key;
                    parametro.Value = p.Value ?? DBNull.Value;
                    cmd.Parameters.Add(parametro);
                }
            }

            return cmd;
        }
        private void EnsureConnectionOpen()
        {
            if (!_conexion.IsConected)
                _conexion.Open();
        }

        // Implementación del patrón Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Liberar recursos manejados
                    if (_conexion != null && _conexion.IsConected)
                    {
                        _conexion.Close();
                    }
                }

                _disposed = true;
            }
        }

        // Destructor por si acaso no se llama Dispose
        ~CrudManager()
        {
            Dispose(false);
        }
    }
}
