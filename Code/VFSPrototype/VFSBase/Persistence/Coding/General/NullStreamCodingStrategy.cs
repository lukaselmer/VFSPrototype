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
        /// <summary>
        /// Decorates the steam, so it can be written to the virtual file system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public Stream DecorateToVFS(Stream stream)
        {
            return stream;
        }

        /// <summary>
        /// Decorates the steam, so it can be written to the host system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public Stream DecorateToHost(Stream stream)
        {
            return stream;
        }
    }
}