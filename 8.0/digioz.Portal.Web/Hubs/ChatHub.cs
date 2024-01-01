using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using digioz.Portal.Bll;
using digioz.Portal.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Hubs
{
	public class ChatHub : Hub
	{
		private readonly IChatLogic _chatLogic;

		public ChatHub(
			IChatLogic chatLogic
		)
		{
			_chatLogic = chatLogic;
		}

		public async Task SendMessage(string user, string userid, string message)
		{
			if (!string.IsNullOrEmpty(message) && message != "[object HTMLInputElement]")
			{
				var chat = new Chat
				{
					Timestamp = DateTime.Now,
					Message = HttpUtility.HtmlEncode(message),
					UserId = userid
				};

				_chatLogic.Add(chat);
			}

			await Clients.All.SendAsync("ReceiveMessage", user, message);


		}
	}
}
