using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace JGS.BusinessLogicEngine
{
    internal static class Configuration
    {
        private const string DEFAULT_SCHEMA_AND_PACKAGE = "BLE_Support";
        private const int DEFAULT_CACHE_DURATION = 0;
        private const string DEFAULT_INCOMING_QUEUE_PATH = ".";
        private const string DEFAULT_OUTGOING_QUEUE_PATH = ".";
        private const JGS.MessageQueues.SmartQueue.QueuePlatform DEFAULT_QUEUE_TYPE = JGS.MessageQueues.SmartQueue.QueuePlatform.InMemoryQueue;

        static Configuration()
        {
            LoadOracleConnectionString();

            LoadSchemaAndPackage();

            LoadCacheDuration();

            LoadIncomingQueuePath();

            LoadOutgoingQueuePath();

            LoadQueueType();
        }

        private static void LoadQueueType()
        {
            string queueType = LoadConfigurationValue("BusinessLogicEngineQueueType", null);
            try
            {
                QueueType = (JGS.MessageQueues.SmartQueue.QueuePlatform)Enum.Parse(typeof(JGS.MessageQueues.SmartQueue.QueuePlatform), queueType);
            }
            catch (Exception ex)
            {
                Logger.AddEntry(Logger.EventSource.Configuration, "LoadConfigurationValue",
                    new ConfigurationErrorsException(
                           "The BusinessLogicEngineQueueType of " + queueType + " from the AppSettings is invalid. Using the default of "
                           + DEFAULT_QUEUE_TYPE.ToString() + ".", ex)
                );
                QueueType = DEFAULT_QUEUE_TYPE;
            }
        }

        private static void LoadOutgoingQueuePath()
        {
            OutgoingQueuePath = LoadConfigurationValue("BusinessLogicEngineOutgoingQueuePath", DEFAULT_OUTGOING_QUEUE_PATH);
        }

        private static void LoadIncomingQueuePath()
        {
            IncomingQueuePath = LoadConfigurationValue("BusinessLogicEngineIncomingQueuePath", DEFAULT_INCOMING_QUEUE_PATH);
        }

        private static void LoadCacheDuration()
        {
            CacheDuration = LoadConfigurationValue("BusinessLogicEngineCacheTime", DEFAULT_CACHE_DURATION);
        }

        private static void LoadSchemaAndPackage()
        {
            SchemaAndPackage = LoadConfigurationValue("BusinessLogicEngineSchemaAndPackage", DEFAULT_SCHEMA_AND_PACKAGE);
            if (!SchemaAndPackage.EndsWith("."))
            {
                SchemaAndPackage += ".";
            }
        }

        private static void LoadOracleConnectionString()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;
            if (ConnectionString == null)
            {
                throw new ConfigurationErrorsException("The Business Logic Engine was unable to read the OracleConnectionString from the ConnectionStrings.");
            }
        }

        private static int LoadConfigurationValue(string settingName, int defaultValue)
        {
            int value;
            try
            {
                value = int.Parse(ConfigurationManager.AppSettings[settingName]);
            }
            catch (Exception ex)
            {
                Logger.AddEntry(Logger.EventSource.Configuration, "LoadConfigurationValue",
                    new ConfigurationErrorsException(
                           "Unable to load the " + settingName + " from the AppSettings. Using the default of "
                           + defaultValue.ToString() + ".", ex)
                );
                value = defaultValue;
            }
            return value;
        }

        private static string LoadConfigurationValue(string settingName, string defaultValue)
        {
            string value;
            try
            {
                value = ConfigurationManager.AppSettings[settingName];
            }
            catch (Exception ex)
            {
                Logger.AddEntry(Logger.EventSource.Configuration, "LoadConfigurationValue",
                    new ConfigurationErrorsException(
                           "Unable to load the " + settingName + " from the AppSettings. Using the default of "
                           + defaultValue + ".", ex)
                );
                value = defaultValue;
            }
            return value;
        }

        public static string ConnectionString
        {
            get;
            private set;
        }

        public static string SchemaAndPackage
        {
            get;
            private set;
        }

        public static bool CacheEnabled
        {
            get
            {
                return CacheDuration != 0;
            }
        }

        public static int CacheDuration
        {
            get;
            private set;
        }

        public static string IncomingQueuePath
        {
            get;
            private set;
        }

        public static string OutgoingQueuePath
        {
            get;
            private set;
        }

        public static JGS.MessageQueues.SmartQueue.QueuePlatform QueueType
        {
            get;
            private set;
        }
    }
}
