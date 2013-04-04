using System;

namespace VFSBase.Exceptions
{
    [Serializable]
    public class VFSException : Exception
    {
        public VFSException(string message) : base(message) { }

        protected VFSException() { }
    }
}