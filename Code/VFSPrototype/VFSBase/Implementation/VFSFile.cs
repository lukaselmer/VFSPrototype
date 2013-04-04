using System.IO;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    internal class VFSFile : IIndexNode
    {
        /*public VFSFile(string name, string source)
            : this(name, File.ReadAllBytes(source))
        {
        }

        public VFSFile(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }*/

        public VFSFile(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public Folder Parent { get; set; }

        public long BlockNumber { get; set; }
        public long IndirectNodeNumber { get; set; }
        public long BlocksCount { get; set; }

        public byte[] Data { get; private set; }
    }
}
