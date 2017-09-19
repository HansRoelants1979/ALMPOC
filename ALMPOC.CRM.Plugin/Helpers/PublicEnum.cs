using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALMPOC.CRM.Plugins.Helpers
{
    public class PublicEnum
    {
        public enum ValidatePhoneNumber
        {
            Invalid,
            Valid
        }

        public enum DataModificationAction
        {
            Create = 1,
            Update = 2,
            Delete = 3,
            Activate = 4,
            Deactivate = 5,
            Merge = 6,
            MoveFrom = 7,
            MoveTo = 8,
            StatusUpdate = 9
        }
    }
}
