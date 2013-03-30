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

        public ConsoleApplication(TextReader textReader, TextWriter textWriter, IFileSystemManipulator fileSystemManipulator)
        {
            if (fileSystemManipulator == null) throw new ArgumentNullException("fileSystemManipulator", "fileSystem must not be null.");

            _fileSystemManipulator = fileSystemManipulator;

            _textReader = textReader;
            _textWriter = textWriter;

            _commands = new Dictionary<string, Action<string>>
                            {
                                {"exists", Exists},
                                {"exit", Exit},
                                {"help", ShowHelp},
                                {"ls", ListDirectory},
                                {"mkdir", Mkdir},
                            };
        }

        private void Exists(string parameters)
        {
            var exists = _fileSystemManipulator.DoesFolderExist(parameters);
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
                _textWriter.Write("> ");
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
            if (!_fileSystemManipulator.DoesFolderExist(parameter))
            {
                _textWriter.WriteLine("File or directory does not exist");
                return;
            }

            var folders = _fileSystemManipulator.Folder(parameter).Folders;
            _textWriter.WriteLine("Found {0} directories:", folders.Count);

            foreach (var folder in folders)
            {
                _textWriter.WriteLine(folder.Name);
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
    }
}