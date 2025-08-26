using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IConexion
    {
        void Open();
        void Close();
        bool IsConected { get; }
        object GetConexion();

        string setConnectionString(string conectionString); 

    }
}
