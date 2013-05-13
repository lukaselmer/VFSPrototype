using VFSBase.Callbacks;
using VFSBase.DiskServiceReference;
using VFSBase.Interfaces;
using VFSBase.Synchronization;

namespace VFSBaseTests.Mocks
{
    internal class SynchronizationServiceMock : SynchronizationService
    {
        public SynchronizationServiceMock(IFileSystem fileSystem, UserDto user, SynchronizationCallbacks callbacks, IDiskService diskService) :
            base(fileSystem, user, callbacks, diskService)
        { }
    }
}