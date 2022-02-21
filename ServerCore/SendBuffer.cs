using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
     public class SendBufferHelper
    {
        // 전역을 만들면 thread끼리 서로 경합하니까 ThreadLocal 이용, 맨처음에 만들어줄때 무엇을 할것인지 람다로 전달
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });
        public static int ChunkSize { get; set; } = 65535 * 100;
        public static ArraySegment<byte> Open(int reserveSize)
        {
            if(CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize); // 기존에 있던걸 날리고 교체
            // c++이라면 여기서 날리지 않고 세세하게 컨트롤 가능, reference counting, 참조하는게 없으면 해제하지 않고 pool에 반환해서 재사용

            return CurrentBuffer.Value.Open(reserveSize);
        }
        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        byte[] _buffer;
        int _usedSize = 0;

        public int FreeSize { get { return _buffer.Length - _usedSize; } }
        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }
        public ArraySegment<byte> Open(int reserveSize) // 예약
        {
            if (reserveSize > FreeSize)
                return null;
            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }
        public ArraySegment<byte> Close(int usedSize) // 실제로 사용
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }
    }
}
