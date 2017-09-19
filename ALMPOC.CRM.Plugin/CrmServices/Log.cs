using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;

using Microsoft.Xrm.Sdk;

using ALMPOC.CRM.Plugins.Helpers;

namespace ALMPOC.CRM.Plugins.CrmServices
{
    /// <summary>
    /// Provides methods for creating Log records in a seperate datastore. Usage of this class requires the "LoggingConfiguration"-entity
    /// </summary>
    [Obsolete("Starting from CRM 2016, use the Tracing Service")]
    public class Logging
    {
        #region Properties

        public enum LoggingLevel { Debug = 1, Info = 2, Warning = 3, Error = 4, Fatal = 5 };
        public string Customer { get; private set; }
        public string CrmConnectionString { get; private set; }
        public Uri ProxyUrl { get; private set; }
        public int[] HandlerTypes { get; private set; }
        public bool TracingEnabled { get; private set; }
        private IPluginExecutionContext PluginContext { get; set; }

        #endregion

        #region Constructor

        public Logging(EntityManager entityManager)
        {
            PluginContext = entityManager.PluginExecutionContext;

            if (string.IsNullOrEmpty(this.Customer))
            {
                // retrieve configuration
                entityManager.OrganizationServiceContext.MergeOption = Microsoft.Xrm.Sdk.Client.MergeOption.NoTracking;
                var config = (from lc in entityManager.OrganizationServiceContext.CreateQuery("inf_loggingconfiguration")
                              where ((OptionSetValue)lc["statecode"]).Value == 0
                              select new
                              {
                                  customer = lc["inf_customername"].ToString(),
                                  proxyurl = lc["inf_proxyurl"].ToString(),
                                  crmconnectionstring = lc["inf_crmconnectionstring"].ToString(),
                                  tracingEnabled = (bool)lc["inf_tracingenabled"],
                                  handlertypes = entityManager.OrganizationServiceContext.CreateQuery("inf_logginghandler")
                                      .Where(lh => lh["inf_loggingconfigurationid"] == lc["inf_loggingconfigurationid"] && ((OptionSetValue)lh["statecode"]).Value == 0)
                                      .Select(lh => ((OptionSetValue)lh["inf_type"]).Value).ToArray()
                              }).FirstOrDefault();

                if (config != null)
                {
                    this.Customer = config.customer;
                    this.CrmConnectionString = config.crmconnectionstring;
                    this.ProxyUrl = new Uri(config.proxyurl.Substring(config.proxyurl.Length - 1) == "/" ? config.proxyurl : config.proxyurl + "/");
                    this.HandlerTypes = config.handlertypes;
                    this.TracingEnabled = config.tracingEnabled;
                }
            }
        }

        #endregion

        #region Public Exception Methods

        public void LogException(Exception ex)
        {
            LogException(ex, LoggingLevel.Error);
        }

        public void LogException(Exception exception, LoggingLevel errorLevel)
        {
            try
            {
                if (!string.IsNullOrEmpty(this.Customer) && this.HandlerTypes.Length > 0)
                {
                    LoggingRequest logRequest = new LoggingRequest();
                    logRequest.CRMConnectionString = this.CrmConnectionString;
                    logRequest.HandlerTypes = this.HandlerTypes;
                    logRequest.Token = Guid.Empty;

                    logRequest.LoggingItem = new LoggingItem();
                    logRequest.LoggingItem.Customer = this.Customer;
                    logRequest.LoggingItem.Description = exception.Message;
                    logRequest.LoggingItem.Stacktrace = exception.StackTrace;
                    logRequest.LoggingItem.ExceptionType = exception.GetType().ToString();
                    logRequest.LoggingItem.Source = exception.Source;
                    logRequest.LoggingItem.CreatedOn = DateTime.Now;
                    logRequest.LoggingItem.InnerException = getInnerException(exception);
                    logRequest.LoggingItem.Level = (int)errorLevel;

                    // Isolation Mode dependent properties
                    if (PluginContext.IsolationMode == 2)
                    {
                        // In Sandbox
                        logRequest.LoggingItem.User = PluginContext.InitiatingUserId.ToString();
                        logRequest.LoggingItem.Host = "PLUGIN_SANDBOX";
                    }
                    else
                    {
                        logRequest.LoggingItem.User = PluginContext.InitiatingUserId.ToString();
                        logRequest.LoggingItem.Host = System.Net.Dns.GetHostName();
                    }

                    execute(new Uri(this.ProxyUrl, "LoggingService.svc/rest").AbsoluteUri, logRequest);
                }
                else
                {
                    // FAILOVER => log in eventviewer
                    throw new Exception("Missing Logging Configuration");
                }
            }
            catch (Exception ex)
            {
                // FAILOVER => log in eventviewer
                writeEventLogEntry(string.Format("Error in Infront Logging Framework: {0}\r\n{1}", ex.Message, ex.ToString()));
            }
        }

