using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CodeCloner
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

            _logger.Log("Renaming files");

            if (_options.FilesToRename?.Length > 0)
            {
                foreach (var fileType in _options.FilesToRename)
                {
                    var files = GetSolutionFiles(_options.SourceDirectory, fileType, SearchOption.AllDirectories).ToList();
                    foreach (var file in files)
                    {
                        if (Path.GetFileName(file).Contains(_options.FromName))
                        {
                            var newName = Path.Combine(Path.GetDirectoryName(file), Path.GetFileName(file).Replace(_options.FromName, _options.ToName));
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
                        var newText = text.Replace(_options.FromName, _options.ToName);
                        newText = newText.Replace(_options.FromName.ToLower(), _options.ToName.ToLower());

                        if (_options.ToName + "s" != _options.PluralName)
                        {
                            newText = newText.Replace(_options.ToName + "s", _options.PluralName);
                            newText = newText.Replace(_options.ToName.ToLower() + "s", _options.PluralName.ToLower());
                        }

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

        public string FromName { get; set; }
        public string ToName { get; set; }
        public string PluralName { get; set; }

        public string[] FilesToRename { get; set; }
        public string[] FilesToSearch { get; set; }
    }
}
