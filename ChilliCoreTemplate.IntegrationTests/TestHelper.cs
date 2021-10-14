using ChilliCoreTemplate.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace ChilliCoreTemplate.IntegrationTests
{
    public class TestHelper
    {
        public static IConfigurationRoot GetConfigurationRoot(string outputPath)
        {            
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }

        public static ProjectSettings GetProjectConfiguration(string outputPath)
        {
            var configRoot = GetConfigurationRoot(outputPath);
            return new ProjectSettings(configRoot);
        }
        
        public static string GetTestFolder()
        {
            var startupPath = AppContext.BaseDirectory;
            var pathItems = startupPath.Split(Path.DirectorySeparatorChar);
            var pos = pathItems.Reverse().ToList().FindIndex(x => string.Equals("bin", x));
            var projectPath = String.Join(Path.DirectorySeparatorChar.ToString(), pathItems.Take(pathItems.Length - pos - 1));
            return projectPath;
        }
    }
}