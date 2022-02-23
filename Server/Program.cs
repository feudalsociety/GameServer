using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;


namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        static void FlushRoom()
        {
            Room.Push(() => Room.Flush());
            JobTimer.Instance.Push(FlushRoom, 250); // 예약
        }

        static void Main(string[] args)
        {
            // PacketManager.Instance.Register();

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 개선을 하자면 중간에 SessionManager를 둬서 만들어주도록한다. session id나 count 관리
            // Manager는 core에 들어가도되고 컨텐츠단에서 관리해도 됨
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            //FlushRoom(); // 예약
            JobTimer.Instance.Push(FlushRoom);
            // 다른 thread에서 JobTimer라는 중앙관리 시스템에 일감을 던져서 예약하게될것

            while(true)
            {
                // 예약된거 처리
                JobTimer.Instance.Flush();
            }
        }
    }
}

// dll를 빌드해서 그대로 unity에 옮기면 디버깅할 때 힘들다.
// 처음에는 코드를 수동으로 가지고 온다음에 걸러내는게 좋다.