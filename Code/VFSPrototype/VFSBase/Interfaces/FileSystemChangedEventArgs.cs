using System;

namespace VFSBase.Interfaces
{
    internal class FileSystemChangedEventArgs : EventArgs
    {
        public FileSystemChangedEventArgs(string location)
        {
            Location = location;
        }

        public string Location { get; private set; }
    }
}