using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    // client가 보내는정보가 각기 다르기 떄문에 RecvBuffer는 session마다 고유의 buffer를 가지고 있음 
    // Sendbuffer는 외부에 있다.
    // Sendbuffer를 내부에서 복사를 하는 방식을 채택하면 성능적 문제가 있음.
    // 외부에서 buffer를 만들어준다음에 꽂아넣는 방식이 복사의 횟수가 현저히 줄어든다. 

    // _usedSize를 초기화하기 까다로움 - 한명한테 보내는게 아니라서 이전에 있던 부분을 다른 세션에서 send를하기 위해 Sendqueue에 넣어논 상태일 수 있기때문에
    // 1회용으로만 사용

     public class SendBufferHelper
    {
        // 전역을 만들면 thread끼리 서로 경합하니까 ThreadLocal 이용, 맨처음에 만들어줄때 무엇을 할것인지 람다로 전달
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });
        public static int ChunkSize { get; set; } = 4096;
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
        // [][][][u][][][][][][]
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
