using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using digiozPortal.BO;
using digiozPortal.DAL.Interfaces;
using digiozPortal.Utilities;

namespace digiozPortal.DAL
{
    public class ChatRepo : AbstractRepo<Chat>, IChatRepo 
    {
        public ChatRepo(IConfigHelper config) :base(config){}
        public List<Chat> GetLatestChats(int top, string orderBy, string order, string fields="*") {
            string sqlChats = $"SELECT TOP {top} {fields} FROM Chat ORDER BY {orderBy} {order};";
            using (var connection = new SqlConnection(_connectionString)) {
                return connection.Query<Chat>(sqlChats).OrderByDescending(x => x.Id).ToList();
            }
        }
    }
}
