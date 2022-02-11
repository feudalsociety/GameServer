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
        static Listener _listener = new Listener();

        static void onAcceptHandler(Socket clientSocket)
        {
            try
            {
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
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); 

            
            _listener.init(endPoint, onAcceptHandler);
            Console.WriteLine("Listening...");


            while (true)
            {
                // // 손님을 입장시킨다.
                // Socket clientSocket = _listener.Accept();
            }
        }
    }
}
