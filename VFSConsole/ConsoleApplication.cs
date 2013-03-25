using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VFSConsole
{
    public class ConsoleApplication
    {
        private readonly TextReader _textReader;
        private readonly TextWriter _textWriter;
        private volatile bool _running = true;
        private readonly IDictionary<string, Action<string>> _commands;

        public ConsoleApplication(TextReader textReader, TextWriter textWriter)
        {
            _textReader = textReader;
            _textWriter = textWriter;

            _commands = new Dictionary<string, Action<string>>
                            {
                                {"help", ShowHelp},
                                {"ls", ListDirectory},
                                {"exit", Exit},
                            };
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
            _textWriter.WriteLine("TODO: implement this!");
            _textWriter.WriteLine("and this");
            _textWriter.WriteLine("and so on...");
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