using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using VFSBase.Implementation;

namespace VFSBrowser.ViewModel
{
    class BrowserFolder
    {
        private readonly FileSystemTextManipulator _manipulator;
        public string Name { get; set; }
        public string Path { get; set; }

        public BrowserFolder(FileSystemTextManipulator manipulator)
        {
            _manipulator = manipulator;
        }

        public IEnumerable<BrowserFolder> Folders
        {
            get
            {
                return _manipulator.Folders(Path).Select(name => new BrowserFolder(_manipulator) { Name = name, Path = Path + name + "/" });
            }
        } 
    }

    class MainViewModel : INotifyPropertyChanged
    {
        private const string FilePath = "../../../Testfiles/gui_test.vhs";
        private const long Size = 1000 * 1000 * 1000 /* 1 MB */;

        private readonly FileSystemTextManipulator _manipulator;
        private readonly BrowserFolder _root;


        public IEnumerable<BrowserFolder> RootFolders
        {
            get { return _root.Folders; }
        } 


        public MainViewModel()
        {
            if (File.Exists(FilePath)) File.Delete(FilePath);
            var fileSystemData = new FileSystemOptions(FilePath, Size);
            
            _manipulator = new FileSystemTextManipulator(fileSystemData);
            _manipulator.CreateFolder("/home/ivo/pictures"); 
            _manipulator.CreateFolder("/home/ivo/movies"); 
            _manipulator.CreateFolder("/home/ivo/docs");
            _manipulator.CreateFolder("/home/lukas/bilder");
            _manipulator.CreateFolder("/home/lukas/filme");
            _manipulator.CreateFolder("/share/eth/dna");
            _manipulator.CreateFolder("/share/eth/jcd");
            _manipulator.CreateFolder("/share/eth/db");
            _manipulator.CreateFolder("/share/fh");
            _manipulator.CreateFolder("/volumes/cd");
            _manipulator.CreateFolder("/volumes/floppy");
            _manipulator.CreateFolder("/volumes/usb");

            _root = new BrowserFolder(_manipulator) {Name = "/", Path = "/"};
        }




        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
