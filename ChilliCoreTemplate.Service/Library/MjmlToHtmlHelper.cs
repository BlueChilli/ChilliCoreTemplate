using ChilliCoreTemplate.Models;
using Mjml.Net;
using System.IO;
using System.Web;

namespace ChilliCoreTemplate.Service
{
    public static class MjmlToHtmlHelper
    {
        internal static string Render(string html)
        {
            if (html.Contains("<mjml>"))
            {
                var mjmlRenderer = new MjmlRenderer();
                var options = new MjmlOptions
                {
                    Beautify = false,
                    FileLoader = () => new DiskFileLoader()
                };
                var result = mjmlRenderer.Render(HttpUtility.HtmlDecode(html), options);

                return result.Html;            
            }
            return html;
        }
    }

    public class DiskFileLoader : IFileLoader
    {
        private string basePath = MyServer.MapPath("Views\\Emails\\Include\\");

        private string GetPath(string path) => Path.Combine(basePath, Path.GetFileName(path));

        public bool ContainsFile(string path)
        {
            return File.Exists(GetPath(path));
        }

        public string LoadText(string path)
        {
            return File.ReadAllText(GetPath(path));
        }
    }
}
