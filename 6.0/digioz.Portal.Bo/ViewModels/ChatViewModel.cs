using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digioz.Portal.Bo.ViewModels
{
	public class ChatViewModel
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string Message { get; set; }
		public DateTime? Timestamp { get; set; }
	}
}
