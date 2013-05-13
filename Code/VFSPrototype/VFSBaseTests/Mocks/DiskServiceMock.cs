using System.Threading.Tasks;
using VFSBase.DiskServiceReference;

namespace VFSBaseTests.Mocks
{
    internal class DiskServiceMock : IDiskService
    {
        public DiskDto DiskFake { get; set; }

        public DiskOptionsDto DiskOptionsMock { get; set; }

        public DiskOptionsDto DiskOptionsResult { get; set; }


        public UserDto Register(string login, string hashedPassword)
        {
            throw new System.NotImplementedException();
        }

        public Task<UserDto> RegisterAsync(string login, string hashedPassword)
        {
            throw new System.NotImplementedException();
        }

        public UserDto Login(string loginName, string hashedPassword)
        {
            throw new System.NotImplementedException();
        }

        public Task<UserDto> LoginAsync(string loginName, string hashedPassword)
        {
            throw new System.NotImplementedException();
        }

        public DiskDto[] Disks(UserDto userDto)
        {
            return new[] { DiskFake };
        }

        public Task<DiskDto[]> DisksAsync(UserDto userDto)
        {
            throw new System.NotImplementedException();
        }

        public DiskDto CreateDisk(UserDto userDto, DiskOptionsDto optionsDto)
        {
            return DiskFake;
        }


        public Task<DiskDto> CreateDiskAsync(UserDto userDto, DiskOptionsDto optionsDto)
        {
            throw new System.NotImplementedException();
        }

        public bool DeleteDisk(UserDto userDto, DiskDto diskDto)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> DeleteDiskAsync(UserDto userDto, DiskDto diskDto)
        {
            throw new System.NotImplementedException();
        }

        public SynchronizationState FetchSynchronizationState(UserDto userDto, DiskDto diskDto)
        {
            return SynchronizationState;
        }

        public Task<SynchronizationState> FetchSynchronizationStateAsync(UserDto userDto, DiskDto diskDto)
        {
            throw new System.NotImplementedException();
        }

        public DiskOptionsDto GetDiskOptions(UserDto userDto, DiskDto diskDto)
        {
            return DiskOptionsMock;
        }

        public Task<DiskOptionsDto> GetDiskOptionsAsync(UserDto userDto, DiskDto diskDto)
        {
            throw new System.NotImplementedException();
        }

        public void SetDiskOptions(UserDto userDto, DiskDto diskDto, DiskOptionsDto optionsDto)
        {
            DiskOptionsResult = optionsDto;
        }


        public Task SetDiskOptionsAsync(UserDto userDto, DiskDto diskDto, DiskOptionsDto optionsDto)
        {
            throw new System.NotImplementedException();
        }

        public void WriteBlock(UserDto userDto, int diskId, long blockNr, byte[] content)
        {
        }

        public Task WriteBlockAsync(UserDto userDto, int diskId, long blockNr, byte[] content)
        {
            throw new System.NotImplementedException();
        }

        public byte[] ReadBlock(UserDto userDto, int diskId, long blockNr)
        {
            throw new System.NotImplementedException();
        }

        public Task<byte[]> ReadBlockAsync(UserDto userDto, int diskId, long blockNr)
        {
            return Task.Run(() => new byte[700]);
        }

        public void UpdateDisk(UserDto userDto, DiskDto diskDto)
        {
        }

        public Task UpdateDiskAsync(UserDto userDto, DiskDto diskDto)
        {
            throw new System.NotImplementedException();
        }

        public SynchronizationState SynchronizationState { get; set; }
    }
}