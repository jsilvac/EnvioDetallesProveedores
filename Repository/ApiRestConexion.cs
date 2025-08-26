using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.Extensions.Configuration;

namespace Repository 
{
    public class ApiRestConexion : IConexion
    {
        private HttpClient _Client;
        private readonly string _baseUrl;
     

        public bool IsConected => _Client != null;

        public ApiRestConexion()
        {
           
        }

        public void Open()
        {
            _Client = new HttpClient()
            {
                BaseAddress = new System.Uri(_baseUrl)
            };
        }

        public void Close()
        { 
            _Client?.Dispose();
            _Client = null;
        }

        public string setConnectionString(string conectionString)
        {
           
            return "";
        }

        public object GetConexion() => _Client;
    }
}
