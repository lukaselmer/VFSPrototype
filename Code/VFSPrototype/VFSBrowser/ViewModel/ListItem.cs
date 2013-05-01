using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFSBrowser.ViewModel
{
    internal class ListItem : AbstractViewModel
    {
        private string _name;
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }
        private bool _isDirectory;
        public bool IsDirectory { get { return _isDirectory; } set { _isDirectory = value; OnPropertyChanged("IsDirectory"); } }

        public string Path { get; set; }

        public ListItem(string path, string name, bool isDirectory)
        {
            IsDirectory = isDirectory;
            Path = path;
            Name = name;
        }
    }
}
