// =====================================================================
//
//  This file is part of the Microsoft CRM Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  This source code is intended only as a supplement to Microsoft
//  Development Tools and/or on-line documentation.  See these other
//  materials for detailed information regarding Microsoft code samples.
//
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//  PARTICULAR PURPOSE.
//
// =====================================================================
using System;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;

using Microsoft.Crm.Sdk;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace ALMPOC.CRM.Plugins.Helpers
{
    /// <summary>
    /// Class created to serialize the IPluginExecutionContext
    /// Since we cannot serialize the Interfaces
    /// </summary>
    public class PluginExecutionContext
    {
        public PluginExecutionContext()
        {
        }
        public PluginExecutionContext(IPluginExecutionContext context)
        {
            BusinessUnitId = context.BusinessUnitId;
            //CallerOrigin = context.CallerOrigin;
            CorrelationId = context.CorrelationId;
            //CorrelationUpdatedTime = context.CorrelationUpdatedTime;
            Depth = context.Depth;
            InitiatingUserId = context.InitiatingUserId;
            InputParameters = context.InputParameters;
            //InvocationSource = context.InvocationSource;
            IsExecutingInOfflineMode = context.IsExecutingOffline;
            MessageName = context.MessageName;
            Mode = context.Mode;
            OrganizationId = context.OrganizationId;
            OrganizationName = context.OrganizationName;
            OutputParameters = context.OutputParameters;
            if (context.ParentContext != null)
            {
                ParentContext = new PluginExecutionContext(context.ParentContext);
            }
            PostEntityImages = context.PostEntityImages;
            PreEntityImages = context.PreEntityImages;
            PrimaryEntityName = context.PrimaryEntityName;
            SecondaryEntityName = context.SecondaryEntityName;
            SharedVariables = context.SharedVariables;
            Stage = context.Stage;
            UserId = context.UserId;
        }
        #region IPluginExecutionContext Members

        public Guid BusinessUnitId;


        //public CallerOrigin CallerOrigin;

        public Guid CorrelationId;

        //public CrmDateTime CorrelationUpdatedTime;

        public int Depth;


        public Guid InitiatingUserId;

        public ParameterCollection InputParameters;

        public int InvocationSource;


        public bool IsExecutingInOfflineMode;

        public string MessageName;


        public int Mode;


        public Guid OrganizationId;

        public string OrganizationName;

        public ParameterCollection OutputParameters;


        public PluginExecutionContext ParentContext;


        public EntityImageCollection PostEntityImages;


        public EntityImageCollection PreEntityImages;


        public string PrimaryEntityName;


        public string SecondaryEntityName;


        public ParameterCollection SharedVariables;


        public int Stage;


        public Guid UserId;


        #endregion
    }

    public class PluginHelper
    {
        /// <summary>
        /// Retrieves EntityId from the Context
        /// Create,Update,Delete,SetState,Assign,DeliverIncoming
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Guid GetEntityId(IPluginExecutionContext context, IOrganizationService service)
        {
            switch (context.MessageName)
            {
                case MessageName.Create:
                case MessageName.DeliverIncoming:
                    if (context.Stage == MessageProcessingStage.BeforeMainOperationOutsideTransaction)
                    {
                        throw new InvalidPluginExecutionException("EntityId is not available in PreCreate");
                    }
                    else
                    {
                        //CreateResponse r;
                        //r.id;
                        if (context.OutputParameters.Contains(ParameterName.Id))
                        {
                            return (Guid)context.OutputParameters[ParameterName.Id];
                        }

                        //DeliverIncomingEmailResponse r;
                        //r.EmailId;
                        if (context.OutputParameters.Contains(ParameterName.EmailId))
                        {
                            return (Guid)context.OutputParameters[ParameterName.EmailId];
                        }
                    }
                    break;
                case MessageName.Update:
                    //context.InputParameters.Contains(ParameterName.Target)
                    //IMetadataService metadataService = context.CreateMetadataService(false);

                    RetrieveEntityRequest rar = new RetrieveEntityRequest();
                    rar.LogicalName = context.PrimaryEntityName;
                    rar.EntityFilters = EntityFilters.Attributes;
                    RetrieveEntityResponse resp = (RetrieveEntityResponse)service.Execute(rar);
                    string keyName = resp.EntityMetadata.PrimaryIdAttribute;

                    //UpdateRequest u;
                    //TargetUpdateAccount a;
                    //a.Account; // This s Dynamic entity
                    //u.Target = a;

                    // Update
                    if (context.InputParameters[ParameterName.Target] is Entity)
                    {
                        Guid key = (Guid)((Entity)context.InputParameters[ParameterName.Target]).Attributes[keyName];
                        return key;
                    }
                    break;
                case MessageName.Delete:
                case MessageName.Assign:
                case MessageName.GrantAccess:
                case MessageName.Handle:
                    if (context.InputParameters[ParameterName.Target] is EntityReference)
                    {
                        EntityReference monikerId = (EntityReference)context.InputParameters[ParameterName.Target];
                        return monikerId.Id;
                    }
                    break;
                case MessageName.SetState:
                case MessageName.SetStateDynamicEntity:
                    //SetStateAccountRequest r;
                    //r.EntityId; // Guid === Moniker 
                    //r.AccountState; // State
                    //r.AccountStatus; // Status
                    return ((EntityReference)context.InputParameters[ParameterName.EntityMoniker]).Id;
                default:
                    if (context.InputParameters.Contains(ParameterName.Target) &&
                        (context.InputParameters[ParameterName.Target] is EntityReference))
                    {
                        EntityReference monikerId = (EntityReference)context.InputParameters[ParameterName.Target];
                        return monikerId.Id;
                    }
                    //Try by best route else fail
                    throw new InvalidPluginExecutionException("GetEntityId could not extract the Guid from Context");
            }
            throw new InvalidPluginExecutionException("GetEntityId could not extract the Guid from Context");
        }

        /// <summary>
        /// Context is serialized to Xml
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetContextXml(IPluginExecutionContext context, out int length)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PluginExecutionContext));
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                PluginExecutionContext c1 = new PluginExecutionContext(context);
                serializer.Serialize(writer, c1);
                StringBuilder sb = writer.GetStringBuilder();
                length = sb.Length;
                if (sb.Length < 10000)
                {
                    return writer.ToString();
                }
                else
                {
                    sb.Insert(0, "Truncated.");
                    return sb.ToString(0, 10000);
                }
            }
        }
        /// <summary>
        /// Custom Entity prepped with the IPluginExecutionContext Information
        /// </summary>
        /// <param name="config"></param>
        /// <param name="secureconfig"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //public static Entity GetPreparedPluginContextEntity(string config, string secureconfig, IPluginExecutionContext context)
        //{
        //    if (null == config)
        //    {
        //        config = "NULL";
        //    }
        //    if (null == secureconfig)
        //    {
        //        secureconfig = "NULL";
        //    }
        //    Entity pluginContextDynamicEntity = new Entity("new_plugincontext");
        //    pluginContextDynamicEntity.Attributes.Add(new StringProperty("new_name", String.Format("{0},{1},{2}", context.MessageName, context.PrimaryEntityName, context.Stage)));
        //    pluginContextDynamicEntity.Attributes.Add(new StringProperty("new_config", config));
        //    pluginContextDynamicEntity.Attributes.Add(new StringProperty("new_secureconfig", secureconfig));
        //    pluginContextDynamicEntity.Attributes.Add(new StringProperty("new_messagename", context.MessageName));
        //    pluginContextDynamicEntity.Attributes.Add(new StringProperty("new_stage", context.Stage.ToString()));
        //    int length;
        //    pluginContextDynamicEntity.Attributes.Add(new StringProperty("new_serializedxml", GetContextXml(context, out length)));
        //    pluginContextDynamicEntity.Attributes.Add(new CrmNumberProperty("new_contextxmllength", new CrmNumber(length)));

        //    return pluginContextDynamicEntity;
        //}
        /// <summary>
        /// In Child pipeline, you can still use CrmService to connect to CRM but we DONOT Recommend as it is prone to Deadlocks
        /// But if you really want to do it, this is a suggested approach.
        /// </summary>
        /// <param name="endPointUrl"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //public static CrmService GetCrmProxyUsingEndpointUrlInChildPipeline(string endPointUrl, IPluginExecutionContext context)
        //{
        //    CrmService childCrmService = new CrmService();
        //    childCrmService.Url = endPointUrl;
        //    childCrmService.Credentials = System.Net.CredentialCache.DefaultCredentials;
        //    childCrmService.CrmAuthenticationTokenValue = new CrmAuthenticationToken();
        //    childCrmService.CrmAuthenticationTokenValue.AuthenticationType = AuthenticationType.AD;
        //    childCrmService.CrmAuthenticationTokenValue.OrganizationName = context.OrganizationName;
        //    childCrmService.CorrelationTokenValue = new CorrelationToken(context.CorrelationId, context.Depth, context.CorrelationUpdatedTime);
        //    return childCrmService;
        //}
    }

    public static class MessageName
    {
        public const string AddItem = "AddItem";
        public const string AddMember = "AddMember";
        public const string AddMembers = "AddMembers";
        public const string AddMembersByFetchXml = "AddMembersByFetchXml";
        public const string Assign = "Assign";
        public const string Associate = "Associate";
        public const string Book = "Book";
        public const string Clone = "Clone";
        public const string Close = "Close";
        public const string CompoundCreate = "CompoundCreate";
        public const string Create = "Create";
        public const string Delete = "Delete";
        public const string DeliverIncoming = "DeliverIncoming";
        public const string DeliverPromote = "DeliverPromote";
        public const string Disassociate = "Disassociate";
        public const string ExecuteWorkflow = "ExecuteWorkflow";
        public const string GrantAccess = "GrantAccess";
        public const string Handle = "Handle";
        public const string Lose = "Lose";
        public const string Merge = "Merge";
        public const string ModifyAccess = "ModifyAccess";
        public const string RemoveItem = "RemoveItem";
        public const string RemoveMember = "RemoveMember";
        public const string RemoveMembers = "RemoveMembers";
        public const string RemoveMembersByFetchXml = "RemoveMembersByFetchXml";
        public const string Reschedule = "Reschedule";
        public const string Retrieve = "Retrieve";
        public const string RetrieveExchangeRate = "RetrieveExchangeRate";
        public const string RetrieveMultiple = "RetrieveMultiple";
        public const string RetrievePrincipalAccess = "RetrievePrincipalAccess";
        public const string RetrieveSharedPrincipalsAndAccess = "RetrieveSharedPrincipalsAndAccess";
        public const string RevokeAccess = "RevokeAccess";
        public const string Route = "Route";
        public const string Send = "Send";
        public const string SetState = "SetState";
        public const string SetStateDynamicEntity = "SetStateDynamicEntity";
        public const string Update = "Update";
        public const string Win = "Win";
    }

    public static class MessageProcessingStage
    {
        //public const int AfterMainOperationOutsideTransaction = 50;
        public const int AfterMainOperationInsideTransaction = 40;
        public const int BeforeMainOperationInsideTransaction = 20;
        public const int BeforeMainOperationOutsideTransaction = 10;
    }

    public static class ParameterName
    {
        public const string Assignee = "Assignee";
        public const string AsyncOperationId = "AsyncOperationId";
        public const string BusinessEntity = "BusinessEntity";
        public const string BusinessEntityCollection = "BusinessEntityCollection";
        public const string CampaignActivityId = "CampaignActivityId";
        public const string CampaignId = "CampaignId";
        public const string ColumnSet = "columnset";
        public const string Context = "context";
        public const string ContractId = "ContractId";
        public const string EmailId = "emailid";
        public const string EndpointId = "EndpointId";
        public const string EntityId = "EntityId";
        public const string EntityMoniker = "EntityMoniker";
        //public const string EntityReference = "EntityReference";
        public const string ExchangeRate = "ExchangeRate";
        public const string FaxId = "FaxId";
        public const string Id = "id";
        public const string ListId = "ListId";
        public const string OptionalParameters = "OptionalParameters";
        public const string PostBusinessEntity = "PostBusinessEntity";
        public const string PostMasterBusinessEntity = "PostMasterBusinessEntity";
        public const string PreBusinessEntity = "PreBusinessEntity";
        public const string PreMasterBusinessEntity = "PreMasterBusinessEntity";
        public const string PreSubordinateBusinessEntity = "PreSubordinateBusinessEntity";
        public const string Query = "Query";
        public const string ReturnDynamicEntities = "ReturnDynamicEntities";
        public const string RouteType = "RouteType";
        public const string Settings = "Settings";
        public const string State = "state";
        public const string Status = "status";
        public const string SubordinateId = "subordinateid";
        public const string Target = "Target";
        public const string TeamId = "TeamId";
        public const string TemplateId = "TemplateId";
        public const string TriggerAttribute = "TriggerAttribute";
        public const string UpdateContent = "UpdateContent";
        public const string ValidationResult = "ValidationResult";
        public const string WorkflowId = "WorkflowId";
    }

    public static class MessageMode
    {
        public const int Synchronous = 0;
        public const int Asynchronous = 1;
    }

    public static class AssemblySourceType
    {
        public const int Database = 0;
        public const int Disk = 1;
        public const int GAC = 2;
    }

    public static class IsolationMode
    {
        public const int None = 1;
        public const int Sandbox = 2;
    }
}
