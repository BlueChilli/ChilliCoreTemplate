using ChilliCoreTemplate.Models;
using Mjml.Net;
using System.IO;

namespace ChilliCoreTemplate.Service
{
    public static class MjmlToHtmlHelper
    {
        internal static void Render(ref string html)
        {
            if (html.Contains("<mjml>"))
            {
                var mjmlRenderer = new MjmlRenderer();
                var options = new MjmlOptions
                {
                    Beautify = false,
                    FileLoader = new DiskFileLoader()
                };
                var result = mjmlRenderer.Render(html, options);
                html = result.Html;
            }
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
