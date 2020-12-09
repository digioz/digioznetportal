using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Dal
{
    public class ChatRepo : AbstractRepo<Chat>, IChatRepo 
    {
        public ChatRepo(IConfigHelper config) :base(config){}

        public List<Chat> GetLatestChats(int top, string orderBy, string order, string fields="*") {
            string sqlChats = $"SELECT TOP {top} {fields} FROM Chat ORDER BY {orderBy} {order};";
            return base.Get(sqlChats).ToList();

            //using (var db = new digiozPortalContext()) {
            //    return db.Query<Chat>(sqlChats).OrderByDescending(x => x.Id).ToList();
            //}
        }
    }
}
