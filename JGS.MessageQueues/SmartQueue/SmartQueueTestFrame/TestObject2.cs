using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace JGS.MessageQueues.SmartQueueTestFrame
{
	[Serializable]
    public class TestObject2 
    {
        public TestObject2()
        {
        }
    private string _TestProperty1 = "TestObject2.TestProperty1";

        public string TestProperty1
        {
            get { return _TestProperty1; }
            set { _TestProperty1 = value; }
        }
    }
}
