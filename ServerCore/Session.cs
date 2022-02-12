using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object(); // send를 multithread 환경에서 실행하기 위한 lock
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        bool _pending = false; // pending이 false 일경우 queue에만 쌓는다.
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs(); // 낚시대 하나만 사용

        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv(recvArgs);
        }

        // Send는 Recieve와 같이 예약 불가능 정해진 시점이 없음, 원하는 타이밍에 호출해야하기 떄문에
        // 동시다발적으로 send를 한다면? - 다행히 sendAsync가 동시다발적으로 호출된다고 뻑나는 함수는 아님
        public void Send(byte[] sendBuff)
        {
            // _socket.Send(sendBuff);
            // 매번 event를 만드는것보다 재사용하면 좋을것 같다
            // SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
            // sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            // SendAsync가 끝나고 OnSendCompleted가 완료되기전까지 queue에만 쌓아놓고 완료되었으면 queue를 비우는 방식

            lock(_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if(_pending == false) RegisterSend();
            }

            // sendArgs.SetBuffer(sendBuff, 0, sendBuff.Length);
            // RegisterSend();
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;
           
            _socket.Shutdown(SocketShutdown.Both); 
            _socket.Close();
        }

        #region 네트워크 통신

        void RegisterSend() // void RegisterSend(SocketAsyncEventArgs args)
        {
            _pending = true;
            // user mode에서 network 패킷을 보내는것은 불가능하고 kernal에서 처리하는 것이기 때문에 쉽게 막쓰는건 문제가 있다.
            byte[] buff = _sendQueue.Dequeue();
            _sendArgs.SetBuffer(buff, 0, buff.Length);

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false) OnSendCompleted(null, _sendArgs);
        }

        // RegisterSend 뿐만아니라 다른 Thread에서 callback 방식으로 호출될 수 도 있다. 그래서 lock이 필요함
        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock(_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        if(_sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                        else _pending = false;
                        // RegisterSend(args); 재사용 불가능
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

        void RegisterRecv(SocketAsyncEventArgs args) 
        {
            // non-blocking version
            bool pending = _socket.ReceiveAsync(args);
            if(pending == false) OnRecvCompleted(null, args);
        }

        // Thread가 동시다발적으로 여기에 들어오는 경우는 없다. event가 하나밖에 없음
        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, 0, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");
                    RegisterRecv(args);
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