using System.IO;

namespace VFSConsole
{
    public interface IConsoleApplicationSettings
    {
        TextReader Reader { get; }
        TextWriter Writer { get; }
    }
}
