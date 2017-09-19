using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Crm.Sdk.Messages;

using ALMPOC.CRM.Plugins.Helpers;

namespace ALMPOC.CRM.Plugins.CrmServices
{
    /// <summary>
    /// Contains methods which can be used on every CRM Organization
    /// </summary>
    public class EntityManager : CrmServiceBase
    {
        #region Constructor

        public EntityManager(IServiceProvider serviceProvider, bool useCurrentUserId)
            : base(serviceProvider, useCurrentUserId)
        {

        }

        #endregion

        #region Public Methods

        public void SetState(string entityName, Guid entityId, OptionSetValue newState, OptionSetValue newStatus)
        {
            SetStateRequest req = new SetStateRequest();
            req.EntityMoniker = new EntityReference();
            req.EntityMoniker.LogicalName = entityName.ToString();
            req.EntityMoniker.Id = entityId;
            req.State = newState;
            req.Status = newStatus;
            OrganizationService.Execute(req);
        }

        public Guid ExecuteWorkflow(Guid workflowId, Guid entityId)
        {
            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest();

            //Assign the ID of the workflow you want to execute to the request.         
            request.WorkflowId = workflowId;

            //Assign the ID of the entity to execute the workflow on to the request.
            request.EntityId = entityId;

            // Execute the workflow.
            return ((ExecuteWorkflowResponse)OrganizationService.Execute(request)).Id;
        }

        public void TraceInputParameters()
        {
            TracingService.Trace("Number of InputParameters : " + (PluginExecutionContext.InputParameters != null ? PluginExecutionContext.InputParameters.Count.ToString() : "0"));
            if (PluginExecutionContext.InputParameters != null)
            {
                foreach (string key in PluginExecutionContext.InputParameters.Keys)
                {
                    TracingService.Trace("InputParameter Key : " + key);
                    TracingService.Trace("InputParameter Type : " + (PluginExecutionContext.InputParameters[key] != null ? PluginExecutionContext.InputParameters[key].GetType().ToString() : "null"));
                }
            }
        }

        public void TraceInputParameters(IPluginExecutionContext pec)
        {
            TracingService.Trace("Number of InputParameters : " + (pec.InputParameters != null ? pec.InputParameters.Count.ToString() : "0"));
            if (pec.InputParameters != null)
            {
                foreach (string key in pec.InputParameters.Keys)
                {
                    TracingService.Trace("InputParameter Key : " + key);
                    TracingService.Trace("InputParameter Type : " + (pec.InputParameters[key] != null ? pec.InputParameters[key].GetType().ToString() : "null"));
                }
            }
        }

        public void TraceInputEntityAttributes(Entity entity)
        {
            TracingService.Trace("Number of attributes : " + (entity != null && entity.Attributes != null ? entity.Attributes.Count.ToString() : "0"));
            if (entity != null && entity.Attributes != null)
            {
                foreach (string key in entity.Attributes.Keys)
                {
                    TracingService.Trace("Attribute Key : " + key);
                    TracingService.Trace("Attribute Type : " + (entity.Attributes[key] != null ? entity.Attributes[key].GetType().ToString() : "null"));
                    TracingService.Trace("Attribute Value : " + (entity.Attributes[key] != null ? GetAttributeValue(entity.Attributes[key]) : "null"));
                }
            }
        }

        public string GetAttributeValue(object oAttribute)
        {
            if (oAttribute.GetType().Equals(typeof(long)))
            {
                return ((long)oAttribute).ToString();
            }
            if (oAttribute.GetType().Equals(typeof(bool)))
            {
                return ((bool)oAttribute).ToString().ToLower();
            }
            if (oAttribute.GetType().Equals(typeof(DateTime)))
            {
                return ((DateTime)oAttribute).ToShortDateString();
            }
            if (oAttribute.GetType().Equals(typeof(Decimal)))
            {
                return ((Decimal)oAttribute).ToString();
            }
            if (oAttribute.GetType().Equals(typeof(Double)))
            {
                return ((Double)oAttribute).ToString();
            }
            if (oAttribute.GetType().Equals(typeof(EntityReference)))
            {
                // Get the name of the primary field for the entity from the metadata
                return ((EntityReference)oAttribute).Id.ToString();
            }
            if (oAttribute.GetType().Equals(typeof(int)))
            {
                return ((int)oAttribute).ToString();
            }
            if (oAttribute.GetType().Equals(typeof(BooleanManagedProperty)))
            {
                return ((BooleanManagedProperty)oAttribute).Value.ToString();
            }
            if (oAttribute.GetType().Equals(typeof(string)))
            {
                return ((string)oAttribute);
            }
            if (oAttribute.GetType().Equals(typeof(Money)))
            {
                return ((Money)oAttribute).Value.ToString();
            }
            if (oAttribute.GetType().Equals(typeof(OptionSetValue)))
            {
                return ((OptionSetValue)oAttribute).Value.ToString();
            }
            if (oAttribute.GetType().Equals(typeof(Guid)))
            {
                return ((Guid)oAttribute).ToString();
            }
            return string.Empty;
        }

        public void TraceOutputParameters()
        {
            TracingService.Trace("Number of OutputParameters : " + (PluginExecutionContext.OutputParameters != null ? PluginExecutionContext.OutputParameters.Count.ToString() : "0"));
            if (PluginExecutionContext.OutputParameters != null)
            {
                foreach (string key in PluginExecutionContext.OutputParameters.Keys)
                {
                    TracingService.Trace("OutputParameter Key : " + key);
                    TracingService.Trace("OutputParameter Type : " + (PluginExecutionContext.OutputParameters[key] != null ? PluginExecutionContext.OutputParameters[key].GetType().ToString() : "null"));
                }
            }
        }

