using System.IO;

namespace VFSBase.Persistence.Coding.General
{
    public interface IStreamCodingStrategy
    {
        /// <summary>
        /// Decorates the steam, so it can be written to the virtual file system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        Stream DecorateToVFS(Stream stream);

        /// <summary>
        /// Decorates the steam, so it can be written to the host system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        Stream DecorateToHost(Stream stream);
    }
}