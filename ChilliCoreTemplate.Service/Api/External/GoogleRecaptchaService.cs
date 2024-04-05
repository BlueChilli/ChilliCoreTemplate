using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Api.Google
{
    public class GoogleRecaptchaService : IService
    {
        private readonly GoogleRecaptchaSection _config;
        private readonly IWebHostEnvironment _env;

        public GoogleRecaptchaService(ProjectSettings config, IWebHostEnvironment env)
        {
            _config = config.GoogleRecaptcha;
            _env = env;
        }

        public virtual async Task<ServiceResult<double>> Validate(string token, double score)
        {
            if (!_config.Enabled)
            {
                return ServiceResult<double>.AsSuccess(1.0);
            }

            if (String.IsNullOrEmpty(token))
            {
                return ServiceResult<double>.AsError("Recaptcha token is missing");
            }

            if (Guid.TryParse(token, out var debugToken))
            {
                if (debugToken != _config.DebugToken || !_env.IsDevelopment())
                {
                    return ServiceResult<double>.AsError("Recaptcha debug token is not valid");
                }
                return ServiceResult<double>.AsSuccess(1.0);
            }

            var client = new RestClient("https://www.google.com/recaptcha/");
            var request = new RestRequest("api/siteverify",Method.Post);
            request.AddObject(new
            {
                secret = _config.PrivateApiKey,
                response = token
            });
            var result = await client.ExecuteAsync<GoogleRecaptchaResponse>(request);
            if (result.IsSuccessful)
            {
                if (score == 0) score = _config.DefaultScore;

                if (result.Data.Score < score)
                {
                    return ServiceResult<double>.AsError($"Recaptcha score of {result.Data.Score} is below required score of {score}");
                }

                return ServiceResult<double>.AsSuccess(result.Data.Score);
            }

            if (result.Data != null && result.Data.ErrorCodes.Any()) return ServiceResult<double>.AsError($"Recaptcha error: {result.Data.ErrorCodes.ToDelimitedString()}");

            return ServiceResult<double>.AsError(result.GetError());
        }
        
        private class GoogleRecaptchaResponse
        {
            public bool Success { get; set; }

            public double Score { get; set; }

            public string Action { get; set; }

            [JsonProperty("challenge_ts")]
            public DateTime ChallengeTs { get; set; }

            public string Hostname { get; set; }

            [JsonProperty("error-codes")]
            public List<string> ErrorCodes { get; set; }
        }

    }
}
