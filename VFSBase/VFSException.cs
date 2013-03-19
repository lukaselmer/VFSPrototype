using System;

namespace VFSBase
{
    public class VFSException : Exception
    {
        public VFSException(string message)
            : base(message)
        {

        }
    }
}