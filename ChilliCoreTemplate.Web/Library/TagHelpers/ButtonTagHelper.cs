using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Web.TagHelpers;

[HtmlTargetElement("button", Attributes = ActionAttribute)]
public class ButtonTagHelper : ButtonBaseTagHelper
{
    private const string ActionAttribute = "mvc-action";

    public ButtonTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory)
    {
    }

    [HtmlAttributeName(ActionAttribute)]
    public IMvcActionDefinition Action { get; set; }

    [HtmlAttributeName("asp-fragment")]
    public string Fragment { get; set; }

    public ButtonWindow Window { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Action == null)
            return;

        base.Process(context, output);
        output.Attributes.RemoveAll(ActionAttribute);

        var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

        var route = Action.GetRouteValueDictionary();
        route = route.AddRouteValues(RouteValues);

        var url = urlHelper.RouteUrl(null, route, null, null, Fragment);

        if (Type == ButtonType.Button)
        {
            if (Window == ButtonWindow.Current)
                output.Attributes.SetAttribute("onclick", new HtmlString($"window.location='{url}';"));
            else
                output.Attributes.SetAttribute("onclick", new HtmlString($"window.open('{url}');"));
        }
        else
        {
            output.Attributes.SetAttribute("formaction", $"{url}");
        }
    }
}

public enum ButtonWindow
{
    Current,
    New
}

[HtmlTargetElement("button", Attributes = ActionAttribute)]
public class ButtonModalTagHelper : ButtonBaseTagHelper
{
    private const string ActionAttribute = "mvc-modal";

    public ButtonModalTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory)
    {
    }

    [HtmlAttributeName(ActionAttribute)]
    public IMvcActionDefinition Action { get; set; }

    public string JsonData { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Action == null)
            return;

        base.Process(context, output);
        output.Attributes.RemoveAll(ActionAttribute);

        var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
        var url = urlHelper.ModelOpenCommand(Action, new MenuUrlValues { RouteValues = RouteValues }, data: JsonData ?? "null");
        output.Attributes.SetAttribute("onclick", new HtmlString(url));
    }
}

[HtmlTargetElement("button", Attributes = ActionAttribute)]
public class ButtonOffCanvasTagHelper : ButtonBaseTagHelper
{
    private const string ActionAttribute = "mvc-offcanvas";

    public ButtonOffCanvasTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory)
    {
    }

    [HtmlAttributeName(ActionAttribute)]
    public IMvcActionDefinition Action { get; set; }

    [HtmlAttributeName("asp-fragment")]
    public string Fragment { get; set; }

    public string JsonData { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Action == null)
            return;

        base.Process(context, output);
        output.Attributes.RemoveAll(ActionAttribute);

        var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
        if (Fragment != null)
        {
            RouteValues = RouteValues ?? new Dictionary<string, string>();
            RouteValues["fragment"] = Fragment;
        }
        var url = urlHelper.ModelOpenCommand(Action, new MenuUrlValues { RouteValues = RouteValues }, data: JsonData ?? "null", type: "offcanvas");
        output.Attributes.SetAttribute("onclick", new HtmlString(url));
    }
}

[HtmlTargetElement("buttonpost", Attributes = ActionAttribute)]
public class ButtonPostTagHelper : ButtonBaseTagHelper
{
    private const string ActionAttribute = "mvc-action";

    public ButtonPostTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory)
    {
    }

    [HtmlAttributeName(ActionAttribute)]
    public IMvcActionDefinition Action { get; set; }

    public string JsonData { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Action == null)
            return;

        output.TagName = "button";

        base.Process(context, output);
        output.Attributes.RemoveAll(ActionAttribute);
        var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

        var route = Action.GetRouteValueDictionary();
        route = route.AddRouteValues(RouteValues);

        var url = urlHelper.RouteUrl(route);
        var target = "";
        if (output.Attributes.ContainsName("target"))
        {
            target = output.Attributes["target"].Value.ToString();
            output.Attributes.RemoveAll("target");
        }
        output.Attributes.SetAttribute("onclick", $"$.doPost('{url}', '{target}', {JsonData ?? "null"});");
    }
}

public abstract class ButtonBaseTagHelper : TagHelper
{
    protected readonly IUrlHelperFactory _urlHelperFactory;

    public ButtonBaseTagHelper(IUrlHelperFactory urlHelperFactory)
    {
        _urlHelperFactory = urlHelperFactory;
    }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public ButtonType Type { get; set; } = ButtonType.Button;

    public ButtonStyle Style { get; set; }

    public ButtonSize Size { get; set; } = ButtonSize.Small;

    public IconType Icon { get; set; }

    [HtmlAttributeName("icon-placement")]
    public IconPlacement IconPlacement { get; set; }

    public string Tooltip { get; set; }

    [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
    public IDictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("type", Type.ToString().ToLower());

        var iconStyle = String.Empty;
        if (Icon != IconType.None)
        {
            iconStyle = IconPlacement == IconPlacement.None ? "btn-square" : "";
            output.Attributes.SetAttribute("data-bs-toggle", "tooltip");
            output.Attributes.SetAttribute("data-bs-original-title", Tooltip ?? Icon.GetDescription());
            var icon = $"<i class=\"bi bi-{Icon.GetData<string>("Icon")}\"></i>";
            if (IconPlacement == IconPlacement.None)
                output.Content.SetHtmlContent(icon);
            else if (IconPlacement == IconPlacement.Left)
            {
                output.PreContent.SetHtmlContent($"<span class=\"pe-3\">{icon}</span>");
            }
            else
                output.PostContent.SetHtmlContent(icon);
        }

        output.Attributes.AppendAttribute("class", $"btn btn-{Style.GetDescription().ToLower()} {Size.GetData<string>("css")} {iconStyle}");
    }
}

[HtmlTargetElement("buttonsubmit")]
public class ButtonSubmitTagHelper : TagHelper
{
    public ButtonSubmitTagHelper()
    {
    }

    public IMvcActionDefinition Form { get; set; }

    public ButtonStyle Style { get; set; } = ButtonStyle.Primary;

    public ButtonSize Size { get; set; } = ButtonSize.Small;

    public bool ShowSpinner { get; set; }

    [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
    public IDictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "button";
        output.Attributes.Add("type", "submit");

        if (Form != null)
        {
            output.Attributes.Add("form", Form.GetFormId());
        }

        if (RouteValues.Count > 0)
        {
            var first = RouteValues.First();
            output.Attributes.Add("name", first.Key);
            output.Attributes.Add("value", first.Value);
        }

        output.Attributes.AppendAttribute("class", $"btn btn-{Style.GetDescription().ToLower()} {Size.GetData<string>("css")}");

        if (ShowSpinner)
        {
            output.PostContent.SetHtmlContent("<span class=\"spinner-border spinner-border-sm ms-2\"></span>");
        }
    }
}
