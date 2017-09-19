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


    }
}
