using System.IO;

namespace VFSBase.Persistence.Coding.General
{
    public interface IStreamCodingStrategy
    {
        Stream DecorateToVFS(Stream stream);
        Stream DecorateToHost(Stream stream);
    }
}