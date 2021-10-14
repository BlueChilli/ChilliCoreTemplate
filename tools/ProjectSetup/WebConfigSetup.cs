using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ProjectSetup
{
    public class WebConfigSetup
    {
        ExecutionLogger _logger;
        WebConfigOptions _options;

        public WebConfigSetup(WebConfigOptions options, ExecutionLogger logger)
        {
            _logger = logger;
            _options = options;
        }

        public void Run()
        {
            ReplaceWebConfig();
            ReplaceWebRelaseConfig();
        }

        private void ReplaceWebConfig()
        {
            _logger.Log("****** Web.config Step ******");

            if (!File.Exists(_options.WebConfigPath))
            {
                _logger.Log($"Web.config not found at {_options.WebConfigPath}");
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(_options.WebConfigPath);

            XmlNode projectConfig;
            try
            {
                projectConfig = xmlDoc.SelectSingleNode("configuration//BlueChilliProjectConfiguration");
            }
            catch
            {
                projectConfig = null;
            }

            if (projectConfig == null)
            {
                _logger.Log("Skipping Web.config");
                return;
            }
            else
            {
                _logger.Log("BlueChilliProjectConfiguration section");
                SetAttribute(projectConfig, "baseUrl", $"https://localhost/{_options.SolutionName.ToLower()}");
                SetAttribute(projectConfig, "projectId", Guid.NewGuid().ToString());
                SetAttribute(projectConfig, "projectName", _options.SolutionName);
                SetAttribute(projectConfig, "projectDisplayName", _options.SolutionName);
            }

            _logger.Log("machine key section");
            var machineKey = xmlDoc.SelectSingleNode("configuration//system.web//machineKey");
            SetAttribute(machineKey, "decryptionKey", HexaUtils.GetRandomHexaString(24));
            SetAttribute(machineKey, "validationKey", HexaUtils.GetRandomHexaString(68));

            _logger.Log("api section");

            var api = xmlDoc.SelectSingleNode("configuration//api");
            SetAttribute(api, "apikey", Guid.NewGuid().ToString());

            _logger.Log("sms section");
            var sms = xmlDoc.SelectSingleNode("configuration//sms");
            SetAttribute(sms, "username", "");
            SetAttribute(sms, "password", "");
            SetAttribute(sms, "from", "");

            _logger.Log("googleApis section");
            var googleApi = xmlDoc.SelectSingleNode("configuration//BlueChilliProjectConfiguration//googleApis");
            SetAttribute(googleApi, "apiKey", "");
            SetAttribute(googleApi, "devApiKey", ""); //old version
            SetAttribute(googleApi, "prodApiKey", ""); //old version

            _logger.Log("filestorage section");
            var s3 = xmlDoc.SelectSingleNode("configuration//BlueChilliProjectConfiguration//filestorage//s3");
            SetAttribute(s3, "accessKeyId", "");
            SetAttribute(s3, "secretAccessKey", "");

            var azure = xmlDoc.SelectSingleNode("configuration//BlueChilliProjectConfiguration//filestorage//azure");
            SetAttribute(azure, "accountName", "");
            SetAttribute(azure, "accountKey", "");

            _logger.Log("resizer section");
            var resizerPlugings = xmlDoc.SelectSingleNode("configuration//resizer//plugins");
            var s3Reader = resizerPlugings.SelectNodes("add").Cast<XmlNode>().Where(n => n.Attributes["name"]?.Value == "S3Reader2").FirstOrDefault();
            SetAttribute(s3Reader, "accessKeyId", "");
            SetAttribute(s3Reader, "secretAccessKey", "");

            xmlDoc.Save(_options.WebConfigPath);
        }

        private void ReplaceWebRelaseConfig()
        {
            _logger.Log("****** Web.Release.config Step ******");

            var webReleasePath = _options.WebConfigPath.Replace("Web.config", "Web.Release.config");
            if (!File.Exists(webReleasePath))
            {
                _logger.Log($"Web.Release.config not found at {webReleasePath}");
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(webReleasePath);

            XmlNode machineKey;
            try
            {
                machineKey = xmlDoc.SelectSingleNode("configuration//system.web//machineKey");
            }
            catch
            {
                machineKey = null;
            }

            if (machineKey == null)
            {
                _logger.Log("Skipping Web.Release.config");
                return;
            }
            else
            {
                _logger.Log("machine key section");
                SetAttribute(machineKey, "decryptionKey", HexaUtils.GetRandomHexaString(24));
                SetAttribute(machineKey, "validationKey", HexaUtils.GetRandomHexaString(68));
            }

            _logger.Log("api section");

            var api = xmlDoc.SelectSingleNode("configuration//api");
            SetAttribute(api, "apikey", Guid.NewGuid().ToString());

            xmlDoc.Save(webReleasePath);
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

    public class WebConfigOptions
    {
        public string WebConfigPath { get; set; }
        public string SolutionName { get; set; }
    }

    public class HexaUtils
    {
        public static string GetRandomHexaString(int bytes)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var buffer = new byte[bytes];
                rng.GetBytes(buffer);

                return HexaUtils.ByteArrayToString(buffer);
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:X2}", b);
            return hex.ToString();
        }
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
