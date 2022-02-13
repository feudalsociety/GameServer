using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecvBuffer
    {
        // [r][][][][w][][][][][]
        // byte 배열로 들고있어도 상관없지만 부분적으로 잘라서 사용하고 싶을 수 도 있으므로 
        ArraySegment<byte> _buffer;
        int _readPos; // 패킷이 완성되기 전까지는 처리하지 않음
        int _writePos;

        public RecvBuffer(int buffersize)
        {
            _buffer = new ArraySegment<byte>(new byte[buffersize], 0, buffersize);
        }

        public int DataSize { get { return _writePos - _readPos; } }
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        public ArraySegment<byte> ReadSegment // 유효범위의 segment를 컨텐츠단에 넘겨줌
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }

        public ArraySegment<byte> WriteSegment // 다음에 recv할때 어디서부터 어디까지가 유효범위인지
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            if (dataSize == 0) // 남은 데이터가 없으면 복사하지 않고 커서 위치만 옮김
            {
                _readPos = _writePos = 0; 
            }
            else
            {
                // 남은 찌끄레기가 있으면 시작 위치로 복사
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        // 성공적으로 처리했으면 OnRead호출
        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;

            _readPos += numOfBytes;
            return true;
        }

        // Client에서 데이터를 쏴서 Recieve했을 때 write cursor를 이동시키는 부분
        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }
    }
}
