using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace dummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 고객의 휴대폰 설정, tcp와 stream은 세트
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                // 문지기에게 입장 요청, 게임에서는 blocking 계열을 쓰면 안됨
                socket.Connect(endPoint);
                Console.WriteLine($"Connected to {socket.RemoteEndPoint.ToString()}");

                // 보낸다
                byte[] sendBuff = Encoding.UTF8.GetBytes("Hello World");
                int sendBytes = socket.Send(sendBuff); // 여기서도 blocking 계열로 계속 대기하면 안됨

                // 받는다
                byte[] recvBuff = new byte[1024];
                int recvBytes = socket.Receive(recvBuff); // 서버가 보내줄 때까지 계속 대기
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                Console.WriteLine($"[From Server] {recvData}");

                // 나간다
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
