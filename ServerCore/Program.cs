using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
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

            // 문지기
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // ipv4, or ipv6 / TCP 

            try
            {
                // 문지기 교육
                listenSocket.Bind(endPoint);

                // 영업 시작
                // backlog : 최대 대기수
                listenSocket.Listen(10);

                while (true)
                {
                    Console.WriteLine("Listening...");

                    // 손님을 입장시킨다. 클라이언트가 입장하지 않을경우? - blocking/non-blocking 
                    // 다음단계로 넘어가지 않고 계속 대기한다.
                    Socket clientSocket = listenSocket.Accept();

                    // 받는다
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Client] {recvData}");

                    // 보낸다
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server!");
                    clientSocket.Send(sendBuff); // Send도 저쪽에서 안받으면 대기

                    // 쫒아낸다.
                    clientSocket.Shutdown(SocketShutdown.Both); // 옵션 : 더이상 듣기도 싫고 말하기도 싫다 
                    clientSocket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }
}
