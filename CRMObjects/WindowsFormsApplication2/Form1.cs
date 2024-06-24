using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using WindowsFormsApplication2.XMLTemp;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml;
using System.IO;


namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        XmlSerializer ser = new XmlSerializer(typeof(ReferenceOrderLine));
        ReferenceOrderLine myLine = new ReferenceOrderLine();

        private void Form1_Load(object sender, EventArgs e)
        {

            
            List<FlexField> ffl = new List<FlexField>();
            int x = 5;
            for (int i = 0; i < x; i++)
            {
                ffl.Add(GetFlexFieldList(i));
            }
            
            myLine.FlexFieldList = ffl;



            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            ser.Serialize(xw, myLine);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sw.ToString());
            doc.Save("c:\\CRMSvc\\test.xml");
            
            webBrowser1.Navigate("c:\\CRMSvc\\test.xml");
        }

        private FlexField GetFlexFieldList(int i)
        {
            
            FlexField ff = new FlexField();
            ff.Name = "Dummy " + i;
            ff.Value = i + "Count";
           
            return ff;
        }
    }
}

