using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace JGS.HttpModules.Monitoring.Support
{
	internal static class Methods
	{
		public const string EVENT_LOG = "Application";
		public static string EVENT_LOG_SOURCE = "Web Performance on " + Environment.MachineName;

		public static string GetMessageStack(this Exception ex)
		{
			return ex.Message + "\n\nInnerException:\n" + (ex.InnerException != null ? ex.InnerException.GetMessageStack() : "");
		}

		public static string GetIpAddress()
		{
			string ip = null;
			if (!string.IsNullOrEmpty(HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]))
			{
				ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
				string[] ipRange = ip.Split(',');
				string trueIP = ipRange[0].Trim();
			}
			else if (!string.IsNullOrEmpty(HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]))
			{
				ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].Trim();
			}
			else
			{
				ip = "UNKNOWN";
			}
			return ip;
		}
	}
}
