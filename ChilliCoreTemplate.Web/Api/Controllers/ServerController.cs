using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.Api;
using ChilliCoreTemplate.Service.Api.Slack;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiKeyIgnore]
    [Route("api/[controller]")]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status500InternalServerError)]
    public class ServerController : ControllerBase
    {
        ProjectSettings _config;
        UserKeyHelper _keyHelper;
        ITemplateViewRenderer _templateViewRenderer;
        IBackgroundTaskQueue _taskQueue;
        FormOptions _formOptions;
        PushNotificationConfiguration _push;
        SlackApiService _slack;

        public ServerController(ProjectSettings config, UserKeyHelper keyHelper, ITemplateViewRenderer templateViewRenderer, IBackgroundTaskQueue taskQueue, IOptions<FormOptions> formOptions, PushNotificationConfiguration push, SlackApiService slack)
        {
            _config = config;
            _keyHelper = keyHelper;
            _templateViewRenderer = templateViewRenderer;
            _taskQueue = taskQueue;
            _formOptions = formOptions.Value;
            _push = push;
            _slack = slack;
        }

        //Note that calls to api/server are not logged in ApiLogs

        [HttpGet("tick")]
        public virtual IActionResult ServerTick()
        {
            return Ok(new { result = "OK" });
        }

        [HttpGet("exception")]
        public virtual IActionResult ServerException(Guid? password = null)
        {
            if (password == new Guid("3444B9DE-CEE2-4CB9-B8FF-0C1F78973764")) throw new Exception("Test error logging2");
            return Ok(new { result = "OK" });
        }

        [HttpGet("error")]
        public virtual IActionResult ServerError(string message = "Test error logging")
        {
            ErrorLogHelper.LogMessage(message);
            return Ok(new { result = "OK" });
        }

        [HttpGet("testrazor")]
        public virtual IActionResult TestRazor(Guid? key = null)
        {
            if (key != new Guid("D26689B5-17C5-473A-AA70-BCA9774D763D"))
            {
                return this.BadRequest();
            }

            var template = RazorTemplates.WelcomeEmail;

            var model = new RazorTemplateDataModel<RegistrationCompleteViewModel>
            {
                Data = new RegistrationCompleteViewModel
                {
                    FirstName = "Developer",
                    Email = "developers@bluechilli.com",
                    Token = Guid.NewGuid().ToString()
                },
                TemplateId = template.TemplateName.Substring(template.TemplateName.LastIndexOf("/") + 1),
                TrackingId = Guid.NewGuid().ToShortGuid()
            };
            EmailServiceHelpers.SetConfigProperties(model, _config, "developers@bluechilli.com");

            var html = TaskHelper.GetResultSafeSync(() => _templateViewRenderer.RenderAsync(template.TemplateName, model));
            return this.Content(html, "text/html", Encoding.UTF8);
        }

        [HttpGet("testpdf")]
        public virtual async Task<IActionResult> TestPdf(Guid? key = null)
        {
            if (key != new Guid("D26689B5-17C5-473A-AA70-BCA9774D763D"))
            {
                return this.BadRequest();
            }

            var template = RazorTemplates.WelcomeEmail;

            var model = new RazorTemplateDataModel<RegistrationCompleteViewModel>
            {
                Data = new RegistrationCompleteViewModel
                {
                    FirstName = "Developer",
                    Email = "developers@bluechilli.com",
                    Token = Guid.NewGuid().ToString()
                },
                TemplateId = template.TemplateName.Substring(template.TemplateName.LastIndexOf("/") + 1),
                TrackingId = Guid.NewGuid().ToShortGuid()
            };
            EmailServiceHelpers.SetConfigProperties(model, _config, "developers@bluechilli.com");

            var html = await _templateViewRenderer.RenderAsync(template.TemplateName, model);

            var svc = this.HttpContext.RequestServices.GetRequiredService<PdfService>();
            var pdfContent = await svc.GeneratePdfAsync(html, new { });

            var file = this.File(pdfContent, "application/pdf");
            return file;
        }

        static readonly string googleChartsSampleHtml = Encoding.UTF8.GetString(Convert.FromBase64String("PGh0bWw+CiAgPGhlYWQ+CiAgICA8IS0tTG9hZCB0aGUgQUpBWCBBUEktLT4KICAgIDxzY3JpcHQgdHlwZT0idGV4dC9qYXZhc2NyaXB0IiBzcmM9Imh0dHBzOi8vd3d3LmdzdGF0aWMuY29tL2NoYXJ0cy9sb2FkZXIuanMiPjwvc2NyaXB0PgogICAgPHNjcmlwdCB0eXBlPSJ0ZXh0L2phdmFzY3JpcHQiPgoKCQlmdW5jdGlvbiB3cmFwQ2hhcnRzTG9hZChjYWxsYmFjaykgewoJCQlnb29nbGUuY2hhcnRzLmxvYWQoJ2N1cnJlbnQnLCB7J3BhY2thZ2VzJzpbJ2NvcmVjaGFydCddfSk7CgkJCQoJCQkvLyBTZXQgYSBjYWxsYmFjayB0byBydW4gd2hlbiB0aGUgR29vZ2xlIFZpc3VhbGl6YXRpb24gQVBJIGlzIGxvYWRlZC4KCQkJLy8gKipUaGlzIGRvZXMgbm90IHdvcmsgd2l0aCB3a2h0bWx0b3BkZgoJCQkvL2dvb2dsZS5jaGFydHMuc2V0T25Mb2FkQ2FsbGJhY2soY2FsbGJhY2spOwoJICAKCQkJdmFyIGludGVydmFsID0gc2V0SW50ZXJ2YWwoZnVuY3Rpb24oKSB7CgkJCQlpZiAoCgkJCQkJZ29vZ2xlLnZpc3VhbGl6YXRpb24gIT09IHVuZGVmaW5lZAoJCQkJCSYmIGdvb2dsZS52aXN1YWxpemF0aW9uLlBpZUNoYXJ0ICE9PSB1bmRlZmluZWQKCQkJCQkmJiBnb29nbGUudmlzdWFsaXphdGlvbi5ldmVudHMgIT09IHVuZGVmaW5lZAoJCQkJKSB7CgkJCQkJY2xlYXJJbnRlcnZhbChpbnRlcnZhbCk7CgkJCQkJY2FsbGJhY2soKTsKCQkJCX0KCQkJfSwgMTApOwoJCX0gICAgICAKCiAgICAgIC8vIENhbGxiYWNrIHRoYXQgY3JlYXRlcyBhbmQgcG9wdWxhdGVzIGEgZGF0YSB0YWJsZSwKICAgICAgLy8gaW5zdGFudGlhdGVzIHRoZSBwaWUgY2hhcnQsIHBhc3NlcyBpbiB0aGUgZGF0YSBhbmQKICAgICAgLy8gZHJhd3MgaXQuCiAgICAgIGZ1bmN0aW9uIGRyYXdDaGFydCgpIHsKCiAgICAgICAgLy8gQ3JlYXRlIHRoZSBkYXRhIHRhYmxlLgogICAgICAgIHZhciBkYXRhID0gbmV3IGdvb2dsZS52aXN1YWxpemF0aW9uLkRhdGFUYWJsZSgpOwogICAgICAgIGRhdGEuYWRkQ29sdW1uKCdzdHJpbmcnLCAnVG9wcGluZycpOwogICAgICAgIGRhdGEuYWRkQ29sdW1uKCdudW1iZXInLCAnU2xpY2VzJyk7CiAgICAgICAgZGF0YS5hZGRSb3dzKFsKICAgICAgICAgIFsnTXVzaHJvb21zJywgM10sCiAgICAgICAgICBbJ09uaW9ucycsIDFdLAogICAgICAgICAgWydPbGl2ZXMnLCAxXSwKICAgICAgICAgIFsnWnVjY2hpbmknLCAxXSwKICAgICAgICAgIFsnUGVwcGVyb25pJywgMl0KICAgICAgICBdKTsKCiAgICAgICAgLy8gU2V0IGNoYXJ0IG9wdGlvbnMKICAgICAgICB2YXIgb3B0aW9ucyA9IHsndGl0bGUnOidIb3cgTXVjaCBQaXp6YSBJIEF0ZSBMYXN0IE5pZ2h0JywKICAgICAgICAgICAgICAgICAgICAgICAnd2lkdGgnOjQwMCwKICAgICAgICAgICAgICAgICAgICAgICAnaGVpZ2h0JzozMDB9OwoKICAgICAgICAvLyBJbnN0YW50aWF0ZSBhbmQgZHJhdyBvdXIgY2hhcnQsIHBhc3NpbmcgaW4gc29tZSBvcHRpb25zLgogICAgICAgIHZhciBjaGFydCA9IG5ldyBnb29nbGUudmlzdWFsaXphdGlvbi5QaWVDaGFydChkb2N1bWVudC5nZXRFbGVtZW50QnlJZCgnY2hhcnRfZGl2JykpOwoJCWdvb2dsZS52aXN1YWxpemF0aW9uLmV2ZW50cy5hZGRMaXN0ZW5lcihjaGFydCwgJ3JlYWR5JywgZnVuY3Rpb24oKXsKCQkJd2luZG93LnN0YXR1cyA9ICdyZWFkeSc7CgkJfSk7CgkJCiAgICAgICAgY2hhcnQuZHJhdyhkYXRhLCBvcHRpb25zKTsKICAgICAgfQoJICAKCSAgd3JhcENoYXJ0c0xvYWQoZHJhd0NoYXJ0KTsKICAgIDwvc2NyaXB0PgogIDwvaGVhZD4KCiAgPGJvZHk+CiAgICA8IS0tRGl2IHRoYXQgd2lsbCBob2xkIHRoZSBwaWUgY2hhcnQtLT4KICAgIDxkaXYgaWQ9ImNoYXJ0X2RpdiI+PC9kaXY+Cgk8ZGl2PkZpeGVkIHZhbHVlLjwvZGl2PgogIDwvYm9keT4KPC9odG1sPg=="));

        [HttpGet("testChartPdf")]
        public virtual async Task<IActionResult> TestChartPdf(Guid? key = null)
        {
            if (key != new Guid("D26689B5-17C5-473A-AA70-BCA9774D763D"))
            {
                return this.BadRequest();
            }

            var svc = this.HttpContext.RequestServices.GetRequiredService<PdfService>();
            var pdfContent = await svc.GeneratePdfAsync(googleChartsSampleHtml, new
            {
                pageSize = "A4",
                orientation = "Portrait",
                windowStatus = "ready", //javascript running in the page MUST set this value in window.status
                marginTop = 0,
                marginRight = 0,
                marginBottom = 0,
                marginLeft = 0
            });

            var file = this.File(pdfContent, "application/pdf");
            return file;
        }

        [HttpGet("hash")]
        public virtual IActionResult Hash(string content, Guid? password = null)
        {
            if (password != new Guid("D26689B5-17C5-473A-AA70-BCA9774D763D"))
            {
                return this.BadRequest();
            }
            return new JsonResult(CommonLibrary.CalculateHash(content));
        }

        [HttpGet("userkey")]
        public virtual IActionResult ServerUserkey(string password)
        {
            const string userKeyHeaderKey = "UserKey";
            var userKey = this.HttpContext.Request.Headers[userKeyHeaderKey].FirstOrDefault();
            userKey = userKey ?? this.HttpContext.Request.Headers[userKeyHeaderKey.ToLower()].FirstOrDefault();

            if (password != _config.ProjectId.Value.ToString())
                return this.Content("password incorrect");

            return this.Content(_keyHelper.UnprotectGuid(userKey).ToString());
        }

        [HttpPost("push")]
        public virtual async Task<IActionResult> TestPush([FromBody]SendNotificationModel model, [FromQuery]Guid? key = null)
        {
            if (key != new Guid("D26689B5-17C5-473A-AA70-BCA9774D763D"))
            {
                return this.BadRequest();
            }

            var result = await _push.SendPushNotification(model);

            return new JsonResult(result);
        }

        [HttpPost]
        [Route("slack")]
        public virtual IActionResult Slack(Guid? password = null)
        {
            if (password == new Guid("3444B9DE-CEE2-4CB9-B8FF-0C1F78973764")) _slack.Channel_Post_Task(null);
            return Ok(new { result = "OK" });
        }


#if DEBUG      
        [HttpPost("emailconcurrency")]
        public IActionResult EmailConcurrency()
        {
            var service = this.HttpContext.RequestServices.GetRequiredService<AccountService>();

            var template = RazorTemplates.WelcomeEmail;

            for (int i = 0; i < 50; i++)
            {
                var model = new RazorTemplateDataModel<RegistrationCompleteViewModel>
                {
                    Data = new RegistrationCompleteViewModel
                    {
                        FirstName = "Developer",
                        Email = "developers@bluechilli.com",
                        Token = Guid.NewGuid().ToString()
                    },
                    TemplateId = template.TemplateName.Substring(template.TemplateName.LastIndexOf("/") + 1),
                    TrackingId = Guid.NewGuid().ToShortGuid()
                };

                service.QueueMail(template, "developers@bluechilli.com", model);
            }

            return new JsonResult(new { message = "Ok" });
        }

        //[HttpPost("dbexception")]
        //public virtual IActionResult ServerDBException()
        //{
        //    var svc = this.HttpContext.RequestServices.GetRequiredService<CompanyService>();
        //    svc.TestDBException();

        //    return Ok();
        //}

        [HttpPost("scalability")]
        public async Task<IActionResult> Scalability(int delay)
        {
            await Task.Delay(delay);

            return new JsonResult(new { message = "Ok" });
        }

        [HttpPost("testupload/{key}")]
        public async Task<IActionResult> TestUploadTimeout(Guid? key, CancellationToken cancellationToken, int? byteDelay = null)
        {
            try
            {
                int bufferSize = 64 * 1024;

                if (!MultipartContentParser.IsMultipartContentType(Request.ContentType))
                {
                    return BadRequest($"Content type not supported.");
                }

                if (key != new Guid("4E5A5216-ECD9-4113-9452-B894608D0A4D"))
                {
                    return BadRequest($"Invalid key");
                }

                var parser = new MultipartContentParser(_formOptions);

                var result = await parser.ParseRequestAsync(Request, async (file, ct) =>
                {
                    int total = 0;

                    try
                    {
                        byte[] buffer = new byte[bufferSize];
                        int read = 0;
                        while ((read = await file.InputStream.ReadAsync(buffer, 0, bufferSize)) > 0)
                        {
                            total += read;
                            if (byteDelay != null)
                            {
                                await Task.Delay(byteDelay.Value);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return "InputStream Error";
                    }

                    return $"OK - {total}";
                }, cancellationToken);

                // Bind form data to a model
                //var formValueProvider = result.GetFormValueProvider(CultureInfo.CurrentCulture);
                //var user = new Model();
                //var bindingSuccessful = await TryUpdateModelAsync(user, prefix: "", valueProvider: formValueProvider);

                return new JsonResult(String.Join("; ", result.FileResults) + ";true; ");
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message + ";false; ");
            }
        }
#endif
    }
}
