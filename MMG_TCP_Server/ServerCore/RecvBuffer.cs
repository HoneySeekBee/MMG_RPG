using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecvBuffer
    {
        private byte[] _buffer;
        private int _readPos;
        private int _writePos;

        public RecvBuffer(int bufferSize)
        {
            _buffer = new byte[bufferSize];
        }

        public int DataSize => _writePos - _readPos;
        public int FreeSize => _buffer.Length - _writePos;

        public ArraySegment<byte> ReadSegment => new ArraySegment<byte>(_buffer, _readPos, DataSize);
        public ArraySegment<byte> WriteSegment => new ArraySegment<byte>(_buffer, _writePos, FreeSize);

        public void OnWrite(int bytes)
        {
            _writePos += bytes;
        }

        public void OnRead(int bytes)
        {
            _readPos += bytes;

            if (_readPos == _writePos)
            {
                // 모든 데이터 처리됨
                _readPos = 0;
                _writePos = 0;
            }
        }
    }
}
