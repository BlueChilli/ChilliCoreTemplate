using ChilliSource.Cloud.Core;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;

namespace ChilliCoreTemplate.Service
{
    public static class ServicesLibrary
    {

        public static string GetError(this RestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.GatewayTimeout) return "Gateway timeout. External service failed to return a response. Please try again later.";
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                if (response.ErrorMessage.StartsWith("Error parsing") && !String.IsNullOrEmpty(response.Content))
                    return response.Content;
                else
                    return response.ErrorMessage;
            }
            return response.StatusDescription;
        }
    }
}
