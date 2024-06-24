using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;

namespace JGS.Web.DellTriggerProviders.Test
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void XmlInEditor_Scroll(object sender, ScrollEventArgs e)
        {
            if (XmlOutEditor.Text == string.Empty) { return; }
            if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                XmlOutEditor.Scrolling.ScrollBy(e.NewValue - e.OldValue, 0);
            }
            else
            {
                XmlOutEditor.Scrolling.ScrollBy(0, e.NewValue - e.OldValue);
            }
        }

        private void XmlOutEditor_Scroll(object sender, ScrollEventArgs e)
        {
            if (XmlInEditor.Text == string.Empty) { return; }
            if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                XmlInEditor.Scrolling.ScrollBy(e.NewValue - e.OldValue, 0);
            }
            else
            {
                XmlInEditor.Scrolling.ScrollBy(0, e.NewValue - e.OldValue);
            }
        }

        private void ChooseXmlInButton_Click(object sender, EventArgs e)
        {
            if (XmlInOpen.ShowDialog(this) == DialogResult.OK)
            {
                this.XmlInText.Text = XmlInOpen.FileName;
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            if (this.XmlInText.Text == string.Empty) { return; }
            XmlInEditor.IsReadOnly = false;
            XmlInEditor.Text = File.ReadAllText(this.XmlInText.Text);
            XmlInEditor.IsReadOnly = true;
            XmlOutEditor.IsReadOnly = false;
            XmlOutEditor.Text = string.Empty;
            XmlOutEditor.IsReadOnly = true;
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            if (this.XmlInText.Text == string.Empty) { return; }

            TriggerProviders.TriggerProviderBase trig;
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            if (this.TriggerCombo.SelectedItem.ToString() == "TRG_PXL_VALIDATIONS")
            {
                object obj = new TriggerProviders.TRG_PXL_VALIDATIONS();
                trig = (TriggerProviders.TriggerProviderBase)obj;
                trig.ConnectionString = new Properties.Settings().OracleConnectionString;
            }
            else if (this.TriggerCombo.SelectedItem.ToString() == "TRG_B2B_VALIDATIONS")
            {
                object obj = new TriggerProviders.TRG_B2B_VALIDATIONS();
                trig = (TriggerProviders.TriggerProviderBase)obj;
                trig.ConnectionString = new Properties.Settings().OracleConnectionString;
            }
            else if (this.TriggerCombo.SelectedItem.ToString() == "TRG_SET_FLEXFIELDS")
            {
                object obj = new TriggerProviders.TRG_SET_FLEXFIELDS();
                trig = (TriggerProviders.TriggerProviderBase)obj;
                trig.ConnectionString = new Properties.Settings().OracleConnectionString;
            }
            else if (this.TriggerCombo.SelectedItem.ToString() == "TRG_SCRAP_APPROVAL")
            {
                object obj = new TriggerProviders.TRG_SCRAP_APPROVAL();
                trig = (TriggerProviders.TriggerProviderBase)obj;
                trig.ConnectionString = new Properties.Settings().OracleConnectionString;
            }
            else if (this.TriggerCombo.SelectedItem.ToString() == "TRG_SCRAP_OEMWARR_UPD")
            {
                object obj = new TriggerProviders.TRG_SCRAP_OEMWARR_UPD();
                trig = (TriggerProviders.TriggerProviderBase)obj;
                trig.ConnectionString = new Properties.Settings().OracleConnectionString;
            }
            else if (this.TriggerCombo.SelectedItem.ToString() == "TRG_SCRAP_3FFS_UPD")
            {
                object obj = new TriggerProviders.TRG_SCRAP_3FFS_UPD();
                trig = (TriggerProviders.TriggerProviderBase)obj;
                trig.ConnectionString = new Properties.Settings().OracleConnectionString;
            }
            else if (this.TriggerCombo.SelectedItem.ToString() == "TRG_SCRAP_FA_VALIDATIONS")
            {
                object obj = new TriggerProviders.TRG_SCRAP_FA_VALIDATIONS();
                trig = (TriggerProviders.TriggerProviderBase)obj;
                trig.ConnectionString = new Properties.Settings().OracleConnectionString;
            }
            ///////// change part entries
            else if (this.TriggerCombo.SelectedItem.ToString() == "TRG_CP_PARTNUMSPECCHARCHECK")
            {
                object obj = new TriggerProviders.TRG_CP_PARTNUMSPECCHARCHECK();
                trig = (TriggerProviders.TriggerProviderBase)obj;
                trig.ConnectionString = new Properties.Settings().OracleConnectionString;
            }
            else if (this.TriggerCombo.SelectedItem.ToString() == "TRG_CP_PNNOTEQUALDELLPN")
            {
                object obj = new TriggerProviders.TRG_CP_PNNOTEQUALDELLPN();
                trig = (TriggerProviders.TriggerProviderBase)obj;
                trig.ConnectionString = new Properties.Settings().OracleConnectionString;
            }
            else if (this.TriggerCombo.SelectedItem.ToString() == "TRG_CP_PNISEQUALDELLPN")
            {
                object obj = new TriggerProviders.TRG_CP_PNISEQUALDELLPN();
                trig = (TriggerProviders.TriggerProviderBase)obj;
                trig.ConnectionString = new Properties.Settings().OracleConnectionString;
            }
            else if (this.TriggerCombo.SelectedItem.ToString() == "TRG_CP_UPDATE_FFS_XML")
            {
                object obj = new TriggerProviders.TRG_CP_UPDATE_FFS_XML();
                trig = (TriggerProviders.TriggerProviderBase)obj;
                trig.ConnectionString = new Properties.Settings().OracleConnectionString;
            }
            else if (this.TriggerCombo.SelectedItem.ToString() == "TRG_SCRAP_OEMWARR_UPD_XML")
            {
                object obj = new TriggerProviders.TRG_SCRAP_OEMWARR_UPD_XML();
                trig = (TriggerProviders.TriggerProviderBase)obj;
                trig.ConnectionString = new Properties.Settings().OracleConnectionString;
            }
            else if (this.TriggerCombo.SelectedItem.ToString() == "TRG_CP_UPDATE_FFS_XML_WC")
            {
                object obj = new TriggerProviders.TRG_CP_UPDATE_FFS_XML_WC();
                trig = (TriggerProviders.TriggerProviderBase)obj;
                trig.ConnectionString = new Properties.Settings().OracleConnectionString;
            }
            else
            {
                return;
            }
            XmlDocument xmlIn = new XmlDocument();
            xmlIn.PreserveWhitespace = true;
            xmlIn.Load(XmlInText.Text);

            XmlDocument xmlOut = trig.Execute(xmlIn);

            XmlOutEditor.IsReadOnly = false;
            XmlOutEditor.Text = xmlOut.InnerXml;
            XmlOutEditor.IsReadOnly = true;
        }
    }
}

