using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service
{
    public class PdfService : IService
    {
        private string _uri;
        private string _apiKey;
        private ILogger<PdfService> _logger;
        private IWebHostEnvironment _env;

        public PdfService(IConfiguration config, ILoggerFactory loggerFactory, IWebHostEnvironment hostingEnvironment)
        {
            _env = hostingEnvironment;
            _uri = config.GetValue<string>("PdfService:Uri");
            _apiKey = config.GetValue<string>("PdfService:ApiKey");

            _logger = loggerFactory.CreateLogger<PdfService>();
        }

        private Task<RestResponse> PostRequest(string html, object options)
        {
            var client = new RestClient(_uri).AddDefaultHeader("x-api-key", _apiKey);

            var request = new RestRequest("", Method.Post);
            var jsonBody = new
            {
                htmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(html)),
                options = options
            };

            request.AddJsonBody(jsonBody);
            request.AddHeader("Accept", "*/*");

            return client.ExecuteAsync(request);
        }

        /// <summary>
        /// See https://wkhtmltopdf.org/usage/wkhtmltopdf.txt for all options. Note that they need to be cammel-cased here because of the nodejs component in use.            
        /// .e.g { pageSize = "A4" } , for option --page-size
        /// </summary>
        /// <param name="html"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<byte[]> GeneratePdfAsync(string html, object options)
        {
            if (String.IsNullOrEmpty(html))
                return new byte[0];

            var response = await PostRequest(html, options);
            var responseJson = JObject.Parse(response.Content);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var pdfBase64 = (string)responseJson["pdfBase64"];
                var pdfContent = String.IsNullOrEmpty(pdfBase64) ? new byte[0] : Convert.FromBase64String(pdfBase64);

                return pdfContent;
            }

            var error = (string)responseJson["message"];
            var errorGuid = Guid.NewGuid().ToString();
            _logger.Log(LogLevel.Error, $"[PdfService error - {errorGuid}] {error}");

            throw new ApplicationException($"Error generating PDF. [ReferenceID: {errorGuid}] {(_env.IsProduction() ? "" : error)}");
        }
    }
}
