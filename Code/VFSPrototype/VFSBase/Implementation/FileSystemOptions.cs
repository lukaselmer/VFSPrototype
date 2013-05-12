using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using VFSBase.DiskServiceReference;
using VFSBase.Exceptions;
using VFSBase.Interfaces;

using VFSBase.Persistence;
using VFSBase.Persistence.Blocks;
using VFSBase.Persistence.Coding;
using VFSBase.Persistence.Coding.General;
using VFSBase.Persistence.Coding.Strategies;
using VFSBlockAbstraction;

namespace VFSBase.Implementation
{
    [Serializable]
    public class FileSystemOptions : IFileSystemOptions
    {
        private int _blockSize;

        [NonSerialized]
        private IStreamCodingStrategy _streamCodingStrategy;


        public FileSystemOptions(string location, StreamEncryptionType encryption, StreamCompressionType compression)
        {
            Location = location;
            Encryption = encryption;
            Compression = compression;
            BlockSize = (int)MathUtil.KB(8);
            MasterBlockSize = (int)MathUtil.KB(32);
            NameLength = 255;
            BlockReferenceSize = 64;
            BlockAllocation = new BlockAllocation();
            IndirectionCountForIndirectNodes = 2;
            RootBlockNr = 0;
        }

        internal long RootBlockNr { get; set; }

        internal void InitializePassword(string password)
        {
            if (Encryption == StreamEncryptionType.None) return;

            using (var r = Rijndael.Create())
            {
                _encryptedEncryptionKey = TransformEncryptionKey(r.Key, password);
                EncryptionInitializationVector = r.IV;
            }

            InitializeStreamCodingStrategy(password);
        }

        public void ApplyEncryptionSettings(FileSystemOptions oldOptions)
        {
            _encryptedEncryptionKey = oldOptions._encryptedEncryptionKey;
            EncryptionKey = oldOptions.EncryptionKey;
            _streamCodingStrategy = oldOptions._streamCodingStrategy;
        }

        private static byte[] TransformEncryptionKey(byte[] key, string password)
        {
            var bb = new byte[key.Length];
            for (var i = 0; i < key.Length; i++)
            {
                bb[i] = (byte)(key[i] ^ password[i % password.Length]);
            }
            return bb;
        }

        internal void InitializeStreamCodingStrategy(string password)
        {
            if (Encryption != StreamEncryptionType.None) EncryptionKey = TransformEncryptionKey(_encryptedEncryptionKey, password);
            _streamCodingStrategy = new StramStrategyResolver(this).ResolveStrategy();
        }

        public string Location { get; set; }

        public StreamEncryptionType Encryption { get; set; }
        public StreamCompressionType Compression { get; set; }

        public int MasterBlockSize { get; set; }

        public int BlockReferenceSize { get; private set; }

        //public int StartOfDirectBlock { get { return 1 /*type byte*/+ NameLength; } }
        //public int StartOfIndirectBlocks { get { return BlockSize - (3 * BlockReferenceSize); } }
        //public int DirectBlocksSpace { get { return StartOfIndirectBlocks - StartOfDirectBlock; } }
        //public int DirectBlocksAmount { get { return DirectBlocksSpace / BlockReferenceSize; } }

        public static FileSystemOptions Deserialize(Stream stream, string password)
        {
            IFormatter formatter = new BinaryFormatter();
            var fileSystemOptions = formatter.Deserialize(stream) as FileSystemOptions;
            if (fileSystemOptions == null) throw new VFSException("Invalid file");

            fileSystemOptions.InitializeStreamCodingStrategy(password);
            return fileSystemOptions;
        }

        public int BlockSize
        {
            get
            {
                return _blockSize;
            }
            set
            {
                if (value < (int)MathUtil.KB(2)) throw new VFSException("block size too small");
                _blockSize = value;
            }
        }

        public int IndirectionCountForIndirectNodes { get; private set; }

        public long DiskFree
        {
            get
            {
                var s = Path.GetFullPath(Location);
                var driveInfo = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == Path.GetPathRoot(s));
                return driveInfo == null ? -1 : driveInfo.TotalFreeSpace;
            }
        }
        public long DiskOccupied { get { return _blockSize * BlockAllocation.OccupiedCount; } }

        public int NameLength { get; internal set; }

        public int ReferencesPerIndirectNode { get { return BlockSize / BlockReferenceSize; } }

        public BlockAllocation BlockAllocation { get; private set; }

        public long MaximumFileSize
        {
            get
            {
                return MathUtil.Power(ReferencesPerIndirectNode, IndirectionCountForIndirectNodes + 1) * BlockSize;
            }
        }

        [NonSerialized]
        private byte[] _encryptionKey;

        private byte[] _encryptedEncryptionKey;

        internal byte[] EncryptionStringForTest { get; set; }

        internal byte[] EncryptionKey
        {
            get { return _encryptionKey; }
            set { _encryptionKey = value; }
        }

        internal byte[] EncryptionInitializationVector { get; set; }

        public IStreamCodingStrategy StreamCodingStrategy
        {
            get
            {
                if (_streamCodingStrategy == null) InitializeStreamCodingStrategy("");
                return _streamCodingStrategy;
            }
        }

        public int Id { get; set; }

        public long LocalVersion { get; set; }

        public long LastServerVersion { get; set; }
    }
}