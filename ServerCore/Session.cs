using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Session
    {
        // 일정한 시간동안 몇 byte들 보냈는지 추적해서 심하게 많이 보내면 쉬면서 하는게 좋긴함
        // 아니면 유저가 보내는 데이터가 비정상적이면 recieve할때 check해서 disconnet하는 기능이 추가되야함
        // 패킷 모아보내기는 이것보다 더 많은 작업이 필요
        // 패킷 모으는것을 서버 엔진에서 할것인지, 컨텐츠에서 모아서 send를 한번만 요청할 것인지

        Socket _socket;
        int _disconnected = 0;

        object _lock = new object(); 
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        // bool _pending = false;
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public void Start(Socket socket)
        {
            _socket = socket;
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }


        public void Send(byte[] sendBuff)
        {
            lock(_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if(_pendingList == 0) RegisterSend(); // if(_pending == false)
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;
           
            _socket.Shutdown(SocketShutdown.Both); 
            _socket.Close();
        }

        #region 네트워크 통신

        void RegisterSend() 
        {
            // _pending = true; _pendinglist가 대신해줌
            _pendingList.Clear(); // 굳이 필요없는 부분이긴 함

            // byte[] buff = _sendQueue.Dequeue();
            // _sendArgs.SetBuffer(buff, 0, buff.Length);

            // BufferList로 한번에 보내면 좀더 효율적, setbuffer이나 list 둘중하나만 사용해야함
            while(_sendQueue.Count > 0)
            {
                buff = _sendQueue.Dequeue();
                // ArraySegment structure으로 heap이 아닌 stack에 할당됨, 실제 add할때 값이 복사되는 형태
                // c# 같은경우 포인터를 사용하기 어려움으로 이렇게 넘겨줌
                // bufferList에 넣을 때 완성된 list 형태로 넣어줘야함
                // _sendArgs.BufferList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }

            _sendArgs.BufferList = _pendingList;
            
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false) OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock(_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        // null을 굳이 넣을 필요는 없긴함
                        _sendArgs.BufferList = null; 
                        _pendingList.Clear();

                        Console.WriteLine($"Transferred bytes : {_sendArgs.BytesTransferred}");

                        if(_sendQueue.Count > 0) // 처리하는 도중에 누가도 패킷을 넣었으면
                            RegisterSend();
                        // else _pending = false;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed : {e.Message}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegisterRecv() 
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if(pending == false) OnRecvCompleted(null, _recvArgs);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, 0, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");
                    RegisterRecv();
                }
                catch(Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed : {e.Message}");
                }
            }
            else
            {
                Disconnect();
            }
        }

        #endregion
    }
}