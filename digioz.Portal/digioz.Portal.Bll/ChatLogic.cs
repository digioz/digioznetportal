using System;
using System.Collections.Generic;
using System.Data;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Bo.ViewModels;
using digioz.Portal.Dal;
using digioz.Portal.Dal.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace digioz.Portal.Bll
{
    public class ChatLogic : AbstractLogic<Chat>, IChatLogic
    {
        IChatRepo _repo;
        public ChatLogic(IChatRepo repo) : base(repo) 
        {
            _repo = repo;
        }

        public List<ChatViewModel> GetLatestChats() 
        {
				var models = new List<ChatViewModel>();

				using (var context = new digiozPortalContext())
				using (var command = context.Database.GetDbConnection().CreateCommand())
				{
					var query = @"SELECT TOP(50) c.*, u.[UserName] FROM [Chat] c
								INNER JOIN [AspNetUsers] u ON u.[Id] = c.[UserId]
								ORDER BY c.Id DESC;";

				command.CommandText = query;
					context.Database.OpenConnection();

					using (var result = command.ExecuteReader())
					{
						int count = result.FieldCount;

						var dt = new DataTable();
						dt.Load(result);

						foreach (DataRow dr in dt.Rows)
						{
							var chat = new ChatViewModel()
							{
								Id = Convert.ToInt32(dr["Id"]),
								UserId = dr["UserId"].ToString(),
								UserName = dr["UserName"].ToString(),
								Message = dr["Message"].ToString(),
								Timestamp = Convert.ToDateTime(dr["Timestamp"])
							};

							models.Add(chat);
						}
					}

					context.Database.CloseConnection();
				}

				return models;
			}

    }
}
