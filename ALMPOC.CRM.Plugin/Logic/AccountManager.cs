using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALMPOC.CRM.Plugins.Logic
{
    public class AccountManager:ManagerBase
    {
        public ITracingService TraceService { get; set; }

        public AccountManager(IServiceProvider serviceProvider, bool useCurrentUserId)
            : base(serviceProvider, useCurrentUserId)
        {
            TraceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        }


        public Entity getCountry(string countryName)
        {
            try
            {
                Entity country = new Entity();

                string fetchXml = @"<fetch count='2500' >
                                        <entity name='ng_country' >
                                            <attribute name='ng_countryid' />
                                            <attribute name='ng_name' />
                                            <attribute name='ng_a2' />
                                            <attribute name='ng_otherlanguagename' /> 
                                            <attribute name='ng_salesregionid' />
                                        </entity>
                                    </fetch>";

                List<Entity> countries = OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.ToList<Entity>();

                foreach (var item in countries)
                {
                    if (! string.IsNullOrEmpty(item.GetAttributeValue<string>("ng_name")))
                    {
                        if (item.GetAttributeValue<string>("ng_name").ToLower() == countryName.ToLower())
                        {
                            country = item;
                            break;
                        }
                    }

                    if (! string.IsNullOrEmpty(item.GetAttributeValue<string>("ng_otherlanguagename")))
                    {

                        List<string> countryNames = item.GetAttributeValue<string>("ng_otherlanguagename").Split(';').OfType<string>().ToList();

                        foreach (var countryNm in countryNames)
                        {
                            if (countryNm.Trim().ToLower() == countryName.ToLower())
                            {
                                country = item;
                                break;
                            }
                        }
                    }
                }

                return country;
            }
            catch (Exception Ex)
            {
                throw new InvalidPluginExecutionException("Error occured in the getCountry function :" + Ex.Message);
            }
        }

        public void transferRecordToQueue(string queueName, Entity record)
        {
            string fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='queue'>
                                    <attribute name='queueid' />
                                    <attribute name='name' />
                                    <order attribute='name' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='name' operator='eq' value='"+ queueName +@"' />
                                    </filter>
                                  </entity>
                                </fetch>";

            EntityReference queue = OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.FirstOrDefault<Entity>().ToEntityReference(); 


            AddToQueueRequest addToSourceQueue = new AddToQueueRequest
            {
                DestinationQueueId = queue.Id,
                Target = record.ToEntityReference()
            };

            OrganizationService.Execute(addToSourceQueue);
        }

        public bool isUserInternalSales(Guid systemuserId)
        {
            try
            {
                int internal_sales = 100000001;
                bool returnValue = false;

                Entity user = OrganizationService.Retrieve("systemuser", systemuserId, new ColumnSet("ng_usertype"));

                if (user != null && user.Contains("ng_usertype"))
                {
                    if (user.GetAttributeValue<OptionSetValue>("ng_usertype").Value != internal_sales)
                    {
                        returnValue = true;
                    }
                }

                return returnValue;

            }
            catch (Exception Ex)
            {
                throw new InvalidPluginExecutionException("Error occured in the isUserInternalSales function :" + Ex.Message);
            }
        }
    }
}
