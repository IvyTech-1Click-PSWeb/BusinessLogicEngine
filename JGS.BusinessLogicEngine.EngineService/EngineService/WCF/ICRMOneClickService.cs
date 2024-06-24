using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using JGS.Shared.Package;
using System.Xml;
using System.Reflection;
using System.Xml.Serialization;
using System.IO;
using CRMObjects.CRMInput;

namespace JGS.BusinessLogicEngine.WCF
{
    [ServiceContract(Namespace = "http://corporate.jabil.org/BusinessLogicEngine")]
    public interface ICRMOneClickService
    {
        [OperationContract]
        string CRMLogContactInfo(string xmlInput);

        [OperationContract]
        string CRMPartAvailability(string xmlInput);

        [OperationContract]
        string CRMCreateOrder(string xmlInput);

        [OperationContract]
        string CRMCreateIncident(string xmlInput);

        [OperationContract]
        string CRMPartAvailavableLookupByPartNum(string xmlInput);

        [OperationContract]
        string CRMTest(string xmlInput);

        [OperationContract]
        string CRMUpdateDellOrder(string xmlInput);

        [OperationContract]
        string CRMGetHolidays(string xmlInput);
    }

}