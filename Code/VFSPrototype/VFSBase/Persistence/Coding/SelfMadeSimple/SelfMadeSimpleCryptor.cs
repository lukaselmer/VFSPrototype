using System;
using System.Security.Cryptography;
using VFSBase.Persistence.Coding.SelfMadeAes;

namespace VFSBase.Persistence.Coding.SelfMadeSimple
{
    internal class SelfMadeSimpleCryptor : ICryptoTransform
    {
        private readonly byte[] _key;
        private readonly byte[] _initializationVector;
        private readonly CryptoDirection _cryptoDirection;

        public SelfMadeSimpleCryptor(byte[] key, byte[] initializationVector, CryptoDirection cryptoDirection)
        {
            _key = key;
            _initializationVector = initializationVector;
            _cryptoDirection = cryptoDirection;
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer == null) throw new ArgumentNullException("inputBuffer");
            if (outputBuffer == null) throw new ArgumentNullException("outputBuffer");

            Array.Copy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
            if (_cryptoDirection == CryptoDirection.Encrypt)
            {
                for (var i = 0; i < outputBuffer.Length; i++)
                {
                    outputBuffer[i] = (byte)(outputBuffer[i] ^ _key[i % _key.Length] ^ _initializationVector[i % _initializationVector.Length] ^ i);
                }
            }
            else
            {
                for (var i = 0; i < outputBuffer.Length; i++)
                {
                    outputBuffer[i] = (byte)(outputBuffer[i] ^ _key[i % _key.Length] ^ _initializationVector[i % _initializationVector.Length] ^ i);
                }
            }

            return inputCount;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var outputBuffer = new byte[inputCount];
            Array.Copy(inputBuffer, inputOffset, outputBuffer, 0, inputCount);
            TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
            return outputBuffer;
        }

        public int InputBlockSize { get { return 1024; } }
        public int OutputBlockSize { get { return 1024; } }
        public bool CanTransformMultipleBlocks { get { return true; } }
        public bool CanReuseTransform { get { return true; } }

        public void Dispose() { }
    }
}