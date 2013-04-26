using System.IO;

namespace VFSBase.Persistence.Coding.General
{
    /// <summary>
    /// Class NullStreamCodingStrategy
    /// 
    /// Null Object pattern
    /// </summary>
    public class NullStreamCodingStrategy : IStreamCodingStrategy
    {
        public Stream DecorateToVFS(Stream stream)
        {
            return stream;
        }

        public Stream DecorateToHost(Stream stream)
        {
            return stream;
        }
    }
}