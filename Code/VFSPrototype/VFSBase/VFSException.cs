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

    public class NotFoundException : VFSException
    {
        public NotFoundException() : base("")
        {
        }
    }
}