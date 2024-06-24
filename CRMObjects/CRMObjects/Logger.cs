using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSEBLogging = Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Diagnostics;
using System.Configuration;


namespace CRMObjects
{
    internal static class Logger
    {
        private const string DEFAULT_EXCEPTION_MESSAGE = "An exception has occurred.";
        private const string LOGGING_CATEGORY = "BusinessLogicEngine";


        public enum EventSource
        {
            Configuration,
            Dispatcher,
            Process,
            Field,
            Rule,
            Action,
            Database,
            API
        }

        static Logger()
        {
           
        }


        #region Exception Logging
    

        public static void AddEntry(EventSource eventSource, string location, Exception ex)
        {
            AddEntry(eventSource, location, DEFAULT_EXCEPTION_MESSAGE, ex);
        }

        public static void AddEntry(EventSource eventSource, string location, string message, Exception ex)
        {
            string title = eventSource.ToString() + "(" + location + "):" + TraceEventType.Error.ToString();
            Dictionary<string, object> properties = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(ex.Message))
                properties.Add("ExceptionMessageStack", ex.Message);

            if (!string.IsNullOrEmpty(ex.Source))
                properties.Add("ExceptionSource", ex.Source);

            if (!string.IsNullOrEmpty(ex.StackTrace))
                properties.Add("ExceptionStackTrace", ex.StackTrace);

            MSEBLogging.Logger.Write(message, LOGGING_CATEGORY, (int)TraceEventType.Error, (int)eventSource, TraceEventType.Error, title, properties);    
        }
        #endregion


        #region General Logging
      
        public static void AddEntry(EventSource eventSource, string location, TraceEventType traceEventType, string message)
        {
            string title = eventSource.ToString() + "(" + location + "):" + traceEventType.ToString();
            MSEBLogging.Logger.Write(message, LOGGING_CATEGORY, (int)traceEventType, (int)eventSource, traceEventType, title);           
        }
        #endregion
    }

}
