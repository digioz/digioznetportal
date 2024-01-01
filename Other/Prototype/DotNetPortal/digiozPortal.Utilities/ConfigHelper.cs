using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace digiozPortal.Utilities
{
    public class ConfigHelper
    {
        private readonly IConfiguration _configuration;

        public string GetConnectionString(string connectionName) {
            string connectionString = _configuration.GetConnectionString(connectionName);

            return connectionString;
        }
    }
}
