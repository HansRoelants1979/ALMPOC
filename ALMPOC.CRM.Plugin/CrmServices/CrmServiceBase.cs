using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Web;
using System.ServiceModel.Description;

using Microsoft.Crm.Sdk;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Client;

using ALMPOC.CRM.Plugins.CrmServices.XrmSdk;
using ALMPOC.CRM.Plugins.Helpers;

namespace ALMPOC.CRM.Plugins.CrmServices
{
    public abstract class CrmServiceBase
    {
        #region Properties

        private bool UseCurrentUserId { get; set; }
        protected Configuration CRMConfiguration;
        public OrganizationServiceProxy organizationServiceProxy;

        public IServiceProvider ServiceProvider { get; private set; }
        public IPluginExecutionContext PluginExecutionContext { get; private set; }

        private IOrganizationServiceFactory organizationServiceFactory;
        public IOrganizationServiceFactory OrganizationServiceFactory
        {
            get
            {
                if (organizationServiceFactory == null)
                {
                    organizationServiceFactory = (IOrganizationServiceFactory)ServiceProvider.GetService(typeof(IOrganizationServiceFactory));
                }
                return organizationServiceFactory;
            }
        }

        private IOrganizationService organizationService;
        public IOrganizationService OrganizationService
        {
            get
            {
                if (organizationService == null)
                {
                    if (organizationServiceProxy != null)
                    {
                        organizationService = (IOrganizationService)organizationServiceProxy;
                    }
                    else
                    {
                        if (UseCurrentUserId)
                        {
                            organizationService = OrganizationServiceFactory.CreateOrganizationService(PluginExecutionContext.UserId);
                        }
                        else
                        {
                            organizationService = OrganizationServiceFactory.CreateOrganizationService(null);
                        }
                    }
                }
                return organizationService;
            }
        }

        private ServiceContext organizationServiceContext;
        public ServiceContext OrganizationServiceContext
        {
            get
            {
                if (organizationServiceContext == null)
                {
                    organizationServiceContext = new ServiceContext(OrganizationService);
                }
                return organizationServiceContext;
            }
        }

        private ITracingService tracingService;
        public ITracingService TracingService
        {
            get
            {
                if (tracingService == null)
                {
                    tracingService = (ITracingService)ServiceProvider.GetService(typeof(ITracingService));
                    //tracingService = new WindowsTracingService();
                }
                return tracingService;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new CRM Service Connection. 
        /// If the instantiateContextFromServiceProvider equals 'True', then a new context will be created from the given service provider.
        /// If the instantiateContextFromServiceProvider equals 'False', then a new context will be created from scratch. All requests will be made with the SYSTEM user account.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="useCurrentUserId"></param>
        /// <param name="instantiateOrganizationServiceFromServiceProvider"></param>
        public CrmServiceBase(IServiceProvider serviceProvider, bool useCurrentUserId, bool instantiateOrganizationServiceFromServiceProvider)
        {
            this.UseCurrentUserId = useCurrentUserId;
            ServiceProvider = serviceProvider;

            PluginExecutionContext = (IPluginExecutionContext)ServiceProvider.GetService(typeof(IPluginExecutionContext));

            if (!instantiateOrganizationServiceFromServiceProvider)
            {
                if (PluginExecutionContext.IsolationMode == IsolationMode.Sandbox)
                {
                    throw new Exception("You cannot instantiate the Organization Service from scratch when running in Sandbox mode.");
                }

                if (LoadServerConfiguration() == null) return;

                organizationServiceProxy = new OrganizationServiceProxy(CRMConfiguration.OrganizationUri,
                                                                            CRMConfiguration.HomeRealmUri,
                                                                            CRMConfiguration.Credentials,
                                                                            CRMConfiguration.DeviceCredentials);
            }
        }

        /// <summary>
        /// Instantiates a new CRM Service Connection based on the given service provider.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="useCurrentUserId"></param>
        public CrmServiceBase(IServiceProvider serviceProvider, bool useCurrentUserId)
            : this(serviceProvider, useCurrentUserId, true)
        {
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Load the server configuration (not possible if hosted in Sandbox Process) 
        /// </summary>
        /// <returns></returns>
        private Configuration LoadServerConfiguration()
        {
            // Check if not run in Isolation
            if (PluginExecutionContext.IsolationMode == IsolationMode.Sandbox)
            {
                throw new Exception("The LoadServerConfiguration method cannot be used in Sandbox mode (System.Web is not configured for Partial Trust)");
            }

            // Extract URl from HTTP Context
            HttpContext currentHttpContext = HttpContext.Current;

            if (currentHttpContext == null)
            {
                return null;
            }

            CRMConfiguration = new Configuration();

            // Set credentials
            ClientCredentials credentials = new ClientCredentials();
            credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;

            // Concatenate URI based on current HTTP Request

            // Check for type of deployment; 

            // On Premise Example : https://server.domain.com/MYORG/AppWebServices
            // IFD Example : https://MYORG.domain.com/AppWebServices

            if (currentHttpContext.Request.Url.Segments[1].ToLower() == (PluginExecutionContext.OrganizationName.ToLower() + "/"))
            { // On premise
                CRMConfiguration.OrganizationUri = new Uri(String.Format("{0}/{1}/XRMServices/2011/Organization.svc", currentHttpContext.Request.Url.GetLeftPart(UriPartial.Authority), PluginExecutionContext.OrganizationName));
            }
            else
            { // IFD
                CRMConfiguration.OrganizationUri = new Uri(String.Format("{0}/XRMServices/2011/Organization.svc", currentHttpContext.Request.Url.GetLeftPart(UriPartial.Authority)));
            }
            CRMConfiguration.Credentials = credentials;

            return CRMConfiguration;

        }

        private OrganizationDetailCollection DiscoverOrganizations(IDiscoveryService service)
        {
            RetrieveOrganizationsRequest orgRequest = new RetrieveOrganizationsRequest();
            RetrieveOrganizationsResponse orgResponse =
                (RetrieveOrganizationsResponse)service.Execute(orgRequest);

            return orgResponse.Details;
        }

        #endregion

        #region Inner Classes

        protected class Configuration
        {
            public Uri OrganizationUri;
            public Uri HomeRealmUri = null;
            public ClientCredentials DeviceCredentials = null;
            public ClientCredentials Credentials = null;
        }

        #endregion
    }
}
