using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public static class MenuConfigByRole
    {
        private delegate void ConfigAction(Configuration config);

        private static ConfigAction _default = (cfg) => { };
        private static Dictionary<Role, ConfigAction> _configs = new Dictionary<Role, ConfigAction>();
        private static List<Role> _roles = new List<Role>();

        static MenuConfigByRole()
        {
            //Configuration.UserData and Configuration.HttpContext properties are available in the setup actions

            MenuConfigByRole.DefaultConfig(cfg =>
            {
                cfg.AddRoot(Mvc.Root.Public_Index);
            });

            MenuConfigByRole.Config(Role.Administrator, cfg =>
            {
                cfg.AddRoot(Mvc.Admin.Company_List, title: "Companies", icon: "fa-city").SetChildren(
                    cfg.CreateBreadcrumb(Mvc.Admin.Company_Edit),
                    cfg.CreateBreadcrumb(Mvc.Admin.Company_Detail)
                );

                cfg.AddRoot(Mvc.Admin.User_Users, icon: "fa-users").SetChildren(
                        cfg.Create(Mvc.Admin.User_Users, breadcrumbHidden: true).SetChildren(
                            cfg.CreateBreadcrumb(Mvc.Admin.User_Users_Details),
                            cfg.CreateBreadcrumb(Mvc.Admin.User_Invite)
                        ),
                        cfg.Create(Mvc.Admin.User_Statistics),
                        cfg.Create(Mvc.Admin.User_Activity),
                        cfg.Create(Mvc.Admin.User_Emails).SetChildren(cfg.CreateBreadcrumb(Mvc.Admin.User_EmailsPreview, title: "Preview")),
                        cfg.Create(Mvc.Admin.User_Sms_List, title: "Sms"),
                        cfg.Create(Mvc.Admin.User_Notification_List, title: "Notifications"),
                        cfg.Create(Mvc.Admin.User_Error_List, title: "Errors").SetChildren(cfg.CreateBreadcrumb(Mvc.Admin.User_Error_Detail))
                    );

            });

            MenuConfigByRole.Config(Role.CompanyAdmin, cfg =>
            {
                cfg.AddRoot(Mvc.Company.User_List, title: "Users", icon: "fa-users").SetChildren(
                        cfg.CreateBreadcrumb(Mvc.Company.User_Detail),
                        cfg.CreateBreadcrumb(Mvc.Company.User_Invite)                       
                    );

                cfg.AddRoot(Mvc.Company.Location_List, title: "Locations", icon: "fa-building").SetChildren(
                    cfg.CreateBreadcrumb(Mvc.Company.Location_Edit)
                );
            });
        }

        private static IReadOnlyList<MenuElement> GetElements(ConfigAction action, UserData user, HttpContext httpContext)
        {
            var config = new Configuration(user, httpContext);
            action(config);

            var elements = config.GetElements();
            CheckIsActiveConsistency(elements);

            return elements;
        }

        public static MenuElement GetCurrentMenu(HttpContext httpContext)
        {
            var menus = GetRootMenus(httpContext);
            var active = FindActive(menus);

            return active ?? GetElements(_default, null, httpContext).FirstOrDefault();
        }

        private static MenuElement FindActive(IReadOnlyList<MenuElement> elements)
        {
            if (elements.Count == 0)
                return null;

            foreach (var element in elements)
            {
                var childActive = FindActive(element.GetChildren());
                if (childActive != null)
                    return childActive;

                if (element.IsActive)
                    return element;
            }

            return null;
        }

        private const string CacheKey = "__MenuConfigByRole_RootMenus";

        public static void ClearCache(HttpContext httpContext)
        {
            httpContext.Items.Remove(CacheKey);
        }

        public static IReadOnlyList<MenuElement> GetRootMenus(HttpContext httpContext)
        {
            var menus = httpContext.Items[CacheKey] as IReadOnlyList<MenuElement>;
            if (menus == null)
            {
                httpContext.Items[CacheKey] = menus = GetRootMenusInternal(httpContext);
            }

            return menus;
        }

        private static bool CheckIsActiveConsistency(IReadOnlyList<MenuElement> menus)
        {
            if (menus.Count == 0)
                return false;

            bool anyActive = false;
            foreach (var menu in menus)
            {
                if (!menu.IsActive)
                {
                    menu.IsActive = CheckIsActiveConsistency(menu.GetChildren());
                }

                anyActive = anyActive | menu.IsActive;
            }

            return anyActive;
        }

        private static IReadOnlyList<MenuElement> GetRootMenusInternal(HttpContext httpContext)
        {
            var userData = httpContext.User.UserData();
            if (userData != null)
            {
                foreach (var role in _roles)
                {
                    if (userData.IsInRole(role))
                    {
                        return GetElements(_configs[role], userData, httpContext);
                    }
                }
            }

            return GetElements(_default, userData, httpContext);
        }

        private static void DefaultConfig(ConfigAction configAction)
        {
            if (configAction == null)
                throw new ArgumentNullException(nameof(configAction));

            _default = configAction;
        }

        private static void Config(Role role, ConfigAction configAction)
        {
            if (configAction == null)
                throw new ArgumentNullException(nameof(configAction));

            _configs.Add(role, configAction);
            _roles.Add(role); //preserve order            
        }

        private class Configuration
        {
            List<MenuElement> _elements = new List<MenuElement>();
            public UserData UserData { get; }
            public HttpContext HttpContext { get; }
            public IUrlHelper UrlHelper { get; }
            public ActionContext ActionContext { get; }
            public ControllerActionDescriptor ControllerDescriptor { get; }

            public Configuration(UserData userData, HttpContext httpContext)
            {
                this.UserData = userData;
                this.HttpContext = httpContext;
                this.UrlHelper = HttpContext.GetUrlHelper();
                this.ActionContext = this.HttpContext.RequestServices.GetRequiredService<IActionContextAccessor>()?.ActionContext;
                this.ControllerDescriptor = ActionContext?.ActionDescriptor as ControllerActionDescriptor;
            }

            public IReadOnlyList<MenuElement> GetElements() { return _elements; }

            internal MenuElement AddRoot(MenuElement element)
            {
                _elements.Add(element);
                return element;
            }

            internal MenuElement Create(IMvcActionDefinition actionResult, string title = null, string icon = null, bool? isActive = null, string url = null, object routeValues = null, bool menuHidden = false, bool breadcrumbHidden = false)
            {
                var actionRoute = actionResult.GetRouteValueDictionary();
                var actionName = (actionRoute["action"] as string).SplitByUppercase();

                var element = new MenuElement();

                if (title != null)
                {
                    element.Title = title;
                }
                else
                {
                    element.Title = actionName;
                }

                if (url != null)
                {
                    element.Url = url;
                }
                else
                {
                    element.Url = actionResult.Url(this.UrlHelper, routeValues: routeValues);
                }

                if (isActive != null)
                {
                    element.IsActive = isActive.Value;
                }
                else
                {
                    element.IsActive = IsActive(actionResult, routeValues);
                }

                if (icon != null) element.Icon = icon;

                element.MenuHidden = menuHidden;
                element.BreadcrumbHidden = breadcrumbHidden;

                return element;
            }

            internal MenuElement CreateBreadcrumb(IMvcActionDefinition actionResult, string title = null, string icon = null, bool? isActive = null, string url = null, object routeValues = null)
            {
                return Create(actionResult, title: title, icon: icon, isActive: isActive, url: url, routeValues: routeValues, menuHidden: true);
            }

            private bool IsActive(IMvcActionDefinition actionResult, object routeValues)
            {
                var actionRoute = actionResult.GetRouteValueDictionary();
                var actionName = actionRoute["action"] as string;
                var controllerName = actionRoute["controller"] as string;
                var areaName = actionRoute["area"] as string;

                if (ControllerDescriptor != null && ControllerDescriptor.ActionName == actionName
                    && ControllerDescriptor.ControllerName == controllerName
                    && GetControllerDescriptorRoute("area") == areaName)
                {
                    if (routeValues != null)
                    {
                        var routeDict = new RouteValueDictionary(routeValues);
                        foreach (var key in routeDict.Keys)
                        {
                            var value = (string)routeDict[key];
                            if (GetControllerDescriptorRoute(key) != value && !IsValidRequestQueryString(key, value))
                                return false;
                        }
                    }

                    return true;
                }

                return false;
            }

            private string GetControllerDescriptorRoute(string key)
            {
                if (ControllerDescriptor != null && ControllerDescriptor.RouteValues.ContainsKey(key))
                {
                    return ControllerDescriptor.RouteValues[key] ?? String.Empty;
                }

                return String.Empty;
            }

            private bool IsValidRequestQueryString(string key, string value)
            {
                if (this.ActionContext != null)
                {
                    var queryString = this.ActionContext.HttpContext.Request.Query[key];
                    return queryString.Any(s => String.Equals(s, value, StringComparison.OrdinalIgnoreCase));
                }

                return false;
            }

            internal MenuElement AddRoot(IMvcActionDefinition actionResult, string title = null, string icon = null, bool? isActive = null, string url = null, object routeValues = null)
            {
                var element = Create(actionResult, title: title, icon: icon, isActive: isActive, url: url, routeValues: routeValues);

                return this.AddRoot(element);
            }
        }
    }

    public class MenuElement
    {
        private List<MenuElement> _children = new List<MenuElement>();
        public string Title { get; internal set; }
        public string Icon { get; internal set; }

        public bool IsActive { get; internal set; }
        public string Url { get; internal set; }

        public MenuElement Parent { get; private set; }

        public bool BreadcrumbHidden { get; internal set; }
        public bool MenuHidden { get; internal set; }

        public MenuElement() { }

        public IReadOnlyList<MenuElement> GetChildren()
        {
            return _children;
        }

        public MenuElement SetChildren(IEnumerable<MenuElement> children)
        {
            _children = (children == null) ? new List<MenuElement>() : children.Where(c => c != null).ToList();
            foreach (var child in _children)
            {
                child.Parent = this;
            }

            return this;
        }

        public MenuElement SetChildren(params MenuElement[] children)
        {
            return SetChildren((IEnumerable<MenuElement>)children);
        }

        public IEnumerable<MenuElement> GetAncestry(bool includeSelf)
        {
            var ancestry = this.Parent == null ? Enumerable.Empty<MenuElement>()
                            : this.Parent.GetAncestry(includeSelf: true);

            if (includeSelf)
            {
                ancestry = ancestry.Concat(ThisAsEnumerable());
            }

            return ancestry;
        }

        private IEnumerable<MenuElement> ThisAsEnumerable()
        {
            yield return this;
        }
    }
}