using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFSBase.Implementation;

namespace VFSBaseTests.Helpers
{
    internal static class TestHelper
    {
        public static FileSystemOptions CreateFileSystemOptions(string location, long diskSize)
        {
            return new FileSystemOptions(location, diskSize, StreamEncryptionType.None, StreamCompressionType.None);
        }
    }
}
