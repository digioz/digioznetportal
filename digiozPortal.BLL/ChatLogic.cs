using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using digiozPortal.BLL.Interfaces;
using digiozPortal.BO;
using digiozPortal.DAL.Interfaces;

namespace digiozPortal.BLL
{
    public class ChatLogic : AbstractLogic<Chat>, IChatLogic
    {
        private string _connectionString;

        public ChatLogic(IRepo<Chat> repo) : base(repo) { }

        public List<Chat> GetLatestChats() {

            var chats = new List<Chat>();

            string sqlChats = "SELECT TOP 10 * FROM Chat ORDER BY Id DESC;";

            using (var connection = new SqlConnection(_connectionString)) {
                chats = connection.Query<Chat>(sqlChats).OrderByDescending(x => x.Id).ToList();
            }

            return chats;
        }
    }
}
