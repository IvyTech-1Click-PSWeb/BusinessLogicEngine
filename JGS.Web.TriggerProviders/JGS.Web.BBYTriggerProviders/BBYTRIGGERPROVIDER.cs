using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JGS.Web.TriggerProviders
{
    public class BBYTRIGGERPROVIDER : JGS.Web.TriggerProviders.TriggerProviderBase
    {


        public override string Name { get; set; }
        public BBYTRIGGERPROVIDER()
        {
            this.Name = "BBYTRIGGERPROVIDER";
        }

        public override System.Xml.XmlDocument Execute(System.Xml.XmlDocument xmlIn)
        {
            Trigger.Trigger trg = new Trigger.Trigger(xmlIn);
            switch (trg.Header.TriggerType)
            {
                case "TIMEOUT":
                    trg = TOTrigger(trg);
                    break;
                case "FAILUREANALYSIS":
                    trg = FATrigger(trg);
                    break;
            }
            return trg.toXML();
        }

        #region "FA"
        private Trigger.Trigger FATrigger(Trigger.Trigger Trigger)
        {
            Trigger.Trigger TRG = Trigger;
            for (int dc = 0; dc <= TRG.Detail.FailureAnalysis.DefectCodeList.Count - 1; dc++)
            {
                for (int ac = 0; ac <= TRG.Detail.FailureAnalysis.DefectCodeList[dc].ActionCodeList.Count - 1; ac++)
                {
                    for (int comps = 0; comps <= TRG.Detail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.NewComponents.Count - 1; comps++)
                    {
                        System.Collections.Generic.List<Oracle.DataAccess.Client.OracleParameter> Params = new System.Collections.Generic.List<Oracle.DataAccess.Client.OracleParameter>();
                        Params.Add(new Oracle.DataAccess.Client.OracleParameter("Component", Oracle.DataAccess.Client.OracleDbType.Varchar2, System.Data.ParameterDirection.Input) { Value = TRG.Detail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.NewComponents[comps].PartNumber });
                        Params.Add(new Oracle.DataAccess.Client.OracleParameter("itemid", Oracle.DataAccess.Client.OracleDbType.Varchar2, System.Data.ParameterDirection.Input) { Value = Trigger.Detail.ItemLevel.ItemID.ToString() });
                        Params.Add(new Oracle.DataAccess.Client.OracleParameter("p_username", Oracle.DataAccess.Client.OracleDbType.Varchar2, System.Data.ParameterDirection.Input) { Value = Trigger.Header.UserObj.Username });
                        string Res = JGS.Web.TriggerProviders.Functions.DbFetch(this.ConnectionString, "WEBAPP1", "JGSRIMBLETRIGGERS", "ThreeXComponent", Params);
                        if (Res == "TRUE")
                        {
                            TRG.Detail.TriggerResult.SetError("Component: " + TRG.Detail.FailureAnalysis.DefectCodeList[dc].ActionCodeList[ac].ComponentCodeList.NewComponents[comps].PartNumber + ", has been assigned more than three times");
                            return TRG;
                        }
                    }
                }
            }
            return TRG;
        }
        #endregion


        #region "TimeOut"
        private Trigger.Trigger TOTrigger(Trigger.Trigger Trigger)
        {
            Trigger.Trigger TRG = Trigger;
                if (Trigger.Detail.TimeOut.ResultCode.Substring(0,4).ToUpper()=="FAIL")
                {
                    if (Trigger.Detail.TimeOut.FailureCodeList.Count <= 0)
                    {
                        Trigger.Detail.TriggerResult.Message = "Trigger Error: Debe seleccionar un código de falla, excepto 33.3-Not Fault Found";
                    }
                }
            return TRG;
        }
        #endregion


    }
}
