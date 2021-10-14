using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ProjectSetup
{
    public class Renamer
    {
        RenamerOptions _options;
        ExecutionLogger _logger;

        public Renamer(RenamerOptions options, ExecutionLogger logger)
        {
            _options = options;
            _logger = logger;
        }

        public void Run()
        {
            _logger.Log("****** Rename Step ******");

            _logger.Log("Removing bin and obj directories");
            var deleteDirs = Directory.GetDirectories(_options.SourceDirectory, $"bin", SearchOption.AllDirectories)
                            .Concat(Directory.GetDirectories(_options.SourceDirectory, $"obj", SearchOption.AllDirectories))
                            .Where(s => s.Contains(_options.SourceSolutionName) || s.Contains(_options.DestSolutionName))
                            .Where(s => !s.Contains($"tools{Path.DirectorySeparatorChar}")) // removing tools directory from search
                            .ToList();

            foreach (var dir in deleteDirs)
            {
                try
                {
                    Directory.Delete(dir, recursive: true);
                }
                catch (Exception ex)
                {
                    _logger.Log($"Failed to remove directory: {dir}");
                }
            }


            _logger.Log("Renaming directories");

            if (_options.RenameDirectories)
            {
                var directories = Directory.GetDirectories(_options.SourceDirectory, $"*{_options.SourceSolutionName}*");
                foreach (var directory in directories)
                {
                    var newName = Path.Combine(Path.GetDirectoryName(directory), Path.GetFileName(directory).Replace(_options.SourceSolutionName, _options.DestSolutionName));
                    if (directory != newName) Directory.Move(directory, newName);
                }
            }

            _logger.Log("Renaming files");

            if (_options.FilesToRename?.Length > 0)
            {
                foreach (var fileType in _options.FilesToRename)
                {
                    var files = GetSolutionFiles(_options.SourceDirectory, fileType, SearchOption.AllDirectories).ToList();
                    foreach (var file in files)
                    {
                        if (Path.GetFileName(file).Contains(_options.SourceSolutionName))
                        {
                            var newName = Path.Combine(Path.GetDirectoryName(file), Path.GetFileName(file).Replace(_options.SourceSolutionName, _options.DestSolutionName));
                            if (file != newName) File.Move(file, newName);
                        }
                    }
                }
            }

            _logger.Log("Search and replace text");

            if (_options.FilesToSearch?.Length > 0)
            {
                foreach (var fileType in _options.FilesToSearch)
                {
                    var files = GetSolutionFiles(_options.SourceDirectory, fileType, SearchOption.AllDirectories).ToList();
                    foreach (var file in files)
                    {
                        string text = File.ReadAllText(file);
                        var newText = text.Replace(_options.SourceSolutionName, _options.DestSolutionName);
                        if (text != newText) File.WriteAllText(file, newText);
                    }
                }
            }
        }

        private List<string> GetSolutionFiles(string baseDirectory, string fileType, SearchOption allDirectories)
        {
            return Directory.GetFiles(_options.SourceDirectory, fileType, SearchOption.AllDirectories)
                    .Where(f => !f.Contains($"tools{Path.DirectorySeparatorChar}")) // removing tools directory from search
                    .ToList();
        }
    }

    public class RenamerOptions
    {
        public string SourceDirectory { get; set; }

        public string SourceSolutionName { get; set; }
        public string DestSolutionName { get; set; }

        public string[] FilesToRename { get; set; }
        public string[] FilesToSearch { get; set; }

        public bool RenameDirectories { get; set; }
    }
}
