using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private ISet<Folder> _folders
        {
            get { return _fileSystem.Root.Folders; }
        }

        public IEnumerable<string> Folders (string path)
        {
            var folders = new Queue<string>(path.Split('/'));
            return _fileSystem.Root.GetFolder(folders).Folders.Select(folder => folder.Name);
        }

        public bool IsDirectory(string path)
        {
            throw new System.NotImplementedException();
        }

        public void CreateFolder(string path)
        {
            var folders = new Queue<string>(path.Split('/'));
            _fileSystem.Root.CreateFolder(folders);
        }

        public void Delete(string path)
        {
            var folders = new Queue<string>(path.Split('/'));
            _fileSystem.Root.Delete(folders);
        }

        public void Move (string source, string dest)
        {
            //var sourceFolder = Folder(source);
        }

        
        public bool Exists(string path)
        {
            var folders = new Queue<string>(path.Split('/'));
            return _fileSystem.Root.Exists(folders);
        }


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

        public void Copy(string source, string dest)
        {
            throw new System.NotImplementedException();
        }

        public void MoveFile(string source, string dest)
        {
            throw new System.NotImplementedException();
        }
    }
}
