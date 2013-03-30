using System;

namespace VFSBase.Implementation
{
    [SerializableAttribute] 
    public class VFSException : Exception
    {
        public VFSException(string message)
            : base(message)
        {

        }
    }
}