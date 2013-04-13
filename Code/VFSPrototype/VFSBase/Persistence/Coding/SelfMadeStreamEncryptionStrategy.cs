using System;
using System.IO;
using System.Security.Cryptography;

namespace VFSBase.Persistence.Coding
{
    internal class SelfMadeStreamEncryptionStrategy : IStreamCodingStrategy
    {
        private readonly EncryptionOptions _options;

        public SelfMadeStreamEncryptionStrategy(EncryptionOptions options)
        {
            _options = options;
        }

        public Stream DecorateToVFS(Stream stream)
        {
            var encryptor = new SelfMadeCryptor(_options.Key, _options.InitializationVector, SelfMadeCryptor.CryptoDirection.Encrypt);
            return new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
        }

        public Stream DecorateToHost(Stream stream)
        {
            var decryptor = new SelfMadeCryptor(_options.Key, _options.InitializationVector, SelfMadeCryptor.CryptoDirection.Decrypt);
            return new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
        }
    }

    internal class SelfMadeCryptor : ICryptoTransform
    {
        private readonly byte[] _key;
        private readonly byte[] _initializationVector;
        private readonly CryptoDirection _cryptoDirection;

        public SelfMadeCryptor(byte[] key, byte[] initializationVector, CryptoDirection cryptoDirection)
        {
            _key = key;
            _initializationVector = initializationVector;
            _cryptoDirection = cryptoDirection;
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            Array.Copy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
            if (_cryptoDirection == CryptoDirection.Encrypt)
            {
                for (int i = 0; i < outputBuffer.Length; i++)
                {
                    outputBuffer[i] = (byte)(outputBuffer[i] ^ _key[i % _key.Length] ^ _initializationVector[i % _initializationVector.Length] ^ i);
                }
            }
            else
            {
                for (int i = 0; i < outputBuffer.Length; i++)
                {
                    outputBuffer[i] = (byte)(outputBuffer[i] ^ _key[i % _key.Length] ^ _initializationVector[i % _initializationVector.Length] ^ i);
                }
            }
            
            return inputCount;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var b = new byte[inputCount];
            Array.Copy(inputBuffer, inputOffset, b, 0, inputCount);
            if (_cryptoDirection == CryptoDirection.Encrypt)
            {
                for (int i = 0; i < b.Length; i++)
                {
                    b[i] = (byte)(b[i] ^ _key[i % _key.Length] ^ _initializationVector[i % _initializationVector.Length] ^ i);
                }
            }
            else
            {
                for (int i = 0; i < b.Length; i++)
                {
                    b[i] = (byte)(b[i] ^ _key[i % _key.Length] ^ _initializationVector[i % _initializationVector.Length] ^ i);
                }
            }
            return b;
        }

        public int InputBlockSize { get { return 1024; } }
        public int OutputBlockSize { get { return 1024; } }
        public bool CanTransformMultipleBlocks { get { return true; } }
        public bool CanReuseTransform { get { return true; } }

        internal enum CryptoDirection
        {
            Encrypt,
            Decrypt
        }

        public void Dispose() { }
    }
}