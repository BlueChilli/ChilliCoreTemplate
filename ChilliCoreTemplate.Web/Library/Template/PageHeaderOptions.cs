using ChilliSource.Cloud.Web;
using ChilliSource.Cloud.Web.MVC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Http;

namespace ChilliCoreTemplate.Web
{
    public class PageHeaderOptions
    {
        List<BreadcrumbPathItem> _pathItems = new List<BreadcrumbPathItem>();

        /// <summary>
        /// Override the name of the last item in the breadcrumb, usually to set the name of the entity or the action like Create
        /// Title = Model.Id == 0 ? "Create" : Model.Page
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Heading defaults to title if not set
        /// </summary>
        public string Heading { get; set; }

        /// <summary>
        /// General page description or instructions (can contain html)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Status labels which are rendered after the page title 
        /// </summary>
        public Dictionary<string, BreadcrumbStatus> Status { get; set; } = new Dictionary<string, BreadcrumbStatus>();

        /// <summary>
        /// Name of a partial view to call, which is rendered on the right empty space of the breadcrumb
        /// Usually used to render global action buttons (buttons not related to the data on the page)
        /// </summary>
        public string Partial { get; set; }

        public List<string> Tabs { get; set; } = new List<string>();

        public IReadOnlyList<BreadcrumbPathItem> PathItems { get { return _pathItems; } } 

        public void AddPath(string text, string url = null)
        {
            this.AddPath(new BreadcrumbPathItem()
            {
                Text = text,
                Url = url
            });
        }

        public void AddPath(BreadcrumbPathItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            this._pathItems.Add(item);
        }

        public void AddPath(IUrlHelper urlHelper, IMvcActionDefinition action, string text, string url = null, BreadCrumbPath path = BreadCrumbPath.Before)
        {
            var item = new BreadcrumbPathItem()
            {
                Text = text,
                Url = url
            };
            var index = PathItems.IndexOf(x => x.Url == action.Url(urlHelper));
            if (index < 0) AddPath(item);
            else _pathItems.Insert(index, item);
        }

        public void AlterPath(IUrlHelper urlHelper, IMvcActionDefinition action, string text, string url = null)
        {
            var item = new BreadcrumbPathItem()
            {
                Text = text,
                Url = url
            };
            var index = PathItems.IndexOf(x => x.Url == action.Url(urlHelper));
            if (index < 0) AddPath(item);
            else _pathItems[index] = item;
        }
    }

    public class BreadcrumbPathItem
    {
        public string Text { get; set; }
        public string Url { get; set; }
    }

    public static class BreadcrumbOptionsExtensions
    {
        public static void AddPath(this PageHeaderOptions options, IUrlHelper urlHelper, string text, IMvcActionDefinition mvcAction)
        {
            var url = mvcAction.Url(urlHelper);
            options.AddPath(new BreadcrumbPathItem() { Text = text, Url = url });
        }

        public static void AddDefaultBreadcrumbItems(this PageHeaderOptions options, HttpContext httpContext)
        {
            var currentMenu = MenuConfigByRole.GetCurrentMenu(httpContext);
            if (currentMenu != null)
            {
                var ancestry = currentMenu.GetAncestry(includeSelf: true).Where(a => !a.BreadcrumbHidden).ToList();
                if (ancestry.Count > 0)
                {
                    for (int i = 0; i < ancestry.Count - 1; i++)
                    {
                        var menu = ancestry[i];
                        options.AddPath(new BreadcrumbPathItem() { Text = menu.Title, Url = menu.Url });
                    }

                    //last item
                    var lastItem = ancestry[ancestry.Count - 1];
                    options.AddPath(new BreadcrumbPathItem() { Text = options.Title.DefaultTo(lastItem.Title ?? ""), Url = lastItem.Url });
                }
            }
            else if (!String.IsNullOrEmpty(options.Title))
            {
                options.AddPath(new BreadcrumbPathItem() { Text = options.Title });
            }
        }
    }

    public enum BreadCrumbPath
    {
        Before,
        After
    }
}