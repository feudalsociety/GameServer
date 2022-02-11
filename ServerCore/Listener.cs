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

        public void init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;

            // 문지기 교육
            _listenSocket.Bind(endPoint);

            // 영업 시작
            // backlog : 최대 대기수
            _listenSocket.Listen(10);

            // 한번만 만들면 재사용 가능
            SocketAsyncEventArgs args = new SocketAsyncEventArgs(); 
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAceeptCompleted);
            RegisterAccept(args);
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // SocketAsyncEventArgs 초기화, 기존의 잔재를 없앰
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)
                OnAceeptCompleted(null, args);
        }

        void OnAceeptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                // TODO
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else 
                Console.WriteLine(args.SocketError.ToString());

            // SocketAsyncEventArgs 재사용 다음번을 위해서
            RegisterAccept(args);
        }

        // public void Accept()
        // {
        //     // 비동기, 성공하든 아니든 바로 return
        //     // 실패해도 바로 빠져나오기 때문에 유저가 접속 요청을하면 알려줘야함 callback
        //     // accept 요청 부분과 실제로 처리 부분이 둘로 분리
        //     // _listenSocket.AcceptAsync( );

        //     return _listenSocket.Accept();
        // }
    }
}
