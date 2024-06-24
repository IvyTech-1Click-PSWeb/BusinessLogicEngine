using System;
using System.Diagnostics;
using System.Timers;
using System.Web;
using JGS.HttpModules.Monitoring.Support;

namespace JGS.HttpModules.Monitoring.Performance
{
	public class HttpModule : IHttpModule
	{
		private static Counters _counters=new Counters();
		private static Timer _refreshTimer;

		static HttpModule()
		{
			// Initialize and publish counters
			try
			{
				_counters.Refresh();
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry(Support.Methods.EVENT_LOG_SOURCE, "An exception occurred initializing the counters:\n" + ex.GetMessageStack() 
					+ "\n\n" +ex.StackTrace, EventLogEntryType.Error);
			}

			try
			{
				// Start periodic refresh of counters
				_refreshTimer = new Timer(1000);
				_refreshTimer.Elapsed += new ElapsedEventHandler(_refreshTimer_Elapsed);
				_refreshTimer.Start();
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry(Support.Methods.EVENT_LOG_SOURCE, "An exception occurred initializing the refresh:\n" + ex.GetMessageStack()
					+ "\n\n" + ex.StackTrace, EventLogEntryType.Error);
			}			

		}

		static void _refreshTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				_refreshTimer.Stop();
				_counters.Refresh();
				_refreshTimer.Start();
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry(Support.Methods.EVENT_LOG_SOURCE, "An exception occurred updating the counters:\n" + ex.GetMessageStack(), EventLogEntryType.Error);
			}
		}

		public void Init(System.Web.HttpApplication context)
		{
			try {
			context.BeginRequest += new EventHandler(context_BeginRequest);
			context.EndRequest += new EventHandler(context_EndRequest);
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry(Support.Methods.EVENT_LOG_SOURCE, "An exception occurred starting data collection:\n" + ex.GetMessageStack(), EventLogEntryType.Error);
			}
		}

		public void Dispose()
		{
		}


		private void context_BeginRequest(object sender, EventArgs e)
		{
			try
			{
				// Increment corresponding counter
				string ipAddress = Methods.GetIpAddress();
				if (HttpContext.Current.Request.IsAuthenticated)
				{
					_counters.AddAuthenticatedUser(ipAddress);
				}
				else
				{
					_counters.AddAnonymousUser(ipAddress);
				}
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry(Support.Methods.EVENT_LOG_SOURCE, "An exception occurred identifying the user:\n" + ex.GetMessageStack(), EventLogEntryType.Error);
			}
		}

		private void context_EndRequest(object sender, EventArgs e)
		{

		}


	}
}
