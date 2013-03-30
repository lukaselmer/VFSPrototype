using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VFSBase
{
    public class FileSystemManipulator : IFileSystemManipulator
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


        public void ImportFile(string source, string dest)
        {
            var fileName = dest.Substring(dest.LastIndexOf('/'));
            var path = dest.Substring(0, dest.LastIndexOf('/'));

            var folders = new Queue<string>(path.Split('/'));   
            if (!_fileSystem.Root.DoesFolderExist(folders)) 
                _fileSystem.Root.CreateFolder(folders);

            //_fileSystem.Root.ImportFile(source, folders, fileName);
        }

        public bool FileExists(string path)
        {
            return false;
        }
    }
}
