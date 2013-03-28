using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFSBase
{
    public class FileSystem
    {
        public FileSystem(string location, long diskSize)
        {
            Location = location;
            DiskSize = diskSize;

            if (File.Exists(Location)) throw new VFSException("File already exists!");

            File.WriteAllText(Location, "");

            Root = new Folder();
        }

        public long DiskSize { get; private set; }

        public string Location { get; private set; }

        public Folder Root { get; private set; }

        public void Destroy()
        {
            File.Delete(Location);
        }
    }
}
