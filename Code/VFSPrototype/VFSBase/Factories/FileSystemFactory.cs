using System;
using System.IO;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBase.Factories
{
    internal static class FileSystemFactory
    {
        internal static FileSystem Create(FileSystemOptions options, string password)
        {
            if (File.Exists(options.Location)) throw new VFSException("File already exists");

            using (var file = File.Open(options.Location, FileMode.CreateNew, FileAccess.Write))
            {
                options.InitializePassword(password);
                options.Serialize(file);
            }

            return new FileSystem(options);
        }

        internal static FileSystem Import(string location, string password)
        {
            if (!File.Exists(location)) throw new VFSException("File does not exist");

            FileSystemOptions newOptions;

            using (var file = File.Open(location, FileMode.Open, FileAccess.ReadWrite))
            {
                try
                {
                    newOptions = FileSystemOptions.Deserialize(file, password);
                }
                catch (ArgumentException exception)
                {
                    throw new VFSException("Invalid virtual file", exception);
                }
                newOptions.Location = location;

                file.Seek(0, SeekOrigin.Begin);
                newOptions.Serialize(file);
            }

            return new FileSystem(newOptions);
        }

        internal static void Delete(IFileSystem fileSystem)
        {
            fileSystem.Dispose();
            File.Delete(fileSystem.FileSystemOptions.Location);
        }
    }
}
