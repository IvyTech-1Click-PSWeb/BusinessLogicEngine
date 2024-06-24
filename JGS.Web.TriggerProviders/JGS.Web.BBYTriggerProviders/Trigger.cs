using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trigger
{



    /// <summary>
    /// Defines Trigger Information internal structure
    /// </summary>
    /// <remarks></remarks>
    public class Trigger
    {
        public Trigger(System.Xml.XmlDocument XML)
        {
            this.LoadXML(XML);
        }




        protected TriggerHeader _hdrHeader = new TriggerHeader();
        public TriggerHeader Header
        {
            get { return _hdrHeader; }
            set { _hdrHeader = value; }
        }

        protected TriggerDetails _detDetail = new TriggerDetails();
        public TriggerDetails Detail
        {
            get { return _detDetail; }
            set { _detDetail = value; }
        }


        private void LoadXML(System.Xml.XmlDocument XML)
        {
            System.Xml.XmlNode MainNode = XML.SelectNodes("/Trigger").Item(0);


            for (int x = 0; x <= MainNode.ChildNodes.Count - 1; x++)
            {
                switch (MainNode.ChildNodes[x].Name)
                {
                    case "Header":
                        System.Xml.XmlNode HeaderNode = MainNode.ChildNodes[x];
                        foreach (System.Xml.XmlNode HeaderNodeVal in HeaderNode.ChildNodes)
                        {
                            switch (HeaderNodeVal.Name)
                            {
                                case "LocationID":
                                    this._hdrHeader.LocationID = int.Parse(HeaderNodeVal.InnerText);
                                    break;
                                case "ClientID":
                                    this._hdrHeader.ClientID = int.Parse(HeaderNodeVal.InnerText);
                                    break;
                                case "ContractID":
                                    this._hdrHeader.ContractID = int.Parse(HeaderNodeVal.InnerText);
                                    break;
                                case "TriggerType":
                                    this._hdrHeader.TriggerType = HeaderNodeVal.InnerText;
                                    break;
                                case "UserObj":
                                    System.Xml.XmlNode HeaderUserObj = HeaderNodeVal;
                                    foreach (System.Xml.XmlNode UserObjVal in HeaderUserObj)
                                    {
                                        switch (UserObjVal.Name)
                                        {
                                            case "UserName":
                                                this._hdrHeader.UserObj.Username = UserObjVal.InnerText;
                                                break;
                                            case "PassWord":
                                                this._hdrHeader.UserObj.Password = UserObjVal.InnerText;
                                                break;
                                        }
                                    }

                                    break;
                            }
                        }

                        break;
                    case "Detail":
                        System.Xml.XmlNode DetailNode = MainNode.ChildNodes[x];
                        foreach (System.Xml.XmlNode DetailNodeVal in DetailNode.ChildNodes)
                        {
                            switch (DetailNodeVal.Name)
                            {
                                case "ItemLevel":
                                    System.Xml.XmlNode ItemLevelNode = DetailNodeVal;
                                    foreach (System.Xml.XmlNode ItemLevelNodeVal in ItemLevelNode.ChildNodes)
                                    {
                                        switch (ItemLevelNodeVal.Name)
                                        {
                                            case "OrderProcessType":
                                                this._detDetail.ItemLevel.OPT = ItemLevelNodeVal.InnerText;
                                                break;
                                            case "OrderProcessTypeID":
                                                this._detDetail.ItemLevel.OPTID = int.Parse(ItemLevelNodeVal.InnerText);
                                                break;
                                            case "WorkCenter":
                                                this._detDetail.ItemLevel.Workcenter = ItemLevelNodeVal.InnerText;
                                                break;
                                            case "WorkCenterID":
                                                this._detDetail.ItemLevel.WorkcenterID = int.Parse(ItemLevelNodeVal.InnerText);
                                                break;
                                            case "BCN":
                                                this._detDetail.ItemLevel.BCN = ItemLevelNodeVal.InnerText;
                                                break;
                                            case "SerialNumber":
                                                this._detDetail.ItemLevel.SerialNumber = ItemLevelNodeVal.InnerText;
                                                break;
                                            case "ItemID":
                                                this._detDetail.ItemLevel.ItemID = int.Parse(ItemLevelNodeVal.InnerText);
                                                break;
                                            case "FixedAssetTag":
                                                this._detDetail.ItemLevel.FixedAssetTag = ItemLevelNodeVal.InnerText;
                                                break;
                                            case "PartNo":
                                                this._detDetail.ItemLevel.PartNumber = ItemLevelNodeVal.InnerText;
                                                break;
                                            case "WorkOrderID":
                                                this._detDetail.ItemLevel.WorkOrderID = int.Parse(ItemLevelNodeVal.InnerText);
                                                break;
                                            case "ItemFlexField":

                                                break;
                                        }
                                    }

                                    break;
                                case "TimeOut":
                                    System.Xml.XmlNode TimeOutNode = DetailNodeVal;
                                    foreach (System.Xml.XmlNode TimeOutNodeVal in TimeOutNode.ChildNodes)
                                    {
                                        switch (TimeOutNodeVal.Name)
                                        {
                                            case "WCFlexFields":
                                                break;
                                            case "DiagnosticCodeList":
                                                System.Xml.XmlNode DiagnosticCodeListNode = TimeOutNodeVal;
                                                foreach (System.Xml.XmlNode DiagnosticCodeListVal in DiagnosticCodeListNode.ChildNodes)
                                                {
                                                    switch (DiagnosticCodeListVal.Name)
                                                    {
                                                        case "DiagnosticCode":
                                                            OutcomeCode DiagnosticCode = new OutcomeCode();

                                                            System.Xml.XmlNode DiagnosticCodeNode = DiagnosticCodeListVal;
                                                            foreach (System.Xml.XmlNode DiagnosticCodeVal in DiagnosticCodeListNode.ChildNodes)
                                                            {
                                                                switch (DiagnosticCodeVal.Name)
                                                                {
                                                                    case "Name":
                                                                        DiagnosticCode.Name = DiagnosticCodeVal.InnerText;
                                                                        break;
                                                                    case "Value":
                                                                        DiagnosticCode.Value = DiagnosticCodeVal.InnerText;
                                                                        break;
                                                                }
                                                            }

                                                            this._detDetail.TimeOut.DiagnosticCodeList.Add(DiagnosticCode);
                                                            break;
                                                    }
                                                }

                                                break;
                                            case "SymptomCodeList":
                                                System.Xml.XmlNode SymptomCodeListNode = TimeOutNodeVal;
                                                foreach (System.Xml.XmlNode SymptomCodeListVal in SymptomCodeListNode.ChildNodes)
                                                {
                                                    switch (SymptomCodeListVal.Name)
                                                    {
                                                        case "SymptomCode":
                                                            OutcomeCode SymptomCode = new OutcomeCode();

                                                            System.Xml.XmlNode SymptomCodeNode = SymptomCodeListVal;
                                                            foreach (System.Xml.XmlNode SymptomCodeVal in SymptomCodeListNode.ChildNodes)
                                                            {
                                                                switch (SymptomCodeVal.Name)
                                                                {
                                                                    case "Name":
                                                                        SymptomCode.Name = SymptomCodeVal.InnerText;
                                                                        break;
                                                                    case "Value":
                                                                        SymptomCode.Value = SymptomCodeVal.InnerText;
                                                                        break;
                                                                }
                                                            }

                                                            this._detDetail.TimeOut.SymptomCodeList.Add(SymptomCode);
                                                            break;
                                                    }
                                                }

                                                break;
                                            case "FailureCodeList":
                                                System.Xml.XmlNode FailureCodeListNode = TimeOutNodeVal;
                                                foreach (System.Xml.XmlNode FailureCodeListVal in FailureCodeListNode.ChildNodes)
                                                {
                                                    switch (FailureCodeListVal.Name)
                                                    {
                                                        case "FailureCode":
                                                            OutcomeCode FailureCode = new OutcomeCode();

                                                            System.Xml.XmlNode FailureCodeNode = FailureCodeListVal;
                                                            foreach (System.Xml.XmlNode FailureCodeVal in FailureCodeListNode.ChildNodes)
                                                            {
                                                                switch (FailureCodeVal.Name)
                                                                {
                                                                    case "Name":
                                                                        FailureCode.Name = FailureCodeVal.InnerText;
                                                                        break;
                                                                    case "Value":
                                                                        FailureCode.Value = FailureCodeVal.InnerText;
                                                                        break;
                                                                }
                                                            }

                                                            this._detDetail.TimeOut.FailureCodeList.Add(FailureCode);
                                                            break;
                                                    }
                                                }

                                                break;
                                            case "RepairCodeList":
                                                System.Xml.XmlNode RepairCodeListNode = TimeOutNodeVal;
                                                foreach (System.Xml.XmlNode RepairCodeListVal in RepairCodeListNode.ChildNodes)
                                                {
                                                    switch (RepairCodeListVal.Name)
                                                    {
                                                        case "RepairCode":
                                                            OutcomeCode RepairCode = new OutcomeCode();

                                                            System.Xml.XmlNode RepairCodeNode = RepairCodeListVal;
                                                            foreach (System.Xml.XmlNode RepairCodeVal in RepairCodeListNode.ChildNodes)
                                                            {
                                                                switch (RepairCodeVal.Name)
                                                                {
                                                                    case "Name":
                                                                        RepairCode.Name = RepairCodeVal.InnerText;
                                                                        break;
                                                                    case "Value":
                                                                        RepairCode.Value = RepairCodeVal.InnerText;
                                                                        break;
                                                                }
                                                            }

                                                            this._detDetail.TimeOut.RepairCodeList.Add(RepairCode);
                                                            break;
                                                    }
                                                }

                                                break;
                                            case "ResultCode":
                                                this._detDetail.TimeOut.ResultCode = TimeOutNodeVal.InnerText;
                                                break;
                                            case "Notes":
                                                this._detDetail.TimeOut.Notes = TimeOutNodeVal.InnerText;
                                                break;
                                        }
                                    }

                                    break;
                                case "FailureAnalysis":
                                    System.Xml.XmlNode FANode = DetailNodeVal;
                                    foreach (System.Xml.XmlNode FANodeVal in FANode.ChildNodes)
                                    {
                                        switch (FANodeVal.Name)
                                        {
                                            case "DefectCodeList":
                                                System.Xml.XmlNode DefectCodeListNode = FANodeVal;
                                                foreach (System.Xml.XmlNode DefectCodeNode in DefectCodeListNode.ChildNodes)
                                                {
                                                    FADefectCode DefectCode = new FADefectCode();
                                                    foreach (System.Xml.XmlNode DefectCodeVal in DefectCodeNode.ChildNodes)
                                                    {
                                                        switch (DefectCodeVal.Name)
                                                        {
                                                            case "DefectCodeName":
                                                                DefectCode.DefectCodeName = DefectCodeVal.InnerText;
                                                                break;
                                                            case "FAFlexFieldList":
                                                                System.Xml.XmlNode DefectCodeFFListNode = DefectCodeVal;
                                                                foreach (System.Xml.XmlNode FFNode in DefectCodeFFListNode.ChildNodes)
                                                                {
                                                                    FlexField DefectCodeFF = new FlexField();
                                                                    foreach (System.Xml.XmlNode FFNodeVal in FFNode.ChildNodes)
                                                                    {
                                                                        switch (FFNodeVal.Name)
                                                                        {
                                                                            case "Name":
                                                                                DefectCodeFF.Name = FFNodeVal.InnerText;
                                                                                break;
                                                                            case "Value":
                                                                                DefectCodeFF.Value = FFNodeVal.InnerText;
                                                                                break;
                                                                        }
                                                                    }
                                                                    DefectCode.FlexField.Add(DefectCodeFF);
                                                                }

                                                                break;
                                                            case "ActionCodeList":
                                                                System.Xml.XmlNode ActionCodeListNode = DefectCodeVal;
                                                                foreach (System.Xml.XmlNode ActionCodeNode in ActionCodeListNode.ChildNodes)
                                                                {
                                                                    FAActionCode ActionCode = new FAActionCode();
                                                                    foreach (System.Xml.XmlNode ActionCodeVal in ActionCodeNode)
                                                                    {
                                                                        switch (ActionCodeVal.Name)
                                                                        {
                                                                            case "ActionCodeName":
                                                                                ActionCode.ActionCodeName = ActionCodeVal.InnerText;
                                                                                break;
                                                                            case "AssemblyCode":
                                                                                ActionCode.AssemblyCode = ActionCodeVal.InnerText;
                                                                                break;
                                                                            case "FAFlexFieldList":
                                                                                System.Xml.XmlNode ActionCodeFFListNode = ActionCodeVal;
                                                                                foreach (System.Xml.XmlNode FFNode in ActionCodeFFListNode.ChildNodes)
                                                                                {
                                                                                    FlexField ActionCodeFF = new FlexField();
                                                                                    foreach (System.Xml.XmlNode FFNodeVal in FFNode.ChildNodes)
                                                                                    {
                                                                                        switch (FFNodeVal.Name)
                                                                                        {
                                                                                            case "Name":
                                                                                                ActionCodeFF.Name = FFNodeVal.InnerText;
                                                                                                break;
                                                                                            case "Value":
                                                                                                ActionCodeFF.Value = FFNodeVal.InnerText;
                                                                                                break;
                                                                                        }
                                                                                    }
                                                                                    ActionCode.FlexField.Add(ActionCodeFF);
                                                                                }

                                                                                break;
                                                                            case "ComponentCodeList":
                                                                                System.Xml.XmlNode ComponentCodeListNode = ActionCodeVal;
                                                                                foreach (System.Xml.XmlNode ComponentCodeListVal in ComponentCodeListNode)
                                                                                {
                                                                                    switch (ComponentCodeListVal.Name)
                                                                                    {
                                                                                        case "NewList":
                                                                                            System.Xml.XmlNode NewListNode = ComponentCodeListVal;
                                                                                            foreach (System.Xml.XmlNode ComponentNode in NewListNode.ChildNodes)
                                                                                            {
                                                                                                Component Component = new Component();
                                                                                                foreach (System.Xml.XmlNode ComponentVal in ComponentNode.ChildNodes)
                                                                                                {
                                                                                                    switch (ComponentVal.Name)
                                                                                                    {
                                                                                                        case "ComponentPartNo":
                                                                                                            Component.PartNumber = ComponentVal.InnerText;
                                                                                                            break;
                                                                                                        case "ComponentLocation":
                                                                                                            Component.Location = ComponentVal.InnerText;
                                                                                                            break;
                                                                                                        case "ManuFacturerPart":
                                                                                                            Component.ManufacturerPartNumber = ComponentVal.InnerText;
                                                                                                            break;
                                                                                                        case "Description":
                                                                                                            Component.Description = ComponentVal.InnerText;
                                                                                                            break;
                                                                                                        case "FAFlexFieldList":
                                                                                                            System.Xml.XmlNode ComponentFFListNode = ComponentVal;
                                                                                                            foreach (System.Xml.XmlNode FFNode in ComponentFFListNode.ChildNodes)
                                                                                                            {
                                                                                                                FlexField ComponentFF = new FlexField();
                                                                                                                foreach (System.Xml.XmlNode FFNodeVal in FFNode.ChildNodes)
                                                                                                                {
                                                                                                                    switch (FFNodeVal.Name)
                                                                                                                    {
                                                                                                                        case "Name":
                                                                                                                            ComponentFF.Name = FFNodeVal.InnerText;
                                                                                                                            break;
                                                                                                                        case "Value":
                                                                                                                            ComponentFF.Value = FFNodeVal.InnerText;
                                                                                                                            break;
                                                                                                                    }
                                                                                                                }
                                                                                                                Component.FlexField.Add(ComponentFF);
                                                                                                            }

                                                                                                            break;
                                                                                                    }
                                                                                                }
                                                                                                ActionCode.ComponentCodeList.NewComponents.Add(Component);
                                                                                            }

                                                                                            break;
                                                                                        case "DefectiveList":
                                                                                            System.Xml.XmlNode DefectiveListNode = ComponentCodeListVal;
                                                                                            foreach (System.Xml.XmlNode ComponentNode in DefectiveListNode.ChildNodes)
                                                                                            {
                                                                                                Component Component = new Component();
                                                                                                foreach (System.Xml.XmlNode ComponentVal in ComponentNode.ChildNodes)
                                                                                                {
                                                                                                    switch (ComponentVal.Name)
                                                                                                    {
                                                                                                        case "ComponentPartNo":
                                                                                                            Component.PartNumber = ComponentVal.InnerText;
                                                                                                            break;
                                                                                                        case "ComponentLocation":
                                                                                                            Component.Location = ComponentVal.InnerText;
                                                                                                            break;
                                                                                                        case "ManuFacturerPart":
                                                                                                            Component.ManufacturerPartNumber = ComponentVal.InnerText;
                                                                                                            break;
                                                                                                        case "Description":
                                                                                                            Component.Description = ComponentVal.InnerText;
                                                                                                            break;
                                                                                                        case "FAFlexFieldList":
                                                                                                            System.Xml.XmlNode ComponentFFListNode = ComponentVal;
                                                                                                            foreach (System.Xml.XmlNode FFNode in ComponentFFListNode.ChildNodes)
                                                                                                            {
                                                                                                                FlexField ComponentFF = new FlexField();
                                                                                                                foreach (System.Xml.XmlNode FFNodeVal in FFNode.ChildNodes)
                                                                                                                {
                                                                                                                    switch (FFNodeVal.Name)
                                                                                                                    {
                                                                                                                        case "Name":
                                                                                                                            ComponentFF.Name = FFNodeVal.InnerText;
                                                                                                                            break;
                                                                                                                        case "Value":
                                                                                                                            ComponentFF.Value = FFNodeVal.InnerText;
                                                                                                                            break;
                                                                                                                    }
                                                                                                                }
                                                                                                                Component.FlexField.Add(ComponentFF);
                                                                                                            }

                                                                                                            break;
                                                                                                    }
                                                                                                }
                                                                                                ActionCode.ComponentCodeList.DefectiveComponents.Add(Component);
                                                                                            }

                                                                                            break;
                                                                                    }
                                                                                }

                                                                                break;
                                                                        }
                                                                    }
                                                                    DefectCode.ActionCodeList.Add(ActionCode);
                                                                }

                                                                break;
                                                        }
                                                    }

                                                    this._detDetail.FailureAnalysis.DefectCodeList.Add(DefectCode);
                                                }

                                                break;
                                        }
                                    }

                                    break;
                                case "ChangePart":
                                    System.Xml.XmlNode ChangePartNode = DetailNodeVal;
                                    foreach (System.Xml.XmlNode ChangePartVal in ChangePartNode.ChildNodes)
                                    {
                                        switch (ChangePartVal.Name)
                                        {
                                            case "NewPartNo":
                                                this._detDetail.ChangePart.NewPartNo = ChangePartVal.InnerText;
                                                break;
                                            case "NewSerialNo":
                                                this._detDetail.ChangePart.NewSerialNo = ChangePartVal.InnerText;
                                                break;
                                            case "NewRevisionLevel":
                                                this._detDetail.ChangePart.NewRevisionLevel = ChangePartVal.InnerText;
                                                break;
                                            case "NewFixedAssetTag":
                                                this._detDetail.ChangePart.NewFixedAssetTag = ChangePartVal.InnerText;
                                                break;
                                        }
                                    }


                                    break;
                                case "TriggerResult":
                                    System.Xml.XmlNode TriggerResultNode = DetailNodeVal;
                                    foreach (System.Xml.XmlNode TriggerResultVal in TriggerResultNode)
                                    {
                                        switch (TriggerResultVal.Name)
                                        {
                                            case "Result":
                                                this._detDetail.TriggerResult.Result = TriggerResultVal.InnerText;
                                                break;
                                            case "Message":
                                                this._detDetail.TriggerResult.Message = TriggerResultVal.InnerText;
                                                break;
                                        }
                                    }

                                    break;
                            }
                        }

                        break;
                }
            }
        }

        public System.Xml.XmlDocument toXML()
        {
            System.Xml.XmlDocument Res = new System.Xml.XmlDocument();

            System.IO.MemoryStream mStream = new System.IO.MemoryStream();
            System.IO.StringWriter Outputstring = new System.IO.StringWriter();
            System.Xml.XmlTextWriter myXmlTextWriter = new System.Xml.XmlTextWriter(mStream, System.Text.Encoding.UTF8);
            string Result = null;

            //TriggerStart
            myXmlTextWriter.WriteStartElement("Trigger");


            //HeaderStart
            myXmlTextWriter.WriteStartElement("Header");

            myXmlTextWriter.WriteElementString("LocationID", this._hdrHeader.LocationID.ToString());
            myXmlTextWriter.WriteElementString("ClientID", this._hdrHeader.ClientID.ToString());
            myXmlTextWriter.WriteElementString("ContractID", this._hdrHeader.ContractID.ToString());
            myXmlTextWriter.WriteElementString("TriggerType", this._hdrHeader.TriggerType);
            //UserObjStart
            myXmlTextWriter.WriteStartElement("UserObj");
            myXmlTextWriter.WriteElementString("UserName", this._hdrHeader.UserObj.Username);
            myXmlTextWriter.WriteElementString("Password", this._hdrHeader.UserObj.Password);
            //UserObjEnd
            myXmlTextWriter.WriteEndElement();

            //HeaderEnd
            myXmlTextWriter.WriteEndElement();


            //DetailStart
            myXmlTextWriter.WriteStartElement("Detail");

            //ItemLevelStart
            myXmlTextWriter.WriteStartElement("ItemLevel");

            myXmlTextWriter.WriteElementString("OrderProcessType", this._detDetail.ItemLevel.OPT);
            myXmlTextWriter.WriteElementString("OrderProcessTypeID", this._detDetail.ItemLevel.OPTID.ToString());
            myXmlTextWriter.WriteElementString("WorkCenter", this._detDetail.ItemLevel.Workcenter);
            myXmlTextWriter.WriteElementString("WorkCenterID", this._detDetail.ItemLevel.WorkcenterID.ToString());
            myXmlTextWriter.WriteElementString("BCN", this._detDetail.ItemLevel.BCN);
            myXmlTextWriter.WriteElementString("SerialNumber", this._detDetail.ItemLevel.SerialNumber);
            myXmlTextWriter.WriteElementString("ItemID", this._detDetail.ItemLevel.ItemID.ToString());
            myXmlTextWriter.WriteElementString("FixedAssetTag", this._detDetail.ItemLevel.FixedAssetTag);
            myXmlTextWriter.WriteElementString("PartNo", this._detDetail.ItemLevel.PartNumber);
            myXmlTextWriter.WriteElementString("WorkOrderID", this._detDetail.ItemLevel.WorkOrderID.ToString());

            //ItemFlexFieldStart
            myXmlTextWriter.WriteStartElement("ItemFlexField");


            for (int x = 0; x <= this._detDetail.ItemLevel.ItemFlexField.Count - 1; x++)
            {
                myXmlTextWriter.WriteStartElement("FlexField");
                myXmlTextWriter.WriteElementString("Name", this._detDetail.ItemLevel.ItemFlexField[x].Name);
                myXmlTextWriter.WriteElementString("Value", this._detDetail.ItemLevel.ItemFlexField[x].Value);
                myXmlTextWriter.WriteEndElement();
            }

            //ItemFlexFieldEnd
            myXmlTextWriter.WriteEndElement();

            //ItemLevelEnd
            myXmlTextWriter.WriteEndElement();





            //TimeOutStart
            myXmlTextWriter.WriteStartElement("TimeOut");

            //WCFlexFieldStart
            myXmlTextWriter.WriteStartElement("WCFlexFields");

            for (int x = 0; x <= this._detDetail.ItemLevel.ItemFlexField.Count - 1; x++)
            {
                myXmlTextWriter.WriteStartElement("FlexField");
                myXmlTextWriter.WriteElementString("Name", this._detDetail.ItemLevel.ItemFlexField[x].Name);
                myXmlTextWriter.WriteElementString("Value", this._detDetail.ItemLevel.ItemFlexField[x].Value);
                myXmlTextWriter.WriteEndElement();
            }
            //WCFlexFieldEnd
            myXmlTextWriter.WriteEndElement();

            //DiagnosticCodeListStart
            myXmlTextWriter.WriteStartElement("DiagnosticCodeList");

            for (int x = 0; x <= this._detDetail.TimeOut.DiagnosticCodeList.Count - 1; x++)
            {
                myXmlTextWriter.WriteStartElement("DiagnosticCode");
                myXmlTextWriter.WriteElementString("Name", this._detDetail.TimeOut.DiagnosticCodeList[x].Name);
                myXmlTextWriter.WriteElementString("Value", this._detDetail.TimeOut.DiagnosticCodeList[x].Value);
                myXmlTextWriter.WriteEndElement();
            }
            //DiagnosticCodeListEnd
            myXmlTextWriter.WriteEndElement();

            //SymptomCodeListStart
            myXmlTextWriter.WriteStartElement("SymptomCodeList");

            for (int x = 0; x <= this._detDetail.TimeOut.SymptomCodeList.Count - 1; x++)
            {
                myXmlTextWriter.WriteStartElement("SymptomCode");
                myXmlTextWriter.WriteElementString("Name", this._detDetail.TimeOut.SymptomCodeList[x].Name);
                myXmlTextWriter.WriteElementString("Value", this._detDetail.TimeOut.SymptomCodeList[x].Value);
                myXmlTextWriter.WriteEndElement();
            }
            //SymptomCodeListEnd
            myXmlTextWriter.WriteEndElement();

            //FailureCodeListStart
            myXmlTextWriter.WriteStartElement("FailureCodeList");

            for (int x = 0; x <= this._detDetail.TimeOut.FailureCodeList.Count - 1; x++)
            {
                myXmlTextWriter.WriteStartElement("FailureCode");
                myXmlTextWriter.WriteElementString("Name", this._detDetail.TimeOut.FailureCodeList[x].Name);
                myXmlTextWriter.WriteElementString("Value", this._detDetail.TimeOut.FailureCodeList[x].Value);
                myXmlTextWriter.WriteEndElement();
            }
            //FailureCodeListEnd
            myXmlTextWriter.WriteEndElement();

            //RepairCodeListStart
            myXmlTextWriter.WriteStartElement("RepairCodeList");

            for (int x = 0; x <= this._detDetail.TimeOut.RepairCodeList.Count - 1; x++)
            {
                myXmlTextWriter.WriteStartElement("RepairCode");
                myXmlTextWriter.WriteElementString("Name", this._detDetail.TimeOut.RepairCodeList[x].Name);
                myXmlTextWriter.WriteElementString("Value", this._detDetail.TimeOut.RepairCodeList[x].Value);
                myXmlTextWriter.WriteEndElement();
            }
            //RepairCodeListEnd
            myXmlTextWriter.WriteEndElement();

            myXmlTextWriter.WriteElementString("ResultCode", this._detDetail.TimeOut.ResultCode);
            myXmlTextWriter.WriteElementString("Notes", this._detDetail.TimeOut.Notes);

            //TimeOutEnd
            myXmlTextWriter.WriteEndElement();





            //FailureAnalysisStart
            myXmlTextWriter.WriteStartElement("FailureAnalysis");

            //DefectCodeListStart
            myXmlTextWriter.WriteStartElement("DefectCodeList");
            for (int dc = 0; dc <= this._detDetail.FailureAnalysis.DefectCodeList.Count - 1; dc++)
            {
                myXmlTextWriter.WriteStartElement("DefectCode");
                myXmlTextWriter.WriteStartElement("FAFlexFieldList");
                for (int x = 0; x <= this._detDetail.FailureAnalysis.DefectCodeList[dc].FlexField.Count - 1; x++)
                {
                    myXmlTextWriter.WriteStartElement("FlexField");
                    myXmlTextWriter.WriteElementString("Name", this._detDetail.FailureAnalysis.DefectCodeList[dc].FlexField[x].Name);
                    myXmlTextWriter.WriteElementString("Value", this._detDetail.FailureAnalysis.DefectCodeList[dc].FlexField[x].Value);
                    myXmlTextWriter.WriteEndElement();
                }
                myXmlTextWriter.WriteEndElement();
                for (int ac = 0; ac <= this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList.Count - 1; ac++)
                {
                    myXmlTextWriter.WriteStartElement("ActionCode");
                    myXmlTextWriter.WriteStartElement("FAFlexFieldList");
                    for (int x = 0; x <= this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].FlexField.Count - 1; x++)
                    {
                        myXmlTextWriter.WriteStartElement("FlexField");
                        myXmlTextWriter.WriteElementString("Name", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].FlexField[x].Name);
                        myXmlTextWriter.WriteElementString("Value", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].FlexField[x].Value);
                        myXmlTextWriter.WriteEndElement();
                    }
                    myXmlTextWriter.WriteEndElement();
                    myXmlTextWriter.WriteElementString("ActionCodeName", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ActionCodeName);
                    myXmlTextWriter.WriteElementString("AssemblyCode", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].AssemblyCode);
                    myXmlTextWriter.WriteStartElement("ComponentCodeList");
                    myXmlTextWriter.WriteStartElement("NewList");
                    for (int Comp = 0; Comp <= this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.NewComponents.Count - 1; Comp++)
                    {
                        myXmlTextWriter.WriteStartElement("Component");
                        myXmlTextWriter.WriteElementString("ComponentPartNo", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.NewComponents[Comp].PartNumber);
                        myXmlTextWriter.WriteElementString("ComponentLocation", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.NewComponents[Comp].Location);
                        myXmlTextWriter.WriteElementString("ManuFacturerPart", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.NewComponents[Comp].ManufacturerPartNumber);
                        myXmlTextWriter.WriteElementString("Description", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.NewComponents[Comp].Description);
                        myXmlTextWriter.WriteStartElement("FAFlexFieldList");
                        for (int x = 0; x <= this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.NewComponents[Comp].FlexField.Count - 1; x++)
                        {
                            myXmlTextWriter.WriteStartElement("FlexField");
                            myXmlTextWriter.WriteElementString("Name", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.NewComponents[Comp].FlexField[x].Name);
                            myXmlTextWriter.WriteElementString("Value", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.NewComponents[Comp].FlexField[x].Value);
                            myXmlTextWriter.WriteEndElement();
                        }
                        myXmlTextWriter.WriteEndElement();
                        myXmlTextWriter.WriteEndElement();
                    }
                    myXmlTextWriter.WriteEndElement();
                    myXmlTextWriter.WriteStartElement("DefectiveList");
                    for (int Comp = 0; Comp <= this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.DefectiveComponents.Count - 1; Comp++)
                    {
                        myXmlTextWriter.WriteStartElement("Component");
                        myXmlTextWriter.WriteElementString("ComponentPartNo", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.DefectiveComponents[Comp].PartNumber);
                        myXmlTextWriter.WriteElementString("ComponentLocation", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.DefectiveComponents[Comp].Location);
                        myXmlTextWriter.WriteElementString("ManuFacturerPart", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.DefectiveComponents[Comp].ManufacturerPartNumber);
                        myXmlTextWriter.WriteElementString("Description", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.DefectiveComponents[Comp].Description);
                        myXmlTextWriter.WriteStartElement("FAFlexFieldList");
                        for (int x = 0; x <= this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.DefectiveComponents[Comp].FlexField.Count - 1; x++)
                        {
                            myXmlTextWriter.WriteStartElement("FlexField");
                            myXmlTextWriter.WriteElementString("Name", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.DefectiveComponents[Comp].FlexField[x].Name);
                            myXmlTextWriter.WriteElementString("Value", this._detDetail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.DefectiveComponents[Comp].FlexField[x].Value);
                            myXmlTextWriter.WriteEndElement();
                        }
                        myXmlTextWriter.WriteEndElement();
                        myXmlTextWriter.WriteEndElement();
                    }
                    myXmlTextWriter.WriteEndElement();
                    myXmlTextWriter.WriteEndElement();
                    myXmlTextWriter.WriteEndElement();
                }

                myXmlTextWriter.WriteEndElement();
            }

            //DefectCodeListStart
            myXmlTextWriter.WriteEndElement();

            //FailureAnalysisEnd
            myXmlTextWriter.WriteEndElement();




            //ChangePartStart
            myXmlTextWriter.WriteStartElement("ChangePart");
            myXmlTextWriter.WriteElementString("NewPartNo", this._detDetail.ChangePart.NewPartNo);
            myXmlTextWriter.WriteElementString("NewSerialNo", this._detDetail.ChangePart.NewSerialNo);
            myXmlTextWriter.WriteElementString("NewRevisionLevel", this._detDetail.ChangePart.NewRevisionLevel);
            myXmlTextWriter.WriteElementString("NewFixedAssetTag", this._detDetail.ChangePart.NewFixedAssetTag);
            //ChangePartEnd
            myXmlTextWriter.WriteEndElement();




            //TriggerResultStart
            myXmlTextWriter.WriteStartElement("TriggerResult");
            myXmlTextWriter.WriteElementString("Result", this._detDetail.TriggerResult.Result);
            myXmlTextWriter.WriteElementString("Message", this._detDetail.TriggerResult.Message);
            //TriggerResultEnd
            myXmlTextWriter.WriteEndElement();


            //DetailEnd
            myXmlTextWriter.WriteEndElement();


            //TriggerEnd
            myXmlTextWriter.WriteEndElement();
            myXmlTextWriter.Flush();

            Result = System.Text.Encoding.UTF8.GetString(mStream.ToArray());
            myXmlTextWriter.Close();

            Res.LoadXml(Result.Remove(0, 1));
            return Res;
        }

    }

    public class TriggerHeader
    {
        protected int _IntLocationID = 0;
        public int LocationID
        {
            get { return _IntLocationID; }
            set { _IntLocationID = value; }
        }
        protected int _IntClientID = 0;
        public int ClientID
        {
            get { return _IntClientID; }
            set { _IntClientID = value; }
        }
        protected int _IntContractID = 0;
        public int ContractID
        {
            get { return _IntContractID; }
            set { _IntContractID = value; }
        }
        protected string _strTriggerType = string.Empty;
        public string TriggerType
        {
            get { return _strTriggerType; }
            set { _strTriggerType = value; }
        }

        protected UserObj _uoUserObj = new UserObj();
        public UserObj UserObj
        {
            get { return _uoUserObj; }
            set { _uoUserObj = value; }
        }

    }


    public class UserObj
    {

        protected string _strUsername = string.Empty;
        public string Username
        {
            get { return _strUsername; }
            set { _strUsername = value; }
        }

        protected string _strPassword = string.Empty;
        public string Password
        {
            get { return _strPassword; }
            set { _strPassword = value; }
        }
    }

    public class TriggerDetails
    {
        protected DetailItemLevel _ilItemLevel = new DetailItemLevel();
        public DetailItemLevel ItemLevel
        {
            get { return _ilItemLevel; }
            set { _ilItemLevel = value; }
        }

        protected DetailTimeOut _toTimeOut = new DetailTimeOut();
        public DetailTimeOut TimeOut
        {
            get { return _toTimeOut; }
            set { _toTimeOut = value; }
        }

        protected DetailFailureAnalysis _faFailureAnalysis = new DetailFailureAnalysis();
        public DetailFailureAnalysis FailureAnalysis
        {
            get { return _faFailureAnalysis; }
            set { _faFailureAnalysis = value; }
        }

        protected DetailChangePart _cpChangePart = new DetailChangePart();
        public DetailChangePart ChangePart
        {
            get { return _cpChangePart; }
            set { _cpChangePart = value; }
        }

        protected DetailTriggerResult _trTriggerResult = new DetailTriggerResult();
        public DetailTriggerResult TriggerResult
        {
            get { return _trTriggerResult; }
            set { _trTriggerResult = value; }
        }

    }

    public class DetailItemLevel
    {
        protected string _strOPT = string.Empty;
        public string OPT
        {
            get { return _strOPT; }
            set { _strOPT = value; }
        }
        protected int _IntOPTID = 0;
        public int OPTID
        {
            get { return _IntOPTID; }
            set { _IntOPTID = value; }
        }

        protected string _strWorkcenter = string.Empty;
        public string Workcenter
        {
            get { return _strWorkcenter; }
            set { _strWorkcenter = value; }
        }
        protected int _IntWorkcenterID = 0;
        public int WorkcenterID
        {
            get { return _IntWorkcenterID; }
            set { _IntWorkcenterID = value; }
        }

        protected string _strBCN = string.Empty;
        public string BCN
        {
            get { return _strBCN; }
            set { _strBCN = value; }
        }
        protected string _strSerialNumber = string.Empty;
        public string SerialNumber
        {
            get { return _strSerialNumber; }
            set { _strSerialNumber = value; }
        }
        protected int _IntItemID = 0;
        public int ItemID
        {
            get { return _IntItemID; }
            set { _IntItemID = value; }
        }

        protected string _strFixedAssetTag = string.Empty;
        public string FixedAssetTag
        {
            get { return _strFixedAssetTag; }
            set { _strFixedAssetTag = value; }
        }

        protected string _strPartNumber = string.Empty;
        public string PartNumber
        {
            get { return _strPartNumber; }
            set { _strPartNumber = value; }
        }
        protected int _IntWorkorderID = 0;
        public int WorkOrderID
        {
            get { return _IntWorkorderID; }
            set { _IntWorkorderID = value; }
        }

        protected System.Collections.Generic.List<FlexField> _ffItemFlexField = new System.Collections.Generic.List<FlexField>();
        public System.Collections.Generic.List<FlexField> ItemFlexField
        {
            get { return _ffItemFlexField; }
            set { _ffItemFlexField = value; }
        }

    }

    public class FlexField
    {
        protected string _strName = string.Empty;
        public string Name
        {
            get { return _strName; }
            set { _strName = value; }
        }
        protected string _strValue = string.Empty;
        public string Value
        {
            get { return _strValue; }
            set { _strValue = value; }
        }
    }

    public class DetailTimeOut
    {
        protected System.Collections.Generic.List<FlexField> _ffFlexFieldList = new System.Collections.Generic.List<FlexField>();
        public System.Collections.Generic.List<FlexField> WCFlexFields
        {
            get { return _ffFlexFieldList; }
            set { _ffFlexFieldList = value; }
        }

        protected System.Collections.Generic.List<OutcomeCode> _ocDiagnosticCodeList = new System.Collections.Generic.List<OutcomeCode>();
        public System.Collections.Generic.List<OutcomeCode> DiagnosticCodeList
        {
            get { return _ocDiagnosticCodeList; }
            set { _ocDiagnosticCodeList = value; }
        }

        protected System.Collections.Generic.List<OutcomeCode> _ocSymptomCodeList = new System.Collections.Generic.List<OutcomeCode>();
        public System.Collections.Generic.List<OutcomeCode> SymptomCodeList
        {
            get { return _ocSymptomCodeList; }
            set { _ocSymptomCodeList = value; }
        }

        protected System.Collections.Generic.List<OutcomeCode> _ocFailureCodeList = new System.Collections.Generic.List<OutcomeCode>();
        public System.Collections.Generic.List<OutcomeCode> FailureCodeList
        {
            get { return _ocFailureCodeList; }
            set { _ocFailureCodeList = value; }
        }

        protected System.Collections.Generic.List<OutcomeCode> _ocRepairCodeList = new System.Collections.Generic.List<OutcomeCode>();
        public System.Collections.Generic.List<OutcomeCode> RepairCodeList
        {
            get { return _ocRepairCodeList; }
            set { _ocRepairCodeList = value; }
        }

        protected string _strResultCode = string.Empty;
        public string ResultCode
        {
            get { return _strResultCode; }
            set { _strResultCode = value; }
        }

        protected string _strNotes = string.Empty;
        public string Notes
        {
            get { return _strNotes; }
            set { _strNotes = value; }
        }

    }

    public class OutcomeCode
    {
        protected string _strName = string.Empty;
        public string Name
        {
            get { return _strName; }
            set { _strName = value; }
        }
        protected string _strValue = string.Empty;
        public string Value
        {
            get { return _strValue; }
            set { _strValue = value; }
        }
    }

    public class DetailFailureAnalysis
    {

        protected System.Collections.Generic.List<FADefectCode> _dcDefectCodeList = new System.Collections.Generic.List<FADefectCode>();
        public System.Collections.Generic.List<FADefectCode> DefectCodeList
        {
            get { return _dcDefectCodeList; }
            set { _dcDefectCodeList = value; }
        }


    }

    public class FADefectCode
    {

        protected string _strDefectCodeName = string.Empty;
        public string DefectCodeName
        {
            get { return _strDefectCodeName; }
            set { _strDefectCodeName = value; }
        }

        protected System.Collections.Generic.List<FlexField> _ffFlexField = new System.Collections.Generic.List<FlexField>();
        public System.Collections.Generic.List<FlexField> FlexField
        {
            get { return _ffFlexField; }
            set { _ffFlexField = value; }
        }

        protected System.Collections.Generic.List<FAActionCode> _acActionCodeList = new System.Collections.Generic.List<FAActionCode>();
        public System.Collections.Generic.List<FAActionCode> ActionCodeList
        {
            get { return _acActionCodeList; }
            set { _acActionCodeList = value; }
        }

    }

    public class FAActionCode
    {
        protected string _strActionCodeName = string.Empty;
        public string ActionCodeName
        {
            get { return _strActionCodeName; }
            set { _strActionCodeName = value; }
        }

        protected string _strAssemblyCode = string.Empty;
        public string AssemblyCode
        {
            get { return _strAssemblyCode; }
            set { _strAssemblyCode = value; }
        }

        protected System.Collections.Generic.List<FlexField> _ffFlexField = new System.Collections.Generic.List<FlexField>();
        public System.Collections.Generic.List<FlexField> FlexField
        {
            get { return _ffFlexField; }
            set { _ffFlexField = value; }
        }

        protected ComponentList _cclComponentCodeList = new ComponentList();
        public ComponentList ComponentCodeList
        {
            get { return _cclComponentCodeList; }
            set { _cclComponentCodeList = value; }
        }


    }

    public class Component
    {
        protected string _strPartNumber = string.Empty;
        public string PartNumber
        {
            get { return _strPartNumber; }
            set { _strPartNumber = value; }
        }

        protected string _strLocation = string.Empty;
        public string Location
        {
            get { return _strLocation; }
            set { _strLocation = value; }
        }

        protected string _strMPN = string.Empty;
        public string ManufacturerPartNumber
        {
            get { return _strMPN; }
            set { _strMPN = value; }
        }

        protected string _strDesc = string.Empty;
        public string Description
        {
            get { return _strDesc; }
            set { _strDesc = value; }
        }


        protected System.Collections.Generic.List<FlexField> _ffFlexField = new System.Collections.Generic.List<FlexField>();
        public System.Collections.Generic.List<FlexField> FlexField
        {
            get { return _ffFlexField; }
            set { _ffFlexField = value; }
        }

    }

    public class ComponentList
    {

        protected System.Collections.Generic.List<Component> _clNewlist = new System.Collections.Generic.List<Component>();
        public System.Collections.Generic.List<Component> NewComponents
        {
            get { return _clNewlist; }
            set { _clNewlist = value; }
        }
        protected System.Collections.Generic.List<Component> _clOldlist = new System.Collections.Generic.List<Component>();
        public System.Collections.Generic.List<Component> DefectiveComponents
        {
            get { return _clOldlist; }
            set { _clOldlist = value; }
        }

    }


    public class DetailChangePart
    {

        protected string _strNewPartNo = string.Empty;
        public string NewPartNo
        {
            get { return _strNewPartNo; }
            set { _strNewPartNo = value; }
        }
        protected string _strNewSerialNo = string.Empty;
        public string NewSerialNo
        {
            get { return _strNewSerialNo; }
            set { _strNewSerialNo = value; }
        }
        protected string _strNewRevisionLevel = string.Empty;
        public string NewRevisionLevel
        {
            get { return _strNewRevisionLevel; }
            set { _strNewRevisionLevel = value; }
        }
        protected string _strNewFixedAssetTag = string.Empty;
        public string NewFixedAssetTag
        {
            get { return _strNewFixedAssetTag; }
            set { _strNewFixedAssetTag = value; }
        }

    }

    public class DetailTriggerResult
    {
        protected string _strResult = string.Empty;
        public string Result
        {
            get { return _strResult; }
            set { _strResult = value; }
        }
        protected string _strMessage = string.Empty;
        public string Message
        {
            get { return _strMessage; }
            set { _strMessage = value; }
        }

        public void SetError(string Message)
        {
            _strResult = "Fail";
            _strMessage = Message;
        }

    }



}

