using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFSBase
{
    public class VFSFile : IComparable
    {
        public VFSFile(string name)
        {
            Name = name;
        }

        protected string Name { get; private set; }

        public int CompareTo(object obj)
        {
            var file = obj as VFSFile;
            if (file == null) return -1;
            return String.Compare(Name, file.Name, StringComparison.Ordinal);
        }
    }
}
