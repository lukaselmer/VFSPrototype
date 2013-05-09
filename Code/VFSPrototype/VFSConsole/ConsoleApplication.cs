﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFSBase.Callbacks;
using VFSBase.Exceptions;
using VFSBase.Interfaces;

namespace VFSConsole
{
    internal class ConsoleApplication : IDisposable
    {
        private readonly TextReader _textReader;
        private readonly TextWriter _textWriter;
        private volatile bool _running = true;
        private readonly IDictionary<string, Action<string>> _commands;
        private IFileSystemTextManipulator _fileSystemTextManipulator;
        private string _currentDirectory = "";

        private readonly Action<int> _currentlyProcessedChanged;
        private readonly Func<bool> _shouldAbort;
        private readonly Action<bool> _operationCompleted;
        private readonly Action<int> _totalToProcessChanged;

        internal ConsoleApplication(IConsoleApplicationSettings consoleApplicationSettings, IFileSystemTextManipulator fileSystemTextManipulator)
        {
            if (fileSystemTextManipulator == null) throw new ArgumentNullException("fileSystemTextManipulator", "fileSystem must not be null.");

            _fileSystemTextManipulator = fileSystemTextManipulator;

            _textReader = consoleApplicationSettings.Reader;
            _textWriter = consoleApplicationSettings.Writer;

            _commands = new Dictionary<string, Action<string>>
                {
                    {"cd", Cd},
                    {"delete", Delete},
                    {"exists", Exists},
                    {"exit", Exit},
                    {"export", Export},
                    {"help", ShowHelp},
                    {"import", Import},
                    {"ls", List},
                    {"mkdir", Mkdir},
                };


            _shouldAbort = () => false;
            _operationCompleted = success => _textWriter.WriteLine("operation completed");
            _totalToProcessChanged = i => _textWriter.WriteLine("total to process: {0}", i);
            _currentlyProcessedChanged = i => _textWriter.WriteLine("total to process: {0}", i);
        }

        internal void Run()
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ProcessLine(string line)
        {
            var commandAndArguments = line.Split(new[] { ' ' }, 2);
            var command = commandAndArguments[0];
            var arguments = commandAndArguments.Count() > 1 ? commandAndArguments[1] : "";

            var func = _commands.ContainsKey(command) ? _commands[command] : CommandNotFound;
            try
            {
                func(arguments);
            }
            catch (VFSException exception)
            {
                _textWriter.WriteLine("An error occurred: {0}", exception.Message);
            }
            catch (Exception exception)
            // This is a violation of the rule Microsoft.Design.CA1031:DoNotCatchGeneralExceptionTypes, but in this
            // case, it is ok to violate this rule.
            {
                _textWriter.WriteLine("An unknown error occurred: {0}", exception.Message);
                _textWriter.WriteLine("The system might be unstable, consider restarting");
            }
        }

        private void Import(string parameters)
        {
            try
            {
                var options = ParseMultipleParameters(parameters, 2);

                var source = options[0];
                var dest = PathFor(options[1]);

                _fileSystemTextManipulator.Import(source, dest, new ImportCallbacks(_shouldAbort, _operationCompleted, _totalToProcessChanged, _currentlyProcessedChanged));
                _textWriter.WriteLine("Imported \"{0}\" to \"{1}\"", source, dest);
            }
            catch (ArgumentException)
            {
                _textWriter.WriteLine(@"Please provide two parameters. E.g. import ""C:\host system\path"" /to/dest");
            }
        }

        private void Export(string parameters)
        {
            try
            {
                var options = ParseMultipleParameters(parameters, 2);

                var source = PathFor(options[0]);
                var dest = options[1];

                _fileSystemTextManipulator.Export(source, dest, new ExportCallbacks(_shouldAbort, _operationCompleted, _totalToProcessChanged, _currentlyProcessedChanged));
                _textWriter.WriteLine("Exported \"{0}\" to \"{1}\"", source, dest);
            }
            catch (ArgumentException)
            {
                _textWriter.WriteLine(@"Please provide two parameters. E.g. export /from/src ""C:\host system\path""");
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
            _fileSystemTextManipulator.Delete(path);
            _textWriter.WriteLine("Deleted {0}", path);
        }

        private void Exists(string parameter)
        {
            var exists = _fileSystemTextManipulator.Exists(PathFor(parameter));
            _textWriter.WriteLine(exists ? "Yes" : "No");
        }

        private void Exit(string obj)
        {
            _running = false;
        }

        private void List(string parameter)
        {
            var path = PathFor(parameter);
            if (!_fileSystemTextManipulator.Exists(path))
            {
                _textWriter.WriteLine("File or directory does not exist");
                return;
            }

            var folders = _fileSystemTextManipulator.Folders(path).ToList();
            _textWriter.WriteLine("Found {0} directories:", folders.Count);

            foreach (var folder in folders)
            {
                _textWriter.WriteLine(folder);
            }

            var files = _fileSystemTextManipulator.Files(path).ToList();
            _textWriter.WriteLine("Found {0} files:", files.Count);

            foreach (var file in files)
            {
                _textWriter.WriteLine(file);
            }
        }

        private void Mkdir(string parameter)
        {
            _fileSystemTextManipulator.CreateFolder(PathFor(parameter));
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

        internal string Prompt
        {
            get
            {
                var prefix = String.IsNullOrEmpty(_currentDirectory) ? "/" : _currentDirectory;
                return string.Format("{0}> ", prefix);
            }
        }

        private void Cd(string parameter)
        {
            if (!_fileSystemTextManipulator.IsDirectory(parameter))
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            // free managed resources

            if (_fileSystemTextManipulator != null)
            {
                _fileSystemTextManipulator.Dispose();
                _fileSystemTextManipulator = null;
            }
        }

    }
}
