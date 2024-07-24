using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core.Distributed;
using System;
using System.IO;

namespace ChilliCoreTemplate.Service;

public class SystemService : IService
{

    public static string GetDocumentCachePath()
    {
        return MyServer.MapPath("DocumentCache");
    }
    public void DocumentCache_CleanUp(ITaskExecutionInfo executionInfo)
    {
        if (executionInfo != null)
        {
            executionInfo.SendAliveSignal();
            if (executionInfo.IsCancellationRequested) return;
        }

        var cacheFolder = GetDocumentCachePath();
        var folders = Directory.EnumerateDirectories(cacheFolder);
        foreach (var folder in folders)
        {
            var created = Directory.GetCreationTime(folder);
            if (created < DateTime.UtcNow.AddDays(-7)) Directory.Delete(folder, true);
        }
        var files = Directory.EnumerateFiles(cacheFolder, "*.*");
        foreach (var file in files)
        {
            if (file.EndsWith(".gitignore") || file.EndsWith("cache.txt")) continue;
            var created = File.GetCreationTime(file);
            if (created < DateTime.UtcNow.AddDays(-7)) File.Delete(file);
        }
    }
}
