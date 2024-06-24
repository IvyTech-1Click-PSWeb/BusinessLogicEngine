using System;
using System.Linq;
using System.Xml.Serialization;

namespace JGS.MessageQueues.Support
{
	public static class Extensions
	{
		public static bool CanBeXmlSerialized(this object o)
		{
			Type objectToTest = typeof(object);
			Type[] interfaces = objectToTest.GetInterfaces();
			bool containsIXmlSerializable = interfaces.Contains(typeof(IXmlSerializable));

			return containsIXmlSerializable || objectToTest.IsSerializable;
		}
	}
}
