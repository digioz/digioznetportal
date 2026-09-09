namespace digioz.Portal.Utilities
{
    public class ConfigHelper : IConfigHelper
    {
        private readonly string _connectionString;
        public ConfigHelper(string connectionString) {
            _connectionString = connectionString;
        }
        public string GetConnectionString() {
            return _connectionString;
        }
    }

}
