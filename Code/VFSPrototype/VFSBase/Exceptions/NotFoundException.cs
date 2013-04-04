using System;
using VFSBase.Exceptions;

namespace VFSBase.Implementation
{
    [Serializable]
    public class NotFoundException : VFSException
    {
        public NotFoundException()
            : base("")
        {
        }
    }
}