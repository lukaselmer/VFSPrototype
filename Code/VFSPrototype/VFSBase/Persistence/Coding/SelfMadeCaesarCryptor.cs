using System;
using System.Security.Cryptography;

namespace VFSBase.Persistence.Coding
{
    internal class SelfMadeCaesarCryptor : ICryptoTransform
    {
        private readonly int _key;
        private readonly CryptoDirection _cryptoDirection;

        public SelfMadeCaesarCryptor(int key, CryptoDirection cryptoDirection)
        {
            _key = key;
            _cryptoDirection = cryptoDirection;
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            Array.Copy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
            if (_cryptoDirection == CryptoDirection.Encrypt)
            {
                for (var i = 0; i < outputBuffer.Length; i++)
                {
                    outputBuffer[i] = (byte)((outputBuffer[i] + _key));
                }
            }
            else
            {
                for (var i = 0; i < outputBuffer.Length; i++)
                {
                    outputBuffer[i] = (byte)((outputBuffer[i] - _key));
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