using System;
using System.Linq;

namespace VFSBrowser.ViewModel
{
    internal class DirectoryPath
    {
        public const string Seperator = "/";

        private string _path = "/";

        public DirectoryPath()
        {
        }


        public DirectoryPath(string path, string name)
        {
            Path = path;
            if (!Path.EndsWith(Seperator)) Path += Seperator;
            Path += name;
            Path = Path.TrimEnd(Seperator.ToCharArray().First());
        }

        public bool IsRoot
        {
            get { return Path == Seperator; }
        }

        public void SwitchToParent()
        {
            Path = Path.Substring (0, Path.LastIndexOf (Seperator, StringComparison.CurrentCulture));
            if (string.IsNullOrEmpty (Path)) Path = "/";
        }

        public string DisplayPath
        {
            get { return IsRoot ? Seperator : Path + Seperator; }
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public DirectoryPath GetChild(string name)
        {
            return new DirectoryPath(DisplayPath, name);
        }
    }
}