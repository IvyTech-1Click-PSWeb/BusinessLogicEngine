using System;
using System.Collections;
using System.Collections.Specialized;
using System.Timers;

namespace JGS.HttpModules.Monitoring.Support
{
	/// <summary>
	/// Calculates number of unique strings passed to the object instance within a life time period
	/// </summary>
	internal class ExpiringUniqueStringCounter
	{
		private Hashtable strings = new Hashtable();
		private Timer _cleanupTimer = new Timer(5000);

		private TimeSpan lifeTimeValue;
		public ExpiringUniqueStringCounter(TimeSpan lifeTime)
		{
			this.LifeTime = lifeTime;
			_cleanupTimer.Elapsed += new ElapsedEventHandler(_cleanupTimer_Elapsed);
		}

		void _cleanupTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			lock (strings)
			{
				DateTime limit = DateTime.Now.Subtract(LifeTime);
				StringCollection keysToRemove = new StringCollection();
				foreach (string key in strings.Keys)
				{
					DateTime updated = (DateTime)strings[key];
					if (updated < limit && !keysToRemove.Contains(key))
						keysToRemove.Add(key);
				}
				foreach (string key in keysToRemove)
					strings.Remove(key);
			}
		}

		public TimeSpan LifeTime
		{
			get
			{
				return lifeTimeValue;
			}
			set
			{
				lifeTimeValue = value;
			}
		}

		public int Count
		{
			get
			{
				return strings.Count;
			}
		}

		public void Add(string s)
		{
			if (strings.ContainsKey(s))
			{
				strings[s] = DateTime.Now;
			}
			else
			{
				strings.Add(s, DateTime.Now);
			}
		}

		public void Reset()
		{
			strings.Clear();
		}
	}
}
