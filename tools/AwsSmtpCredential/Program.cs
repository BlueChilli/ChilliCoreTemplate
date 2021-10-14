using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AwsSmtpCredential
{
    public class Args
    {
        public string AWSAccessKey { get; set; }
        public string FilePath { get; set; }
    }

    class Program
    {
        static int Main(string[] args)
        {
            var parsedArgs = ParseArgs(args);
            if (!String.IsNullOrEmpty(parsedArgs.AWSAccessKey))
            {
                var smptPassword = GetSmptPassword(parsedArgs.AWSAccessKey);

                Console.Write(smptPassword);
                return 0;
            }
            else if (!String.IsNullOrEmpty(parsedArgs.FilePath))
            {
                var secretAccessKey = ReadSecretAccessKey(parsedArgs.FilePath);
                if (String.IsNullOrEmpty(secretAccessKey))
                {
                    Console.WriteLine("SecretAccessKey not found in json file: " + parsedArgs.FilePath);
                    return 1;
                }

                var smptPassword = GetSmptPassword(secretAccessKey);

                Console.Write(smptPassword);
                return 0;
            }

            Console.WriteLine("Usage:");
            Console.WriteLine("AwsSmtpCredential [SecretAccessKey] | [-file] [path]");
            return 1;
        }

        static Args ParseArgs(string[] args)
        {
            var parsed = new Args();
            if (args == null || args.Length == 0 || args.Length > 2)
                return parsed;

            if ("-file".Equals(args[0], StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length == 2)
                {
                    var quotes = new char[] { '\"' };

                    parsed.FilePath = args[1].TrimStart(quotes).TrimEnd(quotes);
                }
            }
            else if (args.Length == 1)
            {
                parsed.AWSAccessKey = args[0];
            }

            return parsed;
        }

        static string ReadSecretAccessKey(string filePath)
        {
            try
            {
                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(file))
                {
                    var content = reader.ReadToEnd();
                    var json = JObject.Parse(content);

                    var secret = (string)((json["AccessKey"] as JObject)?["SecretAccessKey"]);
                    return secret;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static string GetSmptPassword(string key)
        {
            var message = "SendRawEmail";
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var sha = new HMACSHA256(keyBytes))
            {
                var signatureBytes = sha.ComputeHash(messageBytes, 0, messageBytes.Length);
                var versionAndSig = new byte[1 + signatureBytes.Length];

                versionAndSig[0] = 0x02; //version
                Array.Copy(signatureBytes, 0, versionAndSig, 1, signatureBytes.Length);

                return Convert.ToBase64String(versionAndSig);
            }
        }
    }
}
