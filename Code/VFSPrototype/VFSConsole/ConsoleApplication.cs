using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFSBase;

namespace VFSConsole
{
    public class ConsoleApplication
    {
        private readonly TextReader _textReader;
        private readonly TextWriter _textWriter;
        private volatile bool _running = true;
        private readonly IDictionary<string, Action<string>> _commands;
        private readonly IFileSystemManipulator _fileSystemManipulator;
        private string _currentDirectory = "";

        public ConsoleApplication(ConsoleApplicationSettings consoleApplicationSettings, IFileSystemManipulator fileSystemManipulator)
        {
            if (fileSystemManipulator == null) throw new ArgumentNullException("fileSystemManipulator", "fileSystem must not be null.");

            _fileSystemManipulator = fileSystemManipulator;

            _textReader = consoleApplicationSettings.TextReader;
            _textWriter = consoleApplicationSettings.TextWriter;

            _commands = new Dictionary<string, Action<string>>
                {
                    {"cd", Cd},
                    {"delete", Delete},
                    {"exists", Exists},
                    {"exit", Exit},
                    {"help", ShowHelp},
                    {"import", Import},
                    {"ls", ListDirectory},
                    {"mkdir", Mkdir},
                };
        }

        public void Run()
        {
            while (_running)
            {
                _textWriter.Write(Prompt);
                var line = _textReader.ReadLine();
                ProcessLine(line);
            }
            _textWriter.WriteLine("kthxbye");
            _textReader.ReadLine();
        }

        private void ProcessLine(string line)
        {
            var commandAndArguments = line.Split(new[] { ' ' }, 2);
            var command = commandAndArguments[0];
            var arguments = commandAndArguments.Count() > 1 ? commandAndArguments[1] : "";

            var func = _commands.ContainsKey(command) ? _commands[command] : CommandNotFound;
            func(arguments);
        }

        private void Import(string parameters)
        {
            try
            {
                var options = ParseMultipleParameters(parameters, 2);

                var source = options[0];
                var dest = PathFor(options[1]);

                _fileSystemManipulator.ImportFile(source, dest);
                _textWriter.WriteLine("Imported \"{0}\" to \"{1}\"", source, dest);
            }
            catch (ArgumentException)
            {
                _textWriter.WriteLine(@"Please provide two parameters. E.g. import ""C:\host system\path"" /to/dest");
            }
        }

        //NOTE: This method has poor performance.
        private static IList<string> ParseMultipleParameters(string parameters, int parametersCount)
        {
            IList<string> l = new List<string>(parametersCount);

            var currentParameter = "";
            var open = false;

            var chars = parameters.ToList();
            while (chars.Any())
            {
                var c = chars[0];
                chars.RemoveAt(0);

                if (c == '"') open = !open;
                else if (c == ' ' && !open)
                {
                    l.Add(currentParameter);
                    currentParameter = "";
                }
                else currentParameter += c;
            }
            l.Add(currentParameter);

            if (l.Count != parametersCount || open)
            {
                throw new ArgumentException(string.Format("Parameters must be {0}", parametersCount), "parameters");
            }
            return l;
        }

        private void Delete(string parameter)
        {
            var path = PathFor(parameter);
            _fileSystemManipulator.Delete(path);
            _textWriter.WriteLine("Deleted {0}", path);
        }

        private void Exists(string parameter)
        {
            var exists = _fileSystemManipulator.Exists(PathFor(parameter));
            _textWriter.WriteLine(exists ? "Yes" : "No");
        }

        private void Exit(string obj)
        {
            _running = false;
        }

        private void ListDirectory(string parameter)
        {
            var path = PathFor(parameter);
            if (!_fileSystemManipulator.Exists(path))
            {
                _textWriter.WriteLine("File or directory does not exist");
                return;
            }

            var folders = _fileSystemManipulator.Folders(path).ToList();
            _textWriter.WriteLine("Found {0} directories:", folders.Count);

            foreach (var folder in folders)
            {
                _textWriter.WriteLine(folder);
            }
        }

        private void Mkdir(string parameter)
        {
            _fileSystemManipulator.CreateFolder(PathFor(parameter));
            _textWriter.WriteLine("Directory {0} created", PathFor(parameter));
        }

        private string PathFor(string parameter)
        {
            var ret = parameter.StartsWith("/") ? parameter : _currentDirectory + "/" + parameter;
            return ret.EndsWith("/") ? ret.Substring(0, ret.Length - 1) : ret;
        }

        private void CommandNotFound(string parameter)
        {
            _textWriter.WriteLine("Sorry, your command was not found.");
            ShowHelp("");
        }

        private void ShowHelp(string parameters)
        {
            _textWriter.WriteLine("Available commands:");
            foreach (var command in _commands) _textWriter.WriteLine(command.Key);
        }

        public string Prompt
        {
            get
            {
                var prefix = _currentDirectory == "" ? "/" : _currentDirectory;
                return string.Format("{0}> ", prefix);
            }
        }

        public void Cd(string parameter)
        {
            if (!_fileSystemManipulator.IsDirectory(parameter))
            {
                _textWriter.WriteLine("Directory {0} does not exist", parameter);
                return;
            }

            _currentDirectory = parameter.StartsWith("/") ? parameter : (_currentDirectory + "/" + parameter);

            if (_currentDirectory == "/") _currentDirectory = "";

            if (_currentDirectory.EndsWith("/"))
                _currentDirectory = _currentDirectory.Substring(0, _currentDirectory.Length - 1);

            _textWriter.WriteLine("Directory changed");
        }
    }
}
