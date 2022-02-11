using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace ServerCore
{
    class Listener
    {
        Socket _listenSocket;
        Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;

            // 문지기 교육
            _listenSocket.Bind(endPoint);

            // 영업 시작
            // backlog : 최대 대기수
            _listenSocket.Listen(10);

            // 한번만 만들면 재사용 가능, 동시다발적으로 많은 유저를 받아야할 때 이부분을 for문을 걸어 늘려준다.
            SocketAsyncEventArgs args = new SocketAsyncEventArgs(); 
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // SocketAsyncEventArgs 초기화, 기존의 잔재를 없앰
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false) // 동시다발적으로 계속 false만 나오는 경우는 거의 없다.
                OnAcceptCompleted(null, args);
        }

        // 콜백함수는 별도의 Thread를 이용, ThreadPool에서 뽑아서 실행 main과 race condition이 일어나지 않도록 주의
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                // TODO
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else 
                Console.WriteLine(args.SocketError.ToString());

            // stackoverflow가 일어나지 않는다.
            RegisterAccept(args);
        }
    }
}
