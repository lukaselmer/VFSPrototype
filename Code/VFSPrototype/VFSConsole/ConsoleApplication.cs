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
        private string _currentDirectory = "/";

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

        private void Import(string parameters)
        {
            try
            {
                var options = ParseParams(parameters, 2);
                _fileSystemManipulator.ImportFile(options[0], options[1]);
                _textWriter.WriteLine("Imported \"{0}\" to \"{1}\"", options[0], options[1]);
            }
            catch (ArgumentException)
            {
                _textWriter.WriteLine(@"Please provide two parameters. E.g. import ""C:\host system\path"" /to/dest");
            }
        }

        private static IList<string> ParseParams(string parameters, int parametersCount)
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
            _fileSystemManipulator.Delete(parameter);
            _textWriter.WriteLine("Deleted {0}", parameter);
        }

        private void Exists(string parameters)
        {
            var exists = _fileSystemManipulator.Exists(parameters);
            _textWriter.WriteLine(exists ? "Yes" : "No");
        }

        private void Exit(string obj)
        {
            _running = false;
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

        private void ListDirectory(string parameter)
        {
            if (!_fileSystemManipulator.Exists(parameter))
            {
                _textWriter.WriteLine("File or directory does not exist");
                return;
            }

            var folders = _fileSystemManipulator.Folders(parameter).ToList();
            _textWriter.WriteLine("Found {0} directories:", folders.Count);

            foreach (var folder in folders)
            {
                _textWriter.WriteLine(folder);
            }
        }

        private void Mkdir(string parameter)
        {
            _fileSystemManipulator.CreateFolder(parameter);
            _textWriter.WriteLine("Directory {0} created", parameter);
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

        public string Prompt { get { return string.Format("{0}> ", _currentDirectory); } }

        public void Cd(string parameter)
        {
            _currentDirectory = parameter;
            _textWriter.WriteLine("Directory changed");
        }
    }
}