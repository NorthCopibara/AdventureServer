using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpServer
{
    public class PacketBuffer
    {
        private List<byte> _bufferList;
        private byte[] _readBuffer;
        private int _readPosition;
        private bool _buffUpdate;

        private bool _disposedValue;

        public int ReadPosition => _readPosition;
        public byte[] ToArray => _bufferList.ToArray();
        public int Count => _bufferList.Count;
        public int Length => Count - _readPosition;

        public PacketBuffer()
        {
            _bufferList = new List<byte>();
            _readPosition = 0;
        }

        public void Clear()
        {
            _bufferList.Clear();
            _readPosition = 0;
        }

        #region Write data

        public void WriteBytes(byte[] input)
        {
            _bufferList.AddRange(input);
            _buffUpdate = true;
        }

        public void WriteByte(byte input)
        {
            _bufferList.Add(input);
            _buffUpdate = true;
        }

        public void WriteInteger(int input)
        {
            _bufferList.AddRange(BitConverter.GetBytes(input));
            _buffUpdate = true;
        }

        public void WriteFloat(float input)
        {
            _bufferList.AddRange(BitConverter.GetBytes(input));
            _buffUpdate = true;
        }

        public void WriteString(string input)
        {
            _bufferList.AddRange(BitConverter.GetBytes(input.Length));
            _bufferList.AddRange(Encoding.ASCII.GetBytes(input));
            _buffUpdate = true;
        }

        public void WriteBool(bool value)
        {
            _bufferList.AddRange(BitConverter.GetBytes(value));
            _buffUpdate = true;
        }

        #endregion

        #region Read data

        private void Read()
        {
            if (!_buffUpdate) return;
            _readBuffer = _bufferList.ToArray();
            _buffUpdate = false;
        }

        private void CheckLimitBuffer()
        {
            if (_bufferList.Count <= _readPosition)
            {
                throw new Exception("Buffer is past its limit!");
            }
        }

        private void MoveReadPosition(bool peek, int countSteps)
        {
            if (peek & _bufferList.Count > _readPosition)
            {
                _readPosition += countSteps;
            }
        }

        public int ReadInteger(bool peek = true)
        {
            CheckLimitBuffer();
            Read();
            var value = BitConverter.ToInt32(_readBuffer, _readPosition);
            MoveReadPosition(peek, 4);
            return value;
        }

        public float ReadFloat(bool peek = true)
        {
            CheckLimitBuffer();
            Read();
            var value = BitConverter.ToSingle(_readBuffer, _readPosition);
            MoveReadPosition(peek, 4);
            return value;
        }

        public byte ReadByte(bool peek = true)
        {
            CheckLimitBuffer();
            Read();
            var value = _readBuffer[_readPosition];
            MoveReadPosition(peek, 1);
            return value;
        }

        public byte[] ReadBytes(int length, bool peek = true)
        {
            Read();
            var value = _bufferList.GetRange(_readPosition, length).ToArray();
            MoveReadPosition(peek, length);
            return value;
        }

        public string ReadString(bool peek = true)
        {
            var length = ReadInteger();
            Read();
            var value = Encoding.ASCII.GetString(_readBuffer, _readPosition, length);
            MoveReadPosition(peek, length);
            return value;
        }

        public bool ReadBool(bool peek = true)
        {
            CheckLimitBuffer();
            Read();
            var value = BitConverter.ToBoolean(_readBuffer, _readPosition);
            MoveReadPosition(peek, 1);
            return value;
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _bufferList.Clear();
                }

                _readPosition = 0;
            }

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}