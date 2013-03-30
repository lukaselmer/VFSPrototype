using System.Collections.Generic;
using System.IO;
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


        public Folder Folder (string path)
        {
            // TODO: get Folder
            return null;
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

        /*private void SplitPath (string path, out string folder, out string file)
        {
            var filePos = path.LastIndexOf('/');
            file = (filePos >= 0) ? path.Substring(filePos) : path;
            folder = (filePos >= 0) ? path.Substring(0, filePos) : "";   
        }*/

        public void ImportFile(string source, string dest)
        {
            var path = new Queue<string>(dest.Split('/'));   
            _fileSystem.Root.ImportFile(path, source);
        }

        public void ExportFile(string source, string dest)
        {
            var path = new Queue<string>(source.Split('/'));
            _fileSystem.Root.ExportFile(path, dest);
        }

        public void DeleteFile(string file)
        {
            var path = new Queue<string>(file.Split('/'));
            _fileSystem.Root.DeleteFile(path);
        }

        public bool DoesFileExists(string file)
        {
            var path = new Queue<string>(file.Split('/'));
            return _fileSystem.Root.DoesFileExist(path);
        }
    }
}
