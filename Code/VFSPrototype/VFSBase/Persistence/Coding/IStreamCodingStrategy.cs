using System.IO;

namespace VFSBase.Persistence.Coding
{
    public interface IStreamCodingStrategy
    {
        Stream DecorateToVFS(Stream stream);
        Stream DecorateToHost(Stream stream);
    }
}