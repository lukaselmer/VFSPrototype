using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBase.Synchronization
{
    class SynchronizationService
    {
        private readonly IFileSystem _fileSystem;

        public SynchronizationService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
    }
}
