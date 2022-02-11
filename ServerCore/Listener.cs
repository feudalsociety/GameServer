using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace ServerCore
{
    class Listener
    {
        Socket listenSocket;

        public void init(IPEndPoint endPoint)
        {
            listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