        public void TraceSharedVariables(IPluginExecutionContext pluginExecutionContext, int depth)
        {
            TracingService.Trace("SharedVariables - PluginExecutionContext depth : {0}", depth);
            if (pluginExecutionContext != null)
            {
                TracingService.Trace("Number of SharedVariables : " + (pluginExecutionContext.SharedVariables != null ? pluginExecutionContext.SharedVariables.Count.ToString() : "0"));
                if (pluginExecutionContext.SharedVariables != null)
                {
                    foreach (string key in pluginExecutionContext.SharedVariables.Keys)
                    {
                        TracingService.Trace("PluginExecutionContext.SharedVariables Key : " + key);
                        TracingService.Trace("PluginExecutionContext.SharedVariables Type : " + (pluginExecutionContext.SharedVariables[key] != null ? pluginExecutionContext.SharedVariables[key].GetType().ToString() : "null"));
                    }
                }
                depth++;
                TraceSharedVariables(pluginExecutionContext.ParentContext, depth);
            }
        }

        public T GetSharedVariableFromContext<T>(IPluginExecutionContext pluginExecutionContext, string key)
        {
            if (pluginExecutionContext != null)
            {
                if (pluginExecutionContext.SharedVariables != null)
                {
                    if (pluginExecutionContext.SharedVariables.ContainsKey(key))
                    {
                        return (T)pluginExecutionContext.SharedVariables[key];
                    }
                }
                return GetSharedVariableFromContext<T>(pluginExecutionContext.ParentContext, key);
            }
            return default(T);
        }

        public T GetAttributeFromEntityOrImage<T>(Entity entity, string preEntityImageName, string attributeName)
        {
            if (!entity.Attributes.ContainsKey(attributeName))
            {
                if (PluginExecutionContext.PreEntityImages.ContainsKey(preEntityImageName))
                {
                    Entity preEntityImage = (Entity)PluginExecutionContext.PreEntityImages[preEntityImageName];
                    if (preEntityImage.Attributes.ContainsKey(attributeName) && preEntityImage.Attributes[attributeName] != null)
                    {
                        return (T)preEntityImage.Attributes[attributeName];
                    }
                }
            }
            else if (entity.Attributes.ContainsKey(attributeName))
            {
                return (T)entity.Attributes[attributeName];
            }
            return default(T);
        }

        /// <summary>
        /// Appends the specified condition expressions to the incoming query. These additional conditions will only be added
        /// in case the incoming query meets the specified constraints (i.e entityName and existing condition expressions)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="targetEntityConstraint"></param>
        /// <param name="conditionExpressionsConstraint"></param>
        /// <param name="conditionExpressionsToAdd"></param>
        public void RestrictIncomingQuery(string targetEntityConstraint, ConditionExpression[] conditionExpressionsConstraint, ConditionExpression[] conditionExpressionsToAdd)
        {
            if (PluginExecutionContext.InputParameters != null)
            {
                if (PluginExecutionContext.InputParameters.ContainsKey(ParameterName.Query))
                {
                    QueryExpression qe = (QueryExpression)PluginExecutionContext.InputParameters[ParameterName.Query];
                    // Check if executing query targets the requested entity
                    if (qe.EntityName == targetEntityConstraint)
                    {
                        if (qe.Criteria == null)
                        {
                            qe.Criteria = new FilterExpression();
                        }
                        // Only apply additional condition expressions in case the specified condition expressions are present
                        if (qe.Criteria.Conditions.Count >= conditionExpressionsConstraint.Length)
                        {
                            bool conditionExpressionsContstraintArePresent = false;
                            // Check if condition expressions are present in the query
                            foreach (ConditionExpression conditionExpression in conditionExpressionsConstraint)
                            {
                                conditionExpressionsContstraintArePresent = IsConditionExpressionPresent(qe.Criteria.Conditions, conditionExpression);
                            }

                            // Add condition expressions to incoming query
                            if (conditionExpressionsContstraintArePresent)
                            {
                                qe.Criteria.Conditions.AddRange(conditionExpressionsToAdd);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves user settings (language) for the user context
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Entity GetUserSettings(Guid userId)
        {
            QueryByAttribute querySystemUserSettings = new QueryByAttribute("usersettings");
            querySystemUserSettings.Attributes.Add("systemuserid");
            querySystemUserSettings.ColumnSet = new ColumnSet();
            querySystemUserSettings.ColumnSet.AddColumn("uilanguageid");
            querySystemUserSettings.Values.Add(userId);

            EntityCollection entityCollection = OrganizationService.RetrieveMultiple(querySystemUserSettings);
            Entity userSettings = entityCollection.Entities.FirstOrDefault();

            return userSettings;
        }
        #endregion

        #region Private methods
        private bool IsConditionExpressionPresent(DataCollection<ConditionExpression> conditionExpressions, ConditionExpression conditionExpressionToCheck)
        {
            // Loop through the condition expressions until a match has been found
            foreach (ConditionExpression conditionExpression in conditionExpressions)
            {
                // Check attribute
                if (conditionExpression.AttributeName != conditionExpressionToCheck.AttributeName) break;

                // Check operator
                if (conditionExpression.Operator != conditionExpressionToCheck.Operator) break;

                // Check values
                bool valuesFound = false;
                foreach (object valueA in conditionExpression.Values)
                {
                    valuesFound = false;
                    foreach (object valueB in conditionExpressionToCheck.Values)
                    {
                        if (valueA.ToString() == valueB.ToString())
                        {
                            valuesFound = true;
                            break;
                        }
                    }
                }

                // When the condition expression has been found, return true
                if (valuesFound)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
