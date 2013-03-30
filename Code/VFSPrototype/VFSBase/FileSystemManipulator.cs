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
            var folders = new Queue<string>(path.Split('/'));
            return _fileSystem.Root.GetFolder(folders);
        } 

        public void CreateFolder(string path)
        {
            var folders = new Queue<string>(path.Split('/'));
            _fileSystem.Root.CreateFolder(folders);
        }

        public void Delete(string path)
        {
            throw new System.NotImplementedException();
        }

        public void Move (string source, string dest)
        {
            //var sourceFolder = Folder(source);
        }

        public bool Exists(string path)
        {
            throw new System.NotImplementedException();
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
