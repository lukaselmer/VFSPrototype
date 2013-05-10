using System;
using System.Linq;

namespace VFSBrowser.ViewModel
{
    internal class DirectoryPath
    {
        public const string Seperator = "/";

        private string _path = "";

        public DirectoryPath()
        {
        }


        public DirectoryPath(string path, string name)
        {
            _path = path;
            if (!_path.EndsWith(Seperator)) _path += Seperator;
            _path += name;
            _path = _path.TrimEnd(Seperator.ToCharArray().First());
        }

        public bool IsRoot
        {
            get { return string.IsNullOrEmpty(_path); }
        }

        public void SwitchToParent()
        {
            if (string.IsNullOrEmpty(_path)) _path = "";
            _path = _path.Substring(0, _path.LastIndexOf("/", StringComparison.CurrentCulture));
        }

        public string DisplayPath
        {
            get { return IsRoot ? "/" : _path + Seperator; }
        }

        public DirectoryPath GetChild(string name)
        {
            return new DirectoryPath(DisplayPath, name);
        }
    }
}