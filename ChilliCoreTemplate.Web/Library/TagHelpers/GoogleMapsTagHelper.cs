using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.TagHelpers
{

    [HtmlTargetElement("googleMaps")]
    public class GoogleMapsTagHelper : TagHelper
    {
        public string Libraries { get; set; } //places for autocomplete

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var apiKey = ViewContext.HttpContext.RequestServices.GetService<ProjectSettings>().GoogleApis.ApiKey;

            output.TagName = "script";

            var includeLibraries = String.IsNullOrEmpty(Libraries) ? "" : $"&libraries={Libraries}";
            output.Attributes.SetAttribute("src", $"https://maps.googleapis.com/maps/api/js?key={apiKey}&callback=initGoogleMap{includeLibraries}");
            output.Attributes.SetAttribute("async", "");
            output.Attributes.SetAttribute("defer", "");
        }
    }
}
