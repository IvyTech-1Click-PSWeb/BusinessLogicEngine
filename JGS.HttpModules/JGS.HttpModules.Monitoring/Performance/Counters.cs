using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Web;
using JGS.HttpModules.Monitoring.Support;

namespace JGS.HttpModules.Monitoring.Performance
{
	class Counters
	{
		private const string _COUNTER_CATEGORY = "JGSMonitoring";

		private static TimeSpan _expirationTimeSpan = new TimeSpan(0, 5, 0);

		static Counters()
		{
			if (ConfigurationManager.AppSettings["PerformanceExpirationMinutes"] != null)
			{
				_expirationTimeSpan = new TimeSpan(0, int.Parse(ConfigurationManager.AppSettings["PerformanceExpirationMinutes"]), 0);
			}
		}

		private PerformanceCounter _authenticatedUsersCounter;
		private PerformanceCounter _anonymousUsersCounter;
		private PerformanceCounter _maxWorkerThreadsCounter;
		private PerformanceCounter _availableWorkerThreadsCounter;
		private PerformanceCounter _maxIOThreadsCounter;
		private PerformanceCounter _availableIOThreadsCounter;

		public Counters()
		{
			_applicationName = HttpContext.Current.Request.ServerVariables["APPL_MD_PATH"].Replace("/","_");
				//HttpContext.Current.
				//Request.ApplicationPath.Replace("/", "_");

			_anonymousUsers = new ExpiringUniqueStringCounter(_expirationTimeSpan);
			_authenticatedUsers = new ExpiringUniqueStringCounter(_expirationTimeSpan);

			_authenticatedUsersCounter = new PerformanceCounter(_COUNTER_CATEGORY, "AuthenticatedUsers", ApplicationName, false);
			_anonymousUsersCounter = new PerformanceCounter(_COUNTER_CATEGORY, "AnonymousUsers", ApplicationName, false);
			_maxWorkerThreadsCounter = new PerformanceCounter(_COUNTER_CATEGORY, "MaxWorkerThreads", ApplicationName, false);
			_availableWorkerThreadsCounter = new PerformanceCounter(_COUNTER_CATEGORY, "AvailableWorkerThreads", ApplicationName, false);
			_maxIOThreadsCounter = new PerformanceCounter(_COUNTER_CATEGORY, "MaxIOThreads", ApplicationName, false);
			_availableIOThreadsCounter = new PerformanceCounter(_COUNTER_CATEGORY, "AvailableIOThreads", ApplicationName, false);
		}

		public void Refresh()
		{
			lock (this)
			{
				ThreadPool.GetMaxThreads(out _maxWorkerThreads, out _maxIOThreads);
				ThreadPool.GetAvailableThreads(out _availableWorkerThreads, out _availableIOThreads);
				_lastUpdated = DateTime.Now;

				_authenticatedUsersCounter.RawValue = AuthenticatedUsers;
				_anonymousUsersCounter.RawValue = AnonymousUsers;
				_maxWorkerThreadsCounter.RawValue = MaxWorkerThreads;
				_availableWorkerThreadsCounter.RawValue = AvailableWorkerThreads;
				_maxIOThreadsCounter.RawValue = MaxIOThreads;
				_availableIOThreadsCounter.RawValue = AvailableIOThreads;
			}
		}

		private string _applicationName;
		/// <summary>
		/// Unique name identifying originating application
		/// </summary>
		public string ApplicationName
		{
			get
			{
				return _applicationName;
			}
		}

		private DateTime _lastUpdated;
		/// <summary>
		/// Date&amp;time of last update of counters
		/// </summary>
		public DateTime LastUpdated
		{
			get
			{
				return _lastUpdated;
			}
		}

		private static ExpiringUniqueStringCounter _authenticatedUsers;
		/// <summary>
		/// Number of unique active authenticated users (IP addresses)
		/// </summary>
		public int AuthenticatedUsers
		{
			get
			{
				return _authenticatedUsers.Count;
			}
		}

		public void AddAuthenticatedUser(string ip)
		{
			_authenticatedUsers.Add(ip);
		}

		private static ExpiringUniqueStringCounter _anonymousUsers;
		/// <summary>
		/// Number of unique active anonymous users (IP addresses)
		/// </summary>
		public int AnonymousUsers
		{
			get
			{
				return _anonymousUsers.Count;
			}
		}

		public void AddAnonymousUser(string ip)
		{
			_anonymousUsers.Add(ip);
		}

		private int _maxWorkerThreads;
		/// <summary>
		/// Number of available worker threads in the thread pool
		/// </summary>
		public int MaxWorkerThreads
		{
			get
			{
				return _maxWorkerThreads;
			}
		}

		private int _availableWorkerThreads;
		/// <summary>
		/// Number of available worker threads in the thread pool
		/// </summary>
		public int AvailableWorkerThreads
		{
			get
			{
				return _availableWorkerThreads;
			}
		}

		private int _maxIOThreads;
		/// <summary>
		/// Number of available IO threads in the thread pool
		/// </summary>
		public int MaxIOThreads
		{
			get
			{
				return _maxIOThreads;
			}
		}

		private int _availableIOThreads;
		/// <summary>
		/// Number of available IO threads in the thread pool
		/// </summary>
		public int AvailableIOThreads
		{
			get
			{
				return _availableIOThreads;
			}
		}

	}
}
