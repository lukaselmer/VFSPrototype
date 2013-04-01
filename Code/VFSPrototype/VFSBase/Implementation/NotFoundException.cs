using System;

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