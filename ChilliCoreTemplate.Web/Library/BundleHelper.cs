using ChilliSource.Cloud.Web.MVC;
using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ChilliCoreTemplate.Web
{
    public static class BundleHelper
    {
        private const string BundleConfigFile = "bundleconfig.json";
        static readonly Dictionary<string, List<string>> _paths = new Dictionary<string, List<string>>();

        public static IHtmlContent RenderBundle(this IHtmlHelper html, string bundleSrc)
        {
            if (!bundleSrc.StartsWith("~/"))
                throw new ArgumentException(bundleSrc);

            var serviceProvider = html.ViewContext.HttpContext.RequestServices;
            var settings = serviceProvider.GetRequiredService<ProjectSettings>();
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();

            if (settings.UnbundledFiles)
            {
                var files = GetFilePaths(env, bundleSrc);
                var builder = new HtmlContentBuilder(files.Count);

                foreach (var file in files)
                {
                    builder.AppendHtml(BuildTag(html, file));
                }

                return builder;
            }
            else
            {
                return BuildTag(html, bundleSrc);
            }
        }

        private static IHtmlContent BuildTag(IHtmlHelper html, string src)
        {
            var versionedSource = GetVersionedSrc(html, src);
            var tag = Path.GetExtension(src) == ".js" ? $"<script src=\"{versionedSource}\"></script>\r\n" : $"<link rel=\"stylesheet\" href=\"{versionedSource}\"/>\r\n";
            return MvcHtmlStringCompatibility.Create(tag);
        }

        private static List<string> GetFilePaths(IWebHostEnvironment env, string bundleSrc)
        {
            if (!_paths.ContainsKey(bundleSrc))
            {
                var paths = new List<string>();
                var bundleConfigFile = Path.Combine(env.ContentRootPath, BundleConfigFile);
                var bundleConfig = (JArray)JToken.Parse(File.ReadAllText(bundleConfigFile));
                if (bundleConfig == null)
                    throw new ApplicationException("Bundle config file not found or invalid");

                var matchKey = $"wwwroot{bundleSrc.TrimStart('~')}";
                foreach (var jobject in bundleConfig)
                {
                    var outputFileName = (string)jobject["outputFileName"];
                    if (matchKey.Equals(outputFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        paths = (jobject["inputFiles"] as JArray).SelectMany(v =>
                        {
                            var files = ResolveWildcardFiles(env, (string)v);
                            return files.Select(file =>
                            {
                                if (file.StartsWith("wwwroot/"))
                                {
                                    file = file.Substring("wwwroot/".Length);
                                }                                

                                return $"~/{file}";
                            });
                        }).ToList();

                        break;
                    }
                }

                _paths.Add(bundleSrc, paths ?? new List<string>());
            }

            return _paths[bundleSrc];
        }

        private static IEnumerable<string> ResolveWildcardFiles(IWebHostEnvironment env, string relativePath)
        {
            if (!relativePath.Contains("*"))
            {
                yield return relativePath;
                yield break;
            }

            string pattern = Path.GetFileName(relativePath);
            string relDir = relativePath.Substring(0, relativePath.Length - pattern.Length);

            string absPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, relDir));

            // Search files mathing the pattern
            string[] files = Directory.GetFiles(absPath, pattern, SearchOption.TopDirectoryOnly);
            foreach (var file in files.OrderBy(f => f))
            {
                yield return $"{relDir}{Path.GetFileName(file)}";
            }
        }

        private static string GetVersionedSrc(IHtmlHelper html, string srcValue)
        {
            var context = html.ViewContext.HttpContext;            
            var fileVersionProvider = context.RequestServices.GetRequiredService<IFileVersionProvider>();
            var pathBase = context.Request.PathBase;

            var versionedSrc = fileVersionProvider.AddFileVersionToPath(pathBase, srcValue.TrimStart('~'));
            return $"{pathBase.Value}{versionedSrc}";
        }
    }
}
