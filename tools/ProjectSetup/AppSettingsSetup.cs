using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ProjectSetup
{
    public class AppSettingsSetup
    {
        ExecutionLogger _logger;
        AppSettingsOptions _options;

        public AppSettingsSetup(AppSettingsOptions options, ExecutionLogger logger)
        {
            _logger = logger;
            _options = options;
        }

        public void Run()
        {
            AppSettingsConfig();
            PublishFileConfig();
        }

        private void AppSettingsConfig()
        {
            _logger.Log("****** appsettings.json Step ******");

            var appSettingsPath = Path.Combine(_options.WebProjectFolder, "appsettings.json");
            if (!File.Exists(appSettingsPath))
            {
                _logger.Log($"appsettings.json not found at {appSettingsPath}");
                return;
            }

            var json = JObject.Parse(File.ReadAllText(appSettingsPath), new JsonLoadSettings() { CommentHandling = CommentHandling.Load });

            _logger.Log("ProjectSettings section");

            json.PropertyAt("ProjectSettings:Base", "ProjectId").Set(Guid.NewGuid().ToString());
            json.PropertyAt("ProjectSettings:Base", "ProjectName").Set(_options.SolutionName);
            json.PropertyAt("ProjectSettings:Base", "ProjectDisplayName").Set(_options.SolutionName);
            json.PropertyAt("ProjectSettings:Api", "ApiKey").Set(Guid.NewGuid().ToString());

            var newContent = json.ToString(Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(appSettingsPath, newContent);
        }

        private void PublishFileConfig()
        {
            _logger.Log("****** Publish profile Step ******");

            var publishFilePath = Path.Combine(_options.WebProjectFolder, @"Properties\PublishProfiles\Profile.pubxml");
            if (!File.Exists(publishFilePath))
            {
                _logger.Log($"Publish profile not found at {publishFilePath}");
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(publishFilePath);
            var ns = new XmlNamespaceManager(xmlDoc.NameTable);
            ns.AddNamespace("pubns", xmlDoc.DocumentElement.Attributes["xmlns"].Value);

            var projectGuid = xmlDoc.SelectSingleNode("//pubns:Project//pubns:PropertyGroup//pubns:ProjectGuid", ns);
            if (projectGuid == null)
                return;

            _logger.Log("ProjectGuid section");
            projectGuid.InnerText = Guid.NewGuid().ToString();

            xmlDoc.Save(publishFilePath);
        }

        private void SetAttribute(XmlNode node, string name, string value)
        {
            if (node == null)
                return;

            var attribute = node.Attributes[name];
            if (attribute == null)
                return;

            attribute.Value = value;
        }
    }

    public static class JsonUtils
    {
        public static JProperty PropertyAt(this JObject obj, params string[] properties)
        {
            var jobj = obj;
            JProperty jProperty = null;
            foreach (var propertyName in properties)
            {
                jProperty = jobj?.Property(propertyName);
                if (jProperty == null)
                    return null;

                jobj = jProperty.Value as JObject;
            }

            return jProperty;
        }

        public static void Set(this JProperty property, JToken value)
        {
            if (property == null)
                return;

            property.Value = value;
        }
    }

    public class AppSettingsOptions
    {
        public string WebProjectFolder { get; set; }
        public string SolutionName { get; set; }
    }
}
