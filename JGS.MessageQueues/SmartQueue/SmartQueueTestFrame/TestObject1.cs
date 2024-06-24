using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace JGS.MessageQueues.SmartQueueTestFrame
{
	[Serializable]
    public class TestObject1
    {
        public TestObject1()
        {
        }
        private string _TestProperty1 = "Testme";

        public string TestProperty1
        {
            get { return _TestProperty1; }
            set { _TestProperty1 = value; }
        }
        private int _TestProperty2 = 2;

        public int TestProperty2
        {
            get { return _TestProperty2; }
            set { _TestProperty2 = value; }
        }
        public TestObject2 TestObj2 = new TestObject2();
	 }
}
