using ChilliSource.Cloud.Core;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ChilliCoreTemplate.Service
{
    public static partial class Mixpanel
    {
        public static void SendAccountToMixpanel(User account, string eventType = "", Guid? tempUserId = null, Dictionary<string, object> data = null)
        {
            //TODO: implement
        }

        public static void UpdateAccountData(string accountId, Dictionary<string, object> accountData)
        {
            //TODO: implement
        }

        public static void SendEventToMixpanel(int accountId, string eventType, Dictionary<string, object> eventData = null, Dictionary<string, object> accountData = null)
        {
            //TODO: implement
        }

        public static void SendEventToMixpanel(string accountId, string eventType, Dictionary<string, object> eventData = null, Dictionary<string, object> accountData = null)
        {
            //TODO: implement
        }

        public static void CreateAlias(User account, Guid anonymousUserId)
        {
            //TODO: implement
        }
    }
}
