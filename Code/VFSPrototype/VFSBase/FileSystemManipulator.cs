using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VFSBase
{
    public class FileSystemManipulator
    {
        private readonly FileSystem _fileSystem;

        public FileSystemManipulator(FileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public ISet<Folder> Folders
        {
            get { return _fileSystem.Root.Folders; }
        }

        public void CreateFolder(string path)
        {
            var folders = new Queue<string>(path.Split('/'));
            _fileSystem.Root.CreateFolder(folders);
        }

        public bool DoesFolderExist(string path)
        {
            var folders = new Queue<string>(path.Split('/'));
            return _fileSystem.Root.DoesFolderExist(folders);
        }

        public void DeleteFolder(string path)
        {
            var folders = new Queue<string>(path.Split('/'));
            _fileSystem.Root.DeleteFolder(folders);
        }
    }
}