        #endregion

        #region Public Tracing Methods

        public void LogTracing(string message)
        {
            LogTracing(message, LoggingLevel.Info);
        }

        public void LogTracing(string message, LoggingLevel errorLevel)
        {
            try
            {
                if (!string.IsNullOrEmpty(this.Customer) && this.HandlerTypes.Length > 0)
                {
                    if (this.TracingEnabled == true)
                    {
                        LoggingRequest logRequest = new LoggingRequest();
                        logRequest.CRMConnectionString = this.CrmConnectionString;
                        logRequest.HandlerTypes = this.HandlerTypes;
                        logRequest.Token = Guid.Empty;

                        logRequest.LoggingItem = new LoggingItem();
                        logRequest.LoggingItem.Customer = this.Customer;
                        logRequest.LoggingItem.Description = message;
                        logRequest.LoggingItem.CreatedOn = DateTime.Now;
                        logRequest.LoggingItem.Level = (int)errorLevel;
                        logRequest.LoggingItem.CorrelationId = PluginContext.CorrelationId.ToString();
                        logRequest.LoggingItem.Depth = PluginContext.Depth.ToString();
                        logRequest.LoggingItem.Method = System.Reflection.MethodBase.GetCurrentMethod().Name;
                        logRequest.LoggingItem.User = PluginContext.InitiatingUserId.ToString();


                        // Isolation Mode dependent properties
                        if (PluginContext.IsolationMode == 2)
                        {
                            // In Sandbox
                            logRequest.LoggingItem.User = PluginContext.InitiatingUserId.ToString();
                            logRequest.LoggingItem.Host = "PLUGIN_SANDBOX";
                        }
                        else
                        {
                            logRequest.LoggingItem.User = PluginContext.InitiatingUserId.ToString();
                            logRequest.LoggingItem.Host = System.Net.Dns.GetHostName();
                        }

                        execute(new Uri(this.ProxyUrl, "LoggingService.svc/rest").AbsoluteUri, logRequest);
                    }
                }
                else
                {
                    // FAILOVER => log in eventviewer
                    throw new Exception("Missing Logging Configuration");
                }
            }
            catch (Exception ex)
            {
                // FAILOVER => log in eventviewer
                writeEventLogEntry(string.Format("Error in Infront Logging Framework: {0}\r\n{1}", ex.Message, ex.ToString()));
            }
        }

        #endregion

        #region Private Methods

        private void execute(string url, object postObject)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Headers["Content-type"] = "application/json";
                //webClient.UploadDataCompleted += new UploadDataCompletedEventHandler(executeCompleted);

                MemoryStream memoryStream = new MemoryStream();
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(postObject.GetType());

                jsonSerializer.WriteObject(memoryStream, postObject);

                var resultBytes = webClient.UploadData(new Uri(url), "POST", memoryStream.ToArray());

