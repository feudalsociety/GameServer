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

        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            // 추가로 넘겨주고 싶은 정보가 있다면
            // recvArgs.UserToken = 1; // object type
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            RegisterRecv(recvArgs);
        }

        // Send는 Recieve와 같이 예약 불가능, 원하는 타이밍에 호출해야하기 떄문에
        public void Send(byte[] sendBuff)
        {
            _socket.Send(sendBuff);
        }

        // 동시 다발적으로 disconnect를 한다던가, 같은애가 2번하면? - error 한번만 하게끔
        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;
            // if(_socket != null) // ... multithread 환경에서 안전하지 않다.
            _socket.Shutdown(SocketShutdown.Both); 
            _socket.Close();
            // _socket = null;
        }

        #region 네트워크 통신

        void RegisterRecv(SocketAsyncEventArgs args)
        {
            // non-blocking version
            bool pending = _socket.ReceiveAsync(args);
            if(pending == false) OnRecvCompleted(null, args);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 0바이트로 오는경우, 상대방이 연결이 끊어진 경우
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                // TODO
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
                // TODO Disconnect
            }
        }

        #endregion
    }
}