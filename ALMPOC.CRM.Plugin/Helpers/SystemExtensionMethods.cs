using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace ALMPOC.CRM.Plugins.Helpers
{
    public static class SystemExtensions
    {
        #region Native Type Extensions

        public static DateTime? SetDateTimeKind(this DateTime? datetime, DateTimeKind datetimeKind)
        {
            DateTime? newDatetime = null;
            if (datetime.HasValue)
            {
                newDatetime = DateTime.SpecifyKind(datetime.Value, datetimeKind);
            }
            return newDatetime;
        }

        public static DateTime? SetDateTimeKind(this DateTime datetime, DateTimeKind datetimeKind)
        {
            DateTime? newDatetime = DateTime.SpecifyKind(datetime, datetimeKind);

            return newDatetime;
        }

        public static Guid? ToGuid(this string stringValue)
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                return default(Guid?);
            }
            else
            {
                return new Guid(stringValue);
            }
        }

        #endregion
    }
}