                Stream responseStream = new MemoryStream(resultBytes);
                Response response = (Response)new DataContractJsonSerializer(typeof(Response)).ReadObject(responseStream);
                if (!response.Success)
                {
                    writeEventLogEntry(string.Format("Error in Infront Logging Framework: {0}", response.ErrorMessage));
                }

            }
        }

        private string getInnerException(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex.ToString();
        }

        /// <summary>
        /// Writes an entry in the Windows Application Log with source 'MSCRMTracing' and Type 'Information'
        /// </summary>
        /// <param name="message"></param>
        public void writeEventLogEntry(string message)
        {
            writeEventLogEntry(message, EventLogEntryType.Information);
        }

        /// <summary>
        /// Writes an entry in the Windows Application Log with source 'MSCRMTracing'
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventLogEntryType"></param>
        public void writeEventLogEntry(string message, EventLogEntryType eventLogEntryType)
        {
            try
            {
                const string sSource = "MSCRMTracing";
                if (!EventLog.SourceExists(sSource))
                {
                    return;
                }
                EventLog.WriteEntry(sSource, message, eventLogEntryType);
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }

    #region Logging Object Classes

    public class LoggingRequest
    {
        public int[] HandlerTypes { get; set; }
        public string CRMConnectionString { get; set; }
        public Guid Token { get; set; }
        public LoggingItem LoggingItem { get; set; }

        public LoggingRequest()
        {
        }
    }

    public class LoggingItem
    {
        public string Customer { get; set; }
        public string Description { get; set; }
        public string Host { get; set; }
        public string Source { get; set; }
        public string User { get; set; }
        public string ExceptionType { get; set; }
        public int Level { get; set; }
        public string Method { get; set; }
        public string Stacktrace { get; set; }
        public string InnerException { get; set; }
        public string Depth { get; set; }
        public int Duration { get; set; }
        public string CorrelationId { get; set; }
        public DateTime CreatedOn { get; set; }
        public byte[] ExecutionContext { get; set; }

        public LoggingItem()
        {
        }
    }

    public class Response
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        #region Constructor

        public Response()
        {
        }

        #endregion

    }

    #endregion

    /// <summary>
    /// Provides methods for creating Event Log records in the CRM Environment.
    /// </summary>
    [Obsolete("This class will only work if NOT hosted in a sandbox process, use the Logging class instead")]
    public class Log : CrmServiceBase
    {
        const string ENTITY_EVENTLOG = "inf_eventlog";
        public struct EventLogItem
        {
            public string Type;
            public string Message;
            public string Source;
            public string StackTrace;
            public string TargetSite;
            public string InnerExceptions;
            public string AdditionalInfo;
        }

        /// <summary>
        /// Returns a new instance of the Log class. 
        /// Depending on the Plugin IsolationMode, it will use an OrganizationService created from scratch (IsolutionMode = None) or created from the ServiceProvider (IsolationMode = Sandbox).
        /// When running in IsolationMode, the exceptions or information will not be logged into the CRM Database in case an exception occurs during execution.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static Log CreateLog(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (pluginExecutionContext.IsolationMode == IsolationMode.Sandbox)
            {
                return new Log(serviceProvider, false, true);
            }
            return new Log(serviceProvider, false, false);
        }

        protected Log(IServiceProvider serviceProvider, bool useCurrentUserId, bool instantiateOrganizationServiceFromServiceProvider)
            : base(serviceProvider, useCurrentUserId, instantiateOrganizationServiceFromServiceProvider)
        {
            TracingService.Trace("CrmServiceBase instantiated : useCurrentUserId: {0}, instantiateOrganizationServiceFromServiceProvider: {1} ", useCurrentUserId, instantiateOrganizationServiceFromServiceProvider);
        }


        private void LogEvent(EventLogItem eventLog, bool writeWindowsApplicationLog)
        {
            // Check to make sure the proxy is accessible
            //if (base.organizationServiceProxy == null)
            //{
            //    return;
            //}
            try
            {
                Entity logEntity = new Entity(ENTITY_EVENTLOG);
                logEntity.Attributes.Add("inf_name", "Plugin Assembly");
                logEntity.Attributes.Add("inf_type", eventLog.Type);
                logEntity.Attributes.Add("inf_user", this.PluginExecutionContext.UserId.ToString());
                logEntity.Attributes.Add("inf_host", System.Net.Dns.GetHostName());
                logEntity.Attributes.Add("inf_message", eventLog.Message);
                logEntity.Attributes.Add("inf_source", eventLog.Source);
                logEntity.Attributes.Add("inf_stacktrace", eventLog.StackTrace);
                logEntity.Attributes.Add("inf_targetsite", eventLog.TargetSite);
                logEntity.Attributes.Add("inf_innerexceptions", eventLog.InnerExceptions);
                OrganizationService.Create(logEntity);
            }
            catch (Exception handleExceptionFailure)
            {
                if (writeWindowsApplicationLog)
                {
                    WriteEventLogEntry("Log failed : unable to create an event log record in MSCRM. Detailed exception:" + handleExceptionFailure.Message);
                    WriteEventLogEntry(eventLog.Message + Environment.NewLine + eventLog.StackTrace);
                }
            }
        }

        /// <summary>
        /// Creates an Event Log record (inf_eventlog) in MSCRM. 
        /// If this method fails, the message will be written in the Windows Application Log.
        /// </summary>
        /// <param name="ex"></param>
        public void LogInfo(string message)
        {
            LogInfo(new EventLogItem() { Message = message, Type = "Info" });
        }

        /// <summary>
        /// Creates an Event Log record (inf_eventlog) in MSCRM. 
        /// If this method fails, the message will be written in the Windows Application Log.
        /// </summary>
        /// <param name="ex"></param>
        public void LogInfo(EventLogItem eventLog)
        {
            LogEvent(eventLog, true);
        }

        /// <summary>
        /// Traces the exception in MS CRM and creates an Event Log record (inf_eventlog) in MSCRM. 
        /// If this method fails, the initial error will be written in the Windows Application Log.
        /// </summary>
        /// <param name="ex"></param>
        public void LogException(Exception ex)
        {
            try
            {
                #region Trace method
                base.TracingService.Trace(ex.Message + Environment.NewLine + ex.StackTrace);
                #endregion

                #region Construct Event Log Item

                EventLogItem eventLog = new EventLogItem()
                {
                    Type = ex.GetType().ToString(),
                    Message = "<MESSAGE>" + ex.Message + Environment.NewLine + GetFaultDetailMessage(ex) + "</MESSAGE>",
                    Source = ex.Source,
                    StackTrace = ex.StackTrace,
                    TargetSite = ex.TargetSite != null ? ex.TargetSite.ToString() : String.Empty
                };


                StringBuilder innerExceptions = new StringBuilder();
                if (ex.InnerException != null)
                {
                    innerExceptions.Append(Environment.NewLine + "  <INNEREXCEPTIONS>");
                    AppendInnerExceptions(ex.InnerException, innerExceptions);
                    innerExceptions.Append(Environment.NewLine + "  </INNEREXCEPTIONS>");
                    eventLog.InnerExceptions = innerExceptions.ToString();
                }


                if (ex.Data != null && ex.Data.Count > 0)
                {
                    StringBuilder additionInfoMessage = new StringBuilder();
                    foreach (object key in ex.Data.Keys)
                    {
                        additionInfoMessage.Append("<" + key.ToString() + ">");
                        additionInfoMessage.Append(ex.Data[key].ToString());
                        additionInfoMessage.Append("</" + key.ToString() + ">");
                    }
                    eventLog.AdditionalInfo = additionInfoMessage.ToString();
                }

                LogEvent(eventLog, true);

                #endregion

            }
            catch (Exception handleExceptionFailure)
            {
                WriteEventLogEntry("LogException failed : unable to create an event log record in MSCRM. Detailed exception:" + handleExceptionFailure.Message);
                WriteEventLogEntry(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void AppendInnerExceptions(Exception ex, StringBuilder message)
        {
            if (ex.InnerException != null)
            {
                AppendInnerExceptions(ex.InnerException, message);
            }
            message.Append(Environment.NewLine + "    <INNEREXCEPTION type=\"" + ex.GetType().ToString() + "\">");
            message.Append(Environment.NewLine + "    <INNEREXCEPTIONMESSAGE>" + ex.Message + "</INNEREXCEPTIONMESSAGE>");
            message.Append(Environment.NewLine + "    <INNEREXCEPTIONSTACKTRACE>" + ex.StackTrace + "</INNEREXCEPTIONSTACKTRACE>");
            message.Append(Environment.NewLine + "    " + GetFaultDetailMessage(ex));
            message.Append(Environment.NewLine + "    </INNEREXCEPTION>");
        }

        private string GetFaultDetailMessage(Exception ex)
        {
            //System.Web.Services.Protocols.SoapException soapException = null;
            FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> faultException = null;

            if (ex is FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
            {
                faultException = (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)ex;
                if (faultException.Detail != null)
                {
                    return ("<FAULTEXCEPTION>" + faultException.Detail.Message + "</FAULTEXCEPTION>");
                }
                else
                {
                    return ("<FAULTEXCEPTION/>");
                }
            }
            else
            {
                return ("<FAULTEXCEPTION/>");
            }
        }

        /// <summary>
        /// Writes an entry in the Windows Application Log with source 'MSCRMTracing' and Type 'Information'
        /// </summary>
        /// <param name="message"></param>
        public void WriteEventLogEntry(string message)
        {
            WriteEventLogEntry(message, EventLogEntryType.Information);
        }

        /// <summary>
        /// Writes an entry in the Windows Application Log with source 'MSCRMTracing'
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventLogEntryType"></param>
        public void WriteEventLogEntry(string message, EventLogEntryType eventLogEntryType)
        {
            try
            {
                const string sSource = "MSCRMTracing";
                if (!EventLog.SourceExists(sSource))
                {
                    return;
                }
                EventLog.WriteEntry(sSource, message, eventLogEntryType);
            }
            catch (Exception)
            {
            }
        }
    }
}
