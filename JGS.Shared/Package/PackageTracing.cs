using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using JGS.Shared.Package;
using System.Configuration;

namespace JGS.Shared.Package
{
	public enum PackageTraceLevel
	{
		Off,
		Error,
		Warning,
		Info,
		Verbose
	}

	public static class PackageTracing
	{
		static PackageTracing()
		{
			try
			{
				TraceLevel = (PackageTraceLevel)Enum.Parse(typeof(PackageTraceLevel),
					ConfigurationManager.AppSettings["BusinessLogicEngineTraceLevel"]);
			}
			catch
			{
				TraceLevel = PackageTraceLevel.Verbose;
			}
		}

		public static PackageTraceLevel TraceLevel
		{
			get;
			set;
		}

		public static void TraceEvent(this Package package, string message, PackageTraceLevel eventTraceLevel)
		{
			if (eventTraceLevel <= TraceLevel)
			{
				package.TransactionInformation.Messages.Add(message);
			}
		}
	}
}
