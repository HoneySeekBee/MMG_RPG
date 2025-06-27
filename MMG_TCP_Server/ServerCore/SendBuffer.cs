using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBuffer
    {
        const int BufferSize = 4096; 
        static ThreadLocal<SendBuffer> _currentBuffer = new ThreadLocal<SendBuffer>(() => new SendBuffer());

        private byte[] _buffer = new byte[BufferSize];
        private int _usedSize = 0;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > BufferSize)
                throw new ArgumentOutOfRangeException();

            if (_currentBuffer.Value._usedSize + reserveSize > BufferSize)
                _currentBuffer.Value = new SendBuffer();

            return new ArraySegment<byte>(_currentBuffer.Value._buffer, _currentBuffer.Value._usedSize, reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            var buffer = _currentBuffer.Value;
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer._buffer, buffer._usedSize, usedSize);
            buffer._usedSize += usedSize;
            return segment;
        }
    }
}
