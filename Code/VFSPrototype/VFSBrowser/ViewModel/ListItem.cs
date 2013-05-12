using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VFSBrowser.Annotations;

namespace VFSBrowser.ViewModel
{
    [UsedImplicitly]
    internal class ListItem : AbstractViewModel
    {
        private readonly string _name;
        public string Name { get { return _name; } }
        private readonly bool _isDirectory;
        public bool IsDirectory { get { return _isDirectory; }  }

        public string Path { get; set; }

        // Used by the view
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), UsedImplicitly]
        public ImageSource Icon
        {
            get { return IsDirectory ? Folder : File; }
        }

        private static readonly BitmapImage Folder;
        private static readonly BitmapImage File;
        static ListItem()
        {
            Folder = new BitmapImage();
            Folder.BeginInit();
            Folder.UriSource = new Uri("../Resources/Folder.png", UriKind.Relative);
            Folder.EndInit();
            //Folder.Freeze();

            File = new BitmapImage();
            File.BeginInit();
            File.UriSource = new Uri("../Resources/File.png", UriKind.Relative);
            File.EndInit();
            //File.Freeze();
        }

        public ListItem(string path, string name, bool isDirectory)
        {
            _isDirectory = isDirectory;
            Path = path;
            _name = name;
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != this.GetType()) return false;
            return Equals((ListItem)other);
        }

        protected bool Equals(ListItem other)
        {
            return string.Equals(_name, other._name) && string.Equals(Path, other.Path);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name != null ? _name.GetHashCode() : 0) * 397) ^ (Path != null ? Path.GetHashCode() : 0);
            }
        }
    }
}
