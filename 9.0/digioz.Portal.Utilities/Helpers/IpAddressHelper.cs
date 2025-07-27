using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digioz.Portal.Utilities.Helpers
{
	public static class IpAddressHelper
	{
		public static string GetUserIPAddress(HttpContext context)
		{
			string ip = String.Empty;

			try
			{
				ip = context.Connection. RemoteIpAddress.ToString();

				if (ip == "::1")
				{
					ip = "127.0.0.1";
				}
			}
			catch
			{
				ip = "Unable to resolve";
			}

			return ip;
		}
	}
}
