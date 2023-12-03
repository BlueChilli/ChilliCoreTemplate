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
            if (!String.IsNullOrEmpty(response.ErrorMessage)) return response.ErrorMessage;
            return response.StatusDescription;
        }

        public static ServiceResult<T> AsError<T>(string error, string key)
        {
            return new ServiceResult<T>() { Success = false, Result = default(T), Error = error, Key = key, StatusCode = HttpStatusCode.BadRequest };
        }
    }
}
